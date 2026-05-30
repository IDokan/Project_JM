// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/11/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyDefeatedExit.cs
// Summary: Moves a defeated enemy downward off screen when the middle transition begins.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class EnemyDefeatedExit : MonoBehaviour
{
    [SerializeField] private TransitionEventChannel transitionEventChannel;
    [SerializeField] private CharacterStatus characterStatus;

    [SerializeField] private float exitDelay = 1f;
    [SerializeField] private float exitDistance = 10f;
    [SerializeField] private float exitDuration = 1f;

    private void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionPhase;
    }

    private void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionPhase;
    }

    private void OnTransitionPhase(TransitionPhase phase)
    {
        if (phase == TransitionPhase.MiddleCameraMoveBegin && characterStatus.IsDead)
        {
            StartCoroutine(ExitRoutine());
        }
    }

    private IEnumerator ExitRoutine()
    {
        yield return GlobalTimeManager.WaitForGlobalSeconds(exitDelay);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * exitDistance;

        float t = 0f;
        while (t < exitDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / exitDuration);
            yield return null;
        }

        transform.position = end;
    }
}
