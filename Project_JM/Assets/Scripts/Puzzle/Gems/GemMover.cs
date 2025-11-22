// SPDX-License-Identifier: MIT
// Copyright (c) 11/06/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemMover.cs
// Summary: A script for gem movement.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using UnityEngine;

public class GemMover : MonoBehaviour
{
    protected struct MoveStep
    {
        public Vector2 Target;
        public Action OnComplete;
        public float Duration;          // seconds
        public MoveStep(Vector2 target, Action onComplete, float duration)
        {
            Target = target; OnComplete = onComplete; Duration = Mathf.Max(0.01f, duration);
        }
    }

    // === Settings ===
    [SerializeField] protected float _defaultDuration = 0.2f;
    [SerializeField] protected float _epsilon = 0.01f;

    protected readonly Queue<MoveStep> _queue = new();
    protected Coroutine _runner;

    public bool IsMoving => _runner != null;
    public int PendingCount => _queue.Count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
        ClearQueue();
    }

    // --- Public API ---

    /// Enqueue a local position to move to
    public void EnqueueMove(Vector2 target, Action onComplete, float duration = -1f, bool clearExisting = false)
    {
        if (clearExisting)
        {
            ClearQueue();
        }

        if(duration <= 0f)
        {
            duration = _defaultDuration;
        }

        _queue.Enqueue(new MoveStep(target, onComplete, duration));

        if (_runner == null)
        {
            _runner = StartCoroutine(RunQueue());
        }
    }

    /// Clears any pending moves
    public void ClearQueue(Vector2? snapPos = null)
    {
        while (_queue.Count > 0)
        {
            var step = _queue.Dequeue();
            step.OnComplete?.Invoke();
        }

        if (snapPos.HasValue)
        {
            // @@ TODO: Get transform correclty.
            transform.localPosition = snapPos.Value;
        }
    }

    // --- Core loop ---

    IEnumerator RunQueue()
    {
        while (_queue.Count > 0)
        {
            var step = _queue.Dequeue();
            yield return MoveTo(step.Target, step.Duration);

            step.OnComplete?.Invoke();

            if (this == null)
            {
                // safety if destroyed mid-move
                yield break;
            }
        }

        _runner = null;
    }

    // Move with a simple ease-in
    IEnumerator MoveTo(Vector2 target, float duration)
    {
        Vector2 start = transform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            // Quadratic ease-in. Can be improved later.
            float u = t * t;
            transform.localPosition = Vector2.LerpUnclamped(start, target, u);

            // Early snap if close enough
            if ((Vector2)transform.localPosition == target ||
                Vector2.SqrMagnitude((Vector2)transform.localPosition - target) < _epsilon * _epsilon)
            {
                break;
            }

            yield return null;
        }

        transform.localPosition = target;
    }
}
