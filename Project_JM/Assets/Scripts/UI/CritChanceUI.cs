// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CritChanceUI.cs
// Summary: Binds CharacterStatus crit chance to a sprite animator and a TMP label.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TMPro;
using UnityEngine;

public class CritChanceUI : MonoBehaviour
{
    [SerializeField] private CharacterStatus characterStatus;
    [SerializeField] private IndexedSpriteAnimator spriteAnimator;
    [SerializeField] private TextMeshProUGUI critChanceLabel;

    [SerializeField] private ParticleSystem[] particleSystems;
    [SerializeField] private ParticleSystem buffParticle;

    private void OnEnable()
    {
        characterStatus.OnCritChanceChanged += OnCritChanceChanged;
        characterStatus.OnCriticalHit += OnCriticalHit;
        characterStatus.OnCritTimedBuffStarted += OnCritBuffStarted;
        characterStatus.OnCritTimedBuffStopped += OnCritBuffStopped;
    }

    private void OnDisable()
    {
        characterStatus.OnCritChanceChanged -= OnCritChanceChanged;
        characterStatus.OnCriticalHit -= OnCriticalHit;
        characterStatus.OnCritTimedBuffStarted -= OnCritBuffStarted;
        characterStatus.OnCritTimedBuffStopped -= OnCritBuffStopped;
    }

    private void Start()
    {
        OnCritChanceChanged(characterStatus.CriticalChance);
    }

    private void OnCritChanceChanged(float critChance)
    {
        critChanceLabel.text = $"{Mathf.RoundToInt(critChance * 100f)}%";
    }

    private void OnCriticalHit()
    {
        spriteAnimator.NextFrame();
        PlayParticles();
    }

    private void PlayParticles()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }
    }

    private void OnCritBuffStarted() => buffParticle.Play();

    private void OnCritBuffStopped() => buffParticle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
}
