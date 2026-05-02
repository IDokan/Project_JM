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

    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    private AudioSource _currentMusicSource;
    private AudioSource _otherMusicSource;
    private Coroutine _currentFadeCoroutine;
    private Coroutine _otherFadeCoroutine;

    private AudioSource[] _uiSfxPool;
    private AudioSource[] _puzzleSfxPool;
    private AudioSource[] _actionSfxPool;

    private float[] _uiSfxStartTimes;
    private float[] _puzzleSfxStartTimes;
    private float[] _actionSfxStartTimes;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _currentMusicSource = CreateMusicSource("MusicSourceA");
        _otherMusicSource = CreateMusicSource("MusicSourceB");

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
        _currentMusicSource.Stop();
        _otherMusicSource.Stop();
        _currentMusicSource.volume = 0f;
        _otherMusicSource.volume = 0f;

        ConfigureMusicSource(_currentMusicSource, cue);
        _currentMusicSource.volume = cue.Volume;
        _currentMusicSource.Play();
    }

    public void StopBGM()
    {
        StopAllFadeCoroutines();
        _currentMusicSource.Stop();
        _otherMusicSource.Stop();
        _currentMusicSource.volume = 0f;
        _otherMusicSource.volume = 0f;
    }

    public void FadeOutBGM(float duration)
    {
        if (!_currentMusicSource.isPlaying)
        {
            return;
        }
        if (_currentFadeCoroutine != null)
        {
            StopCoroutine(_currentFadeCoroutine);
        }
        _currentFadeCoroutine = StartCoroutine(FadeCoroutine(_currentMusicSource, 0f, duration, fadeCurve));
    }

    public void FadeInBGM(AudioCueSO cue, float duration)
    {
        if (cue == null) return;

        if (_otherFadeCoroutine != null)
        {
            StopCoroutine(_otherFadeCoroutine);
        }
        _otherMusicSource.Stop();
        ConfigureMusicSource(_otherMusicSource, cue);
        _otherMusicSource.volume = 0f;
        _otherMusicSource.Play();
        FadeOutBGM(duration);
        _otherFadeCoroutine = StartCoroutine(FadeCoroutine(_otherMusicSource, cue.Volume, duration, fadeCurve));

        SwapMusicSources();
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

    private IEnumerator FadeCoroutine(AudioSource source, float targetVolume, float duration, AnimationCurve curve)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, curve.Evaluate(elapsed / duration));
            yield return null;
        }

        source.volume = targetVolume;

        if (targetVolume <= 0f)
            source.Stop();
    }

    private void SwapMusicSources()
    {
        (_currentMusicSource, _otherMusicSource) = (_otherMusicSource, _currentMusicSource);
        (_currentFadeCoroutine, _otherFadeCoroutine) = (_otherFadeCoroutine, _currentFadeCoroutine);
    }

    private void StopAllFadeCoroutines()
    {
        if (_currentFadeCoroutine != null) { StopCoroutine(_currentFadeCoroutine); _currentFadeCoroutine = null; }
        if (_otherFadeCoroutine != null) { StopCoroutine(_otherFadeCoroutine); _otherFadeCoroutine = null; }
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
