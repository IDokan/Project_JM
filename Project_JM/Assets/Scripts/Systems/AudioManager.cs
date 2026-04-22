// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 16/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AudioManager.cs
// Summary: Singleton audio manager that persists across scenes. Manages two BGM sources
//          for crossfading and three SFX pools (UI, Puzzle, Action) with per-category
//          mixer group routing. BGM fades use unscaled time.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup uiSfxMixerGroup;
    [SerializeField] private AudioMixerGroup puzzleSfxMixerGroup;
    [SerializeField] private AudioMixerGroup actionSfxMixerGroup;

    [SerializeField] private int uiSfxPoolSize = 4;
    [SerializeField] private int puzzleSfxPoolSize = 4;
    [SerializeField] private int actionSfxPoolSize = 8;

    private AudioSource _musicSourceA;
    private AudioSource _musicSourceB;
    private Coroutine _fadeCoroutineA;
    private Coroutine _fadeCoroutineB;
    private bool _sourceAIsCurrent = true;

    private AudioSource[] _uiSfxPool;
    private AudioSource[] _puzzleSfxPool;
    private AudioSource[] _actionSfxPool;

    private float[] _uiSfxStartTimes;
    private float[] _puzzleSfxStartTimes;
    private float[] _actionSfxStartTimes;

    private AudioSource CurrentMusicSource => _sourceAIsCurrent ? _musicSourceA : _musicSourceB;
    private AudioSource OtherMusicSource => _sourceAIsCurrent ? _musicSourceB : _musicSourceA;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicSourceA = CreateMusicSource("MusicSourceA");
        _musicSourceB = CreateMusicSource("MusicSourceB");

        _uiSfxPool = CreatePool(uiSfxPoolSize, "UISfx", uiSfxMixerGroup);
        _puzzleSfxPool = CreatePool(puzzleSfxPoolSize, "PuzzleSfx", puzzleSfxMixerGroup);
        _actionSfxPool = CreatePool(actionSfxPoolSize, "ActionSfx", actionSfxMixerGroup);

        _uiSfxStartTimes = new float[uiSfxPoolSize];
        _puzzleSfxStartTimes = new float[puzzleSfxPoolSize];
        _actionSfxStartTimes = new float[actionSfxPoolSize];
    }

    // ── BGM ──────────────────────────────────────────────────────────────────

    public void PlayBGM(AudioCueSO cue)
    {
        if (cue == null) return;

        StopAllFadeCoroutines();
        _musicSourceA.Stop();
        _musicSourceB.Stop();
        _musicSourceA.volume = 0f;
        _musicSourceB.volume = 0f;

        _sourceAIsCurrent = true;
        ConfigureMusicSource(_musicSourceA, cue);
        _musicSourceA.volume = cue.Volume;
        _musicSourceA.Play();
    }

    public void StopBGM()
    {
        StopAllFadeCoroutines();
        _musicSourceA.Stop();
        _musicSourceB.Stop();
        _musicSourceA.volume = 0f;
        _musicSourceB.volume = 0f;
    }

    public void FadeOutBGM(float duration)
    {
        StopFadeCoroutine(_sourceAIsCurrent);
        if (_sourceAIsCurrent)
            _fadeCoroutineA = StartCoroutine(FadeCoroutine(_musicSourceA, 0f, duration));
        else
            _fadeCoroutineB = StartCoroutine(FadeCoroutine(_musicSourceB, 0f, duration));
    }

    public void FadeInBGM(AudioCueSO cue, float duration)
    {
        if (cue == null) return;

        StopFadeCoroutine(!_sourceAIsCurrent);
        AudioSource target = OtherMusicSource;
        target.Stop();
        ConfigureMusicSource(target, cue);
        target.volume = 0f;
        target.Play();

        if (!_sourceAIsCurrent)
            _fadeCoroutineA = StartCoroutine(FadeCoroutine(_musicSourceA, cue.Volume, duration));
        else
            _fadeCoroutineB = StartCoroutine(FadeCoroutine(_musicSourceB, cue.Volume, duration));

        _sourceAIsCurrent = !_sourceAIsCurrent;
    }

    // ── SFX ──────────────────────────────────────────────────────────────────

    public void PauseScaledSFX()
    {
        foreach (var source in _puzzleSfxPool) source.Pause();
        foreach (var source in _actionSfxPool) source.Pause();
    }

    public void ResumeScaledSFX()
    {
        foreach (var source in _puzzleSfxPool) source.UnPause();
        foreach (var source in _actionSfxPool) source.UnPause();
    }

    public void PlayUISFX(AudioCueSO cue) => PlayOnPool(_uiSfxPool, _uiSfxStartTimes, cue);
    public void PlayPuzzleSFX(AudioCueSO cue) => PlayOnPool(_puzzleSfxPool, _puzzleSfxStartTimes, cue);
    public void PlayPuzzleSFX(AudioCueSO cue, int clipIndex) => PlayOnPool(_puzzleSfxPool, _puzzleSfxStartTimes, cue, clipIndex);
    public void PlayActionSFX(AudioCueSO cue) => PlayOnPool(_actionSfxPool, _actionSfxStartTimes, cue);

    // ── Private helpers ───────────────────────────────────────────────────────

    private void PlayOnPool(AudioSource[] pool, float[] startTimes, AudioCueSO cue)
    {
        if (cue == null) return;
        PlayOnPoolWithClip(pool, startTimes, cue, cue.GetClip());
    }

    private void PlayOnPool(AudioSource[] pool, float[] startTimes, AudioCueSO cue, int clipIndex)
    {
        if (cue == null) return;
        PlayOnPoolWithClip(pool, startTimes, cue, cue.GetClip(clipIndex));
    }

    private void PlayOnPoolWithClip(AudioSource[] pool, float[] startTimes, AudioCueSO cue, AudioClip clip)
    {
        if (clip == null) return;

        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].isPlaying) continue;

            PlayOnSource(pool[i], cue, clip);
            startTimes[i] = Time.unscaledTime;
            return;
        }

        // All sources busy — stop the oldest and reuse it.
        int oldestIndex = 0;
        for (int i = 1; i < pool.Length; i++)
        {
            if (startTimes[i] < startTimes[oldestIndex])
                oldestIndex = i;
        }

        pool[oldestIndex].Stop();
        PlayOnSource(pool[oldestIndex], cue, clip);
        startTimes[oldestIndex] = Time.unscaledTime;
    }

    private void PlayOnSource(AudioSource source, AudioCueSO cue, AudioClip clip)
    {
        source.clip = clip;
        source.volume = cue.Volume;
        source.pitch = cue.GetPitch();
        source.loop = cue.Loop;
        source.Play();
    }

    private IEnumerator FadeCoroutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;

        if (targetVolume <= 0f)
            source.Stop();
    }

    private void StopFadeCoroutine(bool isMusicSourceA)
    {
        if (isMusicSourceA && _fadeCoroutineA != null)
        {
            StopCoroutine(_fadeCoroutineA);
            _fadeCoroutineA = null;
        }
        else if (!isMusicSourceA && _fadeCoroutineB != null)
        {
            StopCoroutine(_fadeCoroutineB);
            _fadeCoroutineB = null;
        }
    }

    private void StopAllFadeCoroutines()
    {
        if (_fadeCoroutineA != null) { StopCoroutine(_fadeCoroutineA); _fadeCoroutineA = null; }
        if (_fadeCoroutineB != null) { StopCoroutine(_fadeCoroutineB); _fadeCoroutineB = null; }
    }

    private void ConfigureMusicSource(AudioSource source, AudioCueSO cue)
    {
        source.clip = cue.GetClip();
        source.pitch = cue.GetPitch();
        source.loop = cue.Loop;
    }

    private AudioSource CreateMusicSource(string sourceName)
    {
        GameObject go = new GameObject(sourceName);
        go.transform.SetParent(transform);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.outputAudioMixerGroup = bgmMixerGroup;
        return source;
    }

    private AudioSource[] CreatePool(int size, string poolName, AudioMixerGroup mixerGroup)
    {
        AudioSource[] pool = new AudioSource[size];
        for (int i = 0; i < size; i++)
        {
            GameObject go = new GameObject($"{poolName}_{i}");
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.outputAudioMixerGroup = mixerGroup;
            pool[i] = source;
        }
        return pool;
    }
}
