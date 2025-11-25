// SPDX-License-Identifier: MIT
// Copyright (c) 11/24/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlobalTimeManager.cs
// Summary: A manager that provides helper functions related to the Time.

using System.Collections;
using UnityEngine;

public class GlobalTimeManager : MonoBehaviour
{
    public static GlobalTimeManager Instance { get; private set; }

    [SerializeField] protected CharacterDeathEventChannel _characterDeathEventChannel;

    protected float DefaultScaler = 1f;
    protected float TimeScaler = 1f;

    public static float DeltaTime => Time.deltaTime * Instance.TimeScaler;

    protected Coroutine _timerRoutine;

    void OnEnable()
    {
        _characterDeathEventChannel.OnRaised += EndRoutine;
    }

    void OnDisable()
    {
        _characterDeathEventChannel.OnRaised -= EndRoutine;
    }

    protected void Awake()
    {
        Instance = this;
    }

    public void SetTimer(float newScaler, float duration)
    {
        EndRoutine(null);

        _timerRoutine = StartCoroutine(ScaleRoutine(newScaler, duration));
    }

    protected IEnumerator ScaleRoutine(float newValue, float duration)
    {
        TimeScaler = newValue;

        yield return new WaitForSeconds(duration);

        TimeScaler = DefaultScaler;
        _timerRoutine = null;
    }

    protected void EndRoutine(CharacterStatus stat)
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
        }
    }
}
