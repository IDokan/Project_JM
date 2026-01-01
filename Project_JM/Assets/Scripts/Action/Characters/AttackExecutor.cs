// SPDX-License-Identifier: MIT
// Copyright (c) 12/25/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackExecutor.cs
// Summary: A script to bridge between CombatManger and AttackMotions.

using MatchEnums;
using System;
using UnityEngine;

[RequireComponent(typeof(AttackMotion))]
public class AttackExecutor : MonoBehaviour
{
    private AttackMotion motion;

    [Header("Tier Attack Logics (assigned per character)")]
    [SerializeField] private AttackLogic logic3;
    [SerializeField] private AttackLogic logic4;
    [SerializeField] private AttackLogic logic5;

    [Header("Enemy attack logics")]
    [SerializeField] protected AttackLogic logicEnemy;

    protected AttackContext _context;

    protected void Awake()
    {
        if (motion == null)
        {
            motion = GetComponent<AttackMotion>();
        }
    }

    protected void OnEnable()
    {
        if (motion != null)
        {
            motion.OnHit += HandleHit;
        }
    }

    protected void OnDisable()
    {
        if (motion != null)
        {
            motion.OnHit -= HandleHit;
        }
    }

    public void ExecuteAttack(AttackContext context, MatchTier matchTier)
    {
        _context = context;


        var attackerObject = gameObject;

        if (attackerObject != null)
        {
            attackerObject.GetComponent<AttackMotion>().EnqueueAttack(matchTier);
        }
    }

    public void ExecuteEnemyAttack(AttackContext context)
    {
        _context = context;

        var attackerObject = gameObject;
        var targetObject = context.Target as MonoBehaviour;

        Vector3 offset = Vector3.zero;

        if (attackerObject != null && targetObject != null)
        {       // Enemy attacks immediately
            offset = targetObject.transform.position - attackerObject.transform.localPosition;
            attackerObject.GetComponent<EnemyAttackMotion>()?.PlayAttackMotion(offset);
            targetObject.GetComponent<AttackMotion>().RequestHurt(logicEnemy.GetTargetMotionOffset());
        }

    }

    public void HandleHit(MatchTier tier)
    {

        var targetObject = _context.Target as MonoBehaviour;
        targetObject.GetComponent<EnemyAttackMotion>().PlayDamagedMotion(
            GetTierLogic(tier).GetTargetMotionOffset()
            );

        StartCoroutine(GetTierLogic(tier).Execute(_context));
    }

    public AttackLogic GetTierLogic(MatchTier tier) => tier switch
    {
        MatchTier.Three => logic3,
        MatchTier.Four => logic4,
        MatchTier.Five => logic5,
        _ => null
    };
}
