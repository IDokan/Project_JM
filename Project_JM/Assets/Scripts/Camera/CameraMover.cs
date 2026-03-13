// SPDX-License-Identifier: MIT
// Copyright (c) 02/23/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CameraMover.cs
// Summary: A script to move camera.

using UnityEngine;
using System.Collections;

public class CameraMover : MonoBehaviour
{
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected IntroEventChannel introEventChannel;

    [SerializeField] protected float moveDelay = 1f;
    [SerializeField] protected float moveDistance = 9f;
    [SerializeField] protected float moveDuration = 3f;

    protected void OnEnable()
    {
        characterDeathEventChannel.OnRaised += OnCharacterDied;

        if (introEventChannel != null)
        {
            introEventChannel.OnRaised += OnIntroEvent;
        }
    }

    protected void OnDisable()
    {
        characterDeathEventChannel.OnRaised -= OnCharacterDied;
        if (introEventChannel != null)
        {
            introEventChannel.OnRaised -= OnIntroEvent;
        }
    }

    protected void OnCharacterDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            Move(moveDistance, moveDuration, moveDelay);
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

    protected void OnIntroEvent(IntroSequencePhase phase)
    {
        if (phase == IntroSequencePhase.PartyMoveEnd)
        {
            Move(moveDistance / moveDuration * (moveDuration + moveDelay), moveDuration + moveDelay, 0);
        }
    }
}
