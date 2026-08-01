// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/16/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MiddleTransitionController.cs
// Summary: A script to manage middle transition logic.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
//                      Middle transition conducts below tasks:
//                                          1. Starts moving camera.
//                                          2. Spawn next enemy.

using System.Collections;
using UnityEngine;

public class MiddleTransitionController : TransitionController
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected float cameraStartDelay = 1f;
    [SerializeField] protected float enemySpawnDelay = 3f;
    [SerializeField] protected float middleTransitionDuration = 5f;

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.RewardTransitionEnd)
        {
            RequestTransitionStart(BeginMiddleTransition);
        }
    }

    protected void BeginMiddleTransition()
    {
        StartCoroutine(MiddleTransitionRoutine());
    }

    protected IEnumerator MiddleTransitionRoutine()
    {
        transitionEventChannel.Raise(TransitionPhase.MiddleTransitionStarts);

        yield return new WaitForSeconds(cameraStartDelay);
        transitionEventChannel.Raise(TransitionPhase.MiddleCameraMoveBegin);

        yield return new WaitForSeconds(enemySpawnDelay);
        transitionEventChannel.Raise(TransitionPhase.MiddleEnemySpawnBegin);

        yield return new WaitForSeconds(middleTransitionDuration - cameraStartDelay - enemySpawnDelay);
        transitionEventChannel.Raise(TransitionPhase.MiddleTransitionEnd);

        CompleteTransition();
    }
}
