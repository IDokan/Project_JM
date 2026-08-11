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
    [SerializeField] protected Animator chestAnimator;

    [SerializeField] protected float entryDuration = 0.6f;
    [SerializeField] protected float entryArcHeight = 2f;
    [SerializeField] protected float entryOffscreenMargin = 1f;
    [SerializeField] protected float openDelay = 0.5f;

    [SerializeField] protected float exitDistance = 6f;
    [SerializeField] protected float exitDuration = 3f;

    protected static readonly int OpenTrig = Animator.StringToHash("OpenTrig");
    protected static readonly int CloseTrig = Animator.StringToHash("CloseTrig");

    // Fires the instant the chest reaches its target position in
    // EntryRoutine — before openDelay/OpenTrig — so listeners that need the
    // chest's actual world position (e.g. RewardOfferUI seeding its particle
    // swarm's start point) don't have to guess the arrival time via a
    // separately-tuned delay of their own.
    public event System.Action OnLanded;

    protected Coroutine _entryRoutine;
    protected Coroutine _exitRoutine;

    // Called by RewardChestManager when the reward transition starts.
    public void Show(Vector3 position)
    {
        if (_exitRoutine != null)
        {
            StopCoroutine(_exitRoutine);
            _exitRoutine = null;
        }
        if (_entryRoutine != null)
        {
            StopCoroutine(_entryRoutine);
        }

        gameObject.SetActive(true);

        // Clear any stale trigger from a previous use; this chest is a
        // single reused instance and must always begin from its default
        // (closed) status.
        if (chestAnimator != null)
        {
            chestAnimator.ResetTrigger(OpenTrig);
            chestAnimator.ResetTrigger(CloseTrig);
        }

        _entryRoutine = StartCoroutine(EntryRoutine(position));
    }

    // Flies the chest in from outside the right screen edge along a
    // cosine-shaped arc (like a thrown/kicked projectile), then opens it
    // after a short landing delay.
    protected IEnumerator EntryRoutine(Vector3 target)
    {
        Vector3 start = ComputeOffscreenStart(target);
        transform.position = start;

        float t = 0f;
        while (t < entryDuration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / entryDuration);

            Vector3 pos = Vector3.Lerp(start, target, normalizedTime);
            // Cosine hump: 0 at both ends, peak at the midpoint.
            pos.y += entryArcHeight * Mathf.Sin(normalizedTime * Mathf.PI);
            transform.position = pos;

            yield return null;
        }

        transform.position = target;
        OnLanded?.Invoke();

        yield return new WaitForSeconds(openDelay);

        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(OpenTrig);
        }

        _entryRoutine = null;
    }

    protected Vector3 ComputeOffscreenStart(Vector3 target)
    {
        Camera cam = Camera.main;
        float depth = Mathf.Abs(target.z - cam.transform.position.z);
        Vector3 rightEdge = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, depth));

        Vector3 start = target;
        start.x = rightEdge.x + entryOffscreenMargin;
        return start;
    }

    // Called by RewardChestManager when the reward is given; slides
    // the chest off-screen and deactivates it once the slide finishes.
    public void Hide()
    {
        if (_entryRoutine != null)
        {
            StopCoroutine(_entryRoutine);
            _entryRoutine = null;
        }
        if (_exitRoutine != null)
        {
            StopCoroutine(_exitRoutine);
        }

        // The chest is only ever hidden after having been opened, so this
        // trigger is always valid at this point in the flow.
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(CloseTrig);
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
