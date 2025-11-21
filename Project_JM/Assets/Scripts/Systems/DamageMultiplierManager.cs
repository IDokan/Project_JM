// SPDX-License-Identifier: MIT
// Copyright (c) 11/17/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageMultiplierManager.cs
// Summary: A class to manage damage multiplier.

using System.Collections.Generic;
using UnityEngine;

public class TimedModifier
{
    public float Multiplier { get; private set; }
    protected float _timeRemaining;

    public TimedModifier(float multiplier, float duration)
    {
        Multiplier = multiplier;
        _timeRemaining = duration;
    }

    public bool UpdateTimer(float deltaTime)
    {
        _timeRemaining -= deltaTime;
        return _timeRemaining <= 0f;
    }
}

public class DamageMultiplierManager : MonoBehaviour
{
    [SerializeField] CharacterDeathEventChannel _characterDeathEventChannel;
    [SerializeField] DifficultyCurves _difficultyCurves;

    public float DamageMultiplier { get; protected set; } = 1f;
    protected readonly List<TimedModifier> _timedModifiers = new();
    protected int numEnemyDefeated = 0;

    public float GetMultiplier
    {
        get
        {
            float result = DamageMultiplier;
            foreach (var m in _timedModifiers)
            {
                result *= m.Multiplier;
            }

            return result;
        }
    }

    protected void OnEnable() => _characterDeathEventChannel.OnRaised += OnCharacterDiedHandle;
    protected void OnDisable() => _characterDeathEventChannel.OnRaised -= OnCharacterDiedHandle;

    public float GetRawMultiplier
    {
        get { return DamageMultiplier; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = _timedModifiers.Count - 1; i >= 0; i--)
        {
            if (_timedModifiers[i].UpdateTimer(Time.deltaTime))
            {
                _timedModifiers.RemoveAt(i);
            }
        }
    }

    public void AddTimedBonus(float multiplier, float duration)
    {
        _timedModifiers.Add(new TimedModifier(multiplier, duration));
    }

    public void OnCharacterDiedHandle(CharacterStatus status)
    {
        if (status.TryGetComponent<EnemyTag>(out _))
        {
            ++numEnemyDefeated;

            DamageMultiplier = (_difficultyCurves.DamageMultiplierCurve.Evaluate(numEnemyDefeated));
        }
    }
}
