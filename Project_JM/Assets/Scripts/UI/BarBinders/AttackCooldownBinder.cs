// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/13/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackCooldownBinder.cs
// Summary: A binder that connects between EnemyAttackBehaviour and BarUI to display attack cooldown.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using UnityEngine;

[RequireComponent(typeof(BarUI))]
public class AttackCooldownBinder : MonoBehaviour
{
    [SerializeField] protected EnemyAttackBehaviour boundEnemyAI;
    protected BarUI _barUI;

    protected void OnEnable()
    {
        if (boundEnemyAI != null)
        {
            boundEnemyAI.OnAttackTimerChanged += UpdateAttackTimer;
        }
    }
    protected void OnDisable()
    {
        if (boundEnemyAI != null)
        {
            boundEnemyAI.OnAttackTimerChanged -= UpdateAttackTimer;
        }
    }

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

    public void BindNewAI(EnemyAttackBehaviour newAI)
    {
        if (newAI == boundEnemyAI) return;

        if (boundEnemyAI != null)
        {
            boundEnemyAI.OnAttackTimerChanged -= UpdateAttackTimer;
        }

        boundEnemyAI = newAI;
        boundEnemyAI.OnAttackTimerChanged += UpdateAttackTimer;

        UpdateAttackTimer(boundEnemyAI.Cooldown, boundEnemyAI.Cooldown);
    }

    protected void UpdateAttackTimer(float current, float max)
    {
        _barUI.UpdateValue(current, max);
    }
}
