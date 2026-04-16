// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 16/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AudioCueSO.cs
// Summary: ScriptableObject describing a single audio cue — clips for variation,
//          volume, pitch range, and loop flag. Mixer group routing is deferred
//          to AudioManager per category.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioCue", menuName = "JM/Audio/Audio Cue")]
public class AudioCueSO : ScriptableObject
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] private float volume = 1f;
    [SerializeField] private float pitchMin = 0.9f;
    [SerializeField] private float pitchMax = 1.1f;
    [SerializeField] private bool loop = false;

    public float Volume => volume;
    public bool Loop => loop;
    public int ClipCount => clips != null ? clips.Length : 0;

    public AudioClip GetClip() =>
        clips is { Length: > 0 } ? clips[Random.Range(0, clips.Length)] : null;

    public AudioClip GetClip(int index) =>
        clips is { Length: > 0 } ? clips[Mathf.Clamp(index, 0, clips.Length - 1)] : null;

    public float GetRandomPitch() => Random.Range(pitchMin, pitchMax);
}
