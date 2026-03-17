// SPDX-License-Identifier: MIT
// Copyright (c) 03/16/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TransitionManager.cs
// Summary: A script to manager all transition and its data.

using System;
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


    protected void OnEnable()
    {
        if (transitionControllers == null || transitionControllers.Length == 0)
        {
            transitionControllers = GetComponentsInChildren<TransitionController>();
        }

        foreach (TransitionController tc in transitionControllers)
        {
            tc.Started += OnTransitionStarted;
            tc.Completed += OnTransitionCompleted;
        }
    }


    protected void Start()
    {
        NotifySkipTimerChanged();
    }

    protected void OnDisable()
    {
        foreach (TransitionController tc in transitionControllers)
        {
            tc.Started -= OnTransitionStarted;
            tc.Completed -= OnTransitionCompleted;
        }
        _isHoldingSkip = false;
        Time.timeScale = 1f;
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

    protected void OnTransitionStarted(TransitionController tc)
    {
        _currentTransition = tc;
        _isHoldingSkip = false;
        _skipTimer = 0f;

        Time.timeScale = 1f;
        NotifySkipTimerChanged();
    }

    protected void OnTransitionCompleted(TransitionController tc)
    {
        _currentTransition = null;

        _isHoldingSkip = false;

        Time.timeScale = 1f;
        NotifySkipTimerChanged();
    }

    protected void NotifySkipTimerChanged()
    {
        OnSkipTimerChanged?.Invoke(_skipTimer, skipHoldingTime);
    }
}
