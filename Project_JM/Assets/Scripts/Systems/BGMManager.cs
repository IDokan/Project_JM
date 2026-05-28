// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BGMManager.cs
// Summary: Listens to TransitionEventChannel and drives BGM transitions via AudioManager.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class BGMManager : MonoBehaviour
{
    [SerializeField] private TransitionEventChannel transitionEventChannel;

    [SerializeField] private AudioCueSO defaultBGMCue;

    [SerializeField] private float bgmFadeInDuration = 1f;
    [SerializeField] private float bgmFadeOutDuration = 1f;

    private void OnEnable() => transitionEventChannel.OnRaised += OnTransitionPhase;
    private void OnDisable() => transitionEventChannel.OnRaised -= OnTransitionPhase;

    private void OnTransitionPhase(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            AudioManager.Instance.FadeInBGM(defaultBGMCue, bgmFadeInDuration);
        }
        else if (phase == TransitionPhase.EndTransitionBegin)
        {
            AudioManager.Instance.FadeOutBGM(bgmFadeOutDuration);
        }
    }
}
