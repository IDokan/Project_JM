// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/23/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CameraMover.cs
// Summary: A script to move camera.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using System.Collections;

public class CameraMover : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected float moveDelay = 1f;
    [SerializeField] protected float moveDistance = 9f;
    [SerializeField] protected float moveDuration = 3f;

    [SerializeField] protected float introMoveDuration = 1f;

    protected void OnEnable()
    {
        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised += OnTransitionEvent;
        }
    }

    protected void OnDisable()
    {
        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised -= OnTransitionEvent;
        }
    }

    public Coroutine Move(float distance, float seconds, float moveDelay)
    {
        Vector3 start = transform.position;
        Vector3 end = start + distance * Vector3.right;

        return StartCoroutine(MoveRoutineAfterDuration(start, end, seconds, moveDelay));
    }

    protected IEnumerator MoveRoutineAfterDuration(Vector3 start, Vector3 end, float seconds, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (seconds <= 0f)
        {
            transform.position = end;
            yield break;
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / seconds;
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroPartyMoveEnd)
        {
            Move(moveDistance * introMoveDuration / moveDuration, introMoveDuration, 0);
        }
        else if (phase == TransitionPhase.MiddleCameraMoveBegin)
        {
            Move(moveDistance, moveDuration, moveDelay);
        }
    }
}
