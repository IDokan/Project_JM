// SPDX-License-Identifier: MIT
// Copyright (c) 11/24/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlobalTimeManager.cs
// Summary: A manager that provides helper functions related to the Time.

using System;
using System.Collections;
using UnityEngine;

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance { get; private set; }

    public static event Action<float> OnScaleChanged;

    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    protected float DefaultScaler = 1f;
    protected float TimeScaler = 1f;

    protected float _globalTime;

    public static float DeltaTime => UnityEngine.Time.deltaTime * Instance.TimeScaler;

    public static float Time => Instance == null ? 0f : Instance._globalTime;

    protected Coroutine _timerRoutine;

    protected float _skipTimeScale = 3f;
    protected bool _isPaused = false;

    public static IEnumerator WaitForGlobalSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += DeltaTime;
            yield return null;
        }
    }

    void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void Awake()
    {
        Instance = this;

        _globalTime = 0f;
    }

    protected void Update()
    {
        _globalTime += DeltaTime;
    }

    public void SetTimer(float newScaler, float duration)
    {
        EndRoutine();

        _timerRoutine = StartCoroutine(ScaleRoutine(newScaler, duration));
    }

    protected IEnumerator ScaleRoutine(float newValue, float duration)
    {
        TimeScaler = newValue;
        OnScaleChanged?.Invoke(TimeScaler);

        yield return WaitForGlobalSeconds(duration);

        TimeScaler = DefaultScaler;
        OnScaleChanged?.Invoke(TimeScaler);
        _timerRoutine = null;
    }

    protected void EndRoutine()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);

            TimeScaler = DefaultScaler;
            OnScaleChanged?.Invoke(TimeScaler);
            _timerRoutine = null;
        }
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.MiddleTransitionStarts ||
            phase == TransitionPhase.EndTransitionBegin)
        {
            EndRoutine();
        }
    }

    public void SkipTransition(bool isSkipping)
    {
        if (!_isPaused)
        {
            UnityEngine.Time.timeScale = isSkipping ? _skipTimeScale : 1f;
        }
    }

    public void RestoreTimeScaleFromSkip()
    {
        if (!_isPaused)
        {
            UnityEngine.Time.timeScale = 1f;
        }
    }

    public void PauseTimeScale()
    {
        _isPaused = true;
        UnityEngine.Time.timeScale = 0f;
    }

    public void RestoreTimeScaleFromPause()
    {
        _isPaused = false;
        UnityEngine.Time.timeScale = 1f;
    }

    public void RestoreTimeScaleExitCombatScene()
    {
        _isPaused = false;
        UnityEngine.Time.timeScale = 1f;
    }
}
