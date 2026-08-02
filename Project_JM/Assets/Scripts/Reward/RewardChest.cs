// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardChest.cs
// Summary: A purely visual reward chest prop with no transition-event
//          knowledge of its own; RewardChestManager tells it when to show
//          and hide via Show/Hide. One instance is reused for every enemy
//          defeat rather than being instantiated fresh each time.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class RewardChest : MonoBehaviour
{
    [SerializeField] protected float exitDistance = 6f;
    [SerializeField] protected float exitDuration = 3f;

    protected Coroutine _exitRoutine;

    // Called by RewardChestManager when the reward transition starts.
    public void Show(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
    }

    // Called by RewardChestManager when the middle transition starts; slides
    // the chest off-screen and deactivates it once the slide finishes.
    public void Hide()
    {
        if (_exitRoutine != null)
        {
            StopCoroutine(_exitRoutine);
        }

        _exitRoutine = StartCoroutine(ExitRoutine());
    }

    protected IEnumerator ExitRoutine()
    {
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
        _exitRoutine = null;
        gameObject.SetActive(false);
    }
}
