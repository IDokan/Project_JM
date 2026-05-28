// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackTimerUI.cs
// Summary: Plays a particle effect while the Mage's T5 time-slow is active.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class AttackTimerUI : MonoBehaviour
{
    [SerializeField] private ParticleSystem slowParticles;

    protected void OnEnable()  => GlobalTimeManager.OnScaleChanged += OnTimeScaleChanged;
    protected void OnDisable() => GlobalTimeManager.OnScaleChanged -= OnTimeScaleChanged;

    private void OnTimeScaleChanged(float newScale)
    {
        if (newScale < 1f)
            slowParticles.Play();
        else
            slowParticles.Stop();
    }
}
