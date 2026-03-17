// SPDX-License-Identifier: MIT
// Copyright (c) 03/16/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MiddleTransitionController.cs
// Summary: A script to manage middle transition logic.
//                      Middle transition conducts below tasks:
//                                          1. Starts moving camera.
//                                          2. Spawn next enemy.

using System.Collections;
using UnityEngine;

public class MiddleTransitionController : TransitionController
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;

    [SerializeField] protected float middleTransitionDuration = 5f;


    protected void OnEnable()
    {
        characterDeathEventChannel.OnRaised += OnAnyoneDied;
    }

    protected void OnDisable()
    {
        characterDeathEventChannel.OnRaised -= OnAnyoneDied;
    }

    protected void OnAnyoneDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            StartCoroutine(MiddleTransitionRoutine());
        }
    }

    protected IEnumerator MiddleTransitionRoutine()
    {
        RaiseStarted();

        transitionEventChannel.Raise(TransitionPhase.MiddleTransitionStarts);

        yield return new WaitForSeconds(middleTransitionDuration);

        RaiseCompleted();
    }
}
