// SPDX-License-Identifier: MIT
// Copyright (c) 11/13/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackCooldownBinder.cs
// Summary: A binder that connects between EnemyAttackBehaviour and BarUI to display attack cooldown.


using UnityEngine;

[RequireComponent(typeof(BarUI))]
public class AttackCooldownBinder : MonoBehaviour
{
    [SerializeField] protected EnemyAttackBehaviour _boundEnemyAI;
    protected BarUI _barUI;

    protected void OnEnable() => _boundEnemyAI.OnAttackTimerChanged += UpdateAttackTimer;
    protected void OnDisable() => _boundEnemyAI.OnAttackTimerChanged -= UpdateAttackTimer;

    protected void Awake()
    {
        _barUI = GetComponent<BarUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected void UpdateAttackTimer(float current, float max)
    {
        _barUI.UpdateValue(current, max);
    }
}
