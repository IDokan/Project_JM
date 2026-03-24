// SPDX-License-Identifier: MIT
// Copyright (c) 03/16/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TransitionManager.cs
// Summary: A script to manager all transition and its data.

using System;
using System.Collections.Generic;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    [SerializeField] protected TransitionController[] transitionControllers;
    [SerializeField] protected float skipHoldingTime = 1.5f;
    [SerializeField] protected float skipTimeScale = 3f;

    public event Action<float, float> OnSkipTimerChanged;

    protected TransitionController _currentTransition = null;

    protected bool _isHoldingSkip = false;
    protected float _skipTimer = 0f;

    protected readonly Queue<PendingTransitionRequest> _pendingTransitionRequests = new Queue<PendingTransitionRequest>();


    protected struct PendingTransitionRequest
    {
        public PendingTransitionRequest(TransitionController controller, Action startAction)
        {
            Controller = controller;
            StartAction = startAction;
        }

        public TransitionController Controller { get; }
        public Action StartAction { get; }
    }

    protected void OnEnable()
    {
        if (transitionControllers == null || transitionControllers.Length == 0)
        {
            transitionControllers = GetComponentsInChildren<TransitionController>();
        }
    }


    protected void Start()
    {
        NotifySkipTimerChanged();
    }

    protected void OnDisable()
    {
        _pendingTransitionRequests.Clear();
        _currentTransition = null;
        ResetSkipState();
    }

    void Update()
    {
        if (_currentTransition == null)
        {
            return;
        }

        float delta = Time.unscaledDeltaTime;

        _skipTimer += _isHoldingSkip ? delta : -delta / 2f;

        _skipTimer = Mathf.Clamp(_skipTimer, 0f, skipHoldingTime);

        Time.timeScale = (_isHoldingSkip && _skipTimer >= skipHoldingTime) ?
            skipTimeScale : 1f;

        NotifySkipTimerChanged();
    }

    public void BeginSkipHold()
    {
        if (_currentTransition == null)
        {
            return;
        }

        _isHoldingSkip = true;
    }

    public void EndSkipHold()
    {
        _isHoldingSkip = false;

        Time.timeScale = 1f;
    }


    public bool TryStartTransition(TransitionController controller, Action startAction)
    {
        if (controller == null || startAction == null)
        {
            return false;
        }

        if (_currentTransition == null)
        {
            _currentTransition = controller;
            ResetSkipState();
            startAction();
            return true;
        }

        // Check multiple identical requests
        if (_currentTransition == controller || HasPendingRequest(controller))
        {
            return false;
        }

        _pendingTransitionRequests.Enqueue(new PendingTransitionRequest(controller, startAction));

        return false;
    }

    public void CompleteTransition(TransitionController controller)
    {
        if (_currentTransition == null || _currentTransition != controller)
        {
            // Deny other transition's completion
            return;
        }

        _currentTransition = null;

        ResetSkipState();
        TryStartNextTransition();
    }

    protected void NotifySkipTimerChanged()
    {
        OnSkipTimerChanged?.Invoke(_skipTimer, skipHoldingTime);
    }

    protected void ResetSkipState()
    {
        _isHoldingSkip = false;
        _skipTimer = 0f;
        Time.timeScale = 1f;
        NotifySkipTimerChanged();
    }

    protected bool HasPendingRequest(TransitionController controller)
    {
        foreach (PendingTransitionRequest request in _pendingTransitionRequests)
        {
            if (request.Controller == controller)
            {
                return true;
            }
        }
        return false;
    }

    protected void TryStartNextTransition()
    {
        while (_pendingTransitionRequests.Count > 0 && _currentTransition == null)
        {
            PendingTransitionRequest pendingRequest = _pendingTransitionRequests.Dequeue();

            if (pendingRequest.Controller == null ||
                pendingRequest.StartAction == null)
            {
                continue;
            }

            _currentTransition = pendingRequest.Controller;

            ResetSkipState();
            pendingRequest.StartAction();
        }
    }
}
