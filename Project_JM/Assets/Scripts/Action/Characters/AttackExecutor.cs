// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/25/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackExecutor.cs
// Summary: A script to bridge between CombatManger and AttackMotions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using MatchEnums;
using System;
using UnityEngine;

public class AttackExecutor : MonoBehaviour
{
    protected AttackMotion _motion;
    protected EnemyAttackMotion _enemyMotion;

    [Header("Tier Attack Logics (assigned per character)")]
    [SerializeField] private AttackLogic logic3;
    [SerializeField] private AttackLogic logic4;
    [SerializeField] private AttackLogic logic5;
    [SerializeField] protected Transform attack3AttackPoint;
    [SerializeField] protected Transform attack4AttackPoint;
    [SerializeField] protected Transform attack5AttackPoint;

    [Header("Enemy attack logics")]
    [SerializeField] protected AttackLogic logicEnemy;
    [SerializeField] protected BoardDisableLogic boardDisableLogic;
    [SerializeField] protected BoardDisableEventChannel boardDisableChannel;
    [Header("Melee Attack")]
    [SerializeField, Tooltip("Null transform passes target's transform as hit transform")]
    protected Transform attackEnemyAttackPoint;
    [Header("Ranged Attack")]
    [SerializeField] private Vector3 hitTransformOffset;

    public Vector3 HitTransformOffset => hitTransformOffset;

    protected AttackContext _context;
    public AttackContext Context => _context;

    protected void Awake()
    {
        if (_motion == null)
        {
            _motion = GetComponent<AttackMotion>();
        }

        if (_enemyMotion == null)
        {
            _enemyMotion = GetComponent<EnemyAttackMotion>();
        }
    }

    protected void OnEnable()
    {
        if (_motion != null)
        {
            _motion.OnHit += HandleHit;
        }

        if (_enemyMotion != null)
        {
            _enemyMotion.OnHit += EnemyHandleHit;
        }
    }

    protected void OnDisable()
    {
        if (_motion != null)
        {
            _motion.OnHit -= HandleHit;
        }

        if (_enemyMotion != null)
        {
            _enemyMotion.OnHit -= EnemyHandleHit;
        }
    }

    protected void Start()
    {
        if (boardDisableChannel != null)
        {
            boardDisableChannel.Raise(new BoardDisableEventContext
            {
                boardDisablePhase = BoardDisablePhase.Preview,
                boardDisableLogic = boardDisableLogic
            });
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
        var targetObject = _context.Target as MonoBehaviour;

        Vector3 offset = Vector3.zero;

        if (attackerObject != null && targetObject != null)
        {       // Enemy attacks immediately
            offset = targetObject.transform.position - attackerObject.transform.position;
            attackerObject.GetComponent<EnemyAttackMotion>()?.PlayAttackMotion(offset);
        }

    }

    public void HandleHit(MatchTier tier)
    {
        HandleHitWithTransform(tier, tier switch
        {
            MatchTier.Three => attack3AttackPoint,
            MatchTier.Four => attack4AttackPoint,
            MatchTier.Five => attack5AttackPoint,
            _ => null
        });
    }

    public void HandleHitWithTransform(MatchTier tier, Transform hitTransform)
    {

        var targetObject = _context.Target as MonoBehaviour;
        _context.HitPosition = hitTransform.position;

        if (targetObject != null)
        {
            targetObject.GetComponent<EnemyAttackMotion>().PlayDamagedMotion(
                GetTierLogic(tier).GetTargetMotionOffset()
                );

            StartCoroutine(GetTierLogic(tier).Execute(_context));
        }
    }

    public void EnemyHandleHit()
    {
        var targetObject = _context.Target as MonoBehaviour;

        _context.HitPosition = attackEnemyAttackPoint != null ? attackEnemyAttackPoint.position : targetObject.transform.position + hitTransformOffset;

        if (targetObject != null)
        {
            targetObject.GetComponent<AttackMotion>().RequestHurt(logicEnemy.GetTargetMotionOffset());

            StartCoroutine(logicEnemy.Execute(_context));

            boardDisableChannel.Raise(new BoardDisableEventContext
            {
                boardDisablePhase = BoardDisablePhase.Commit,
                boardDisableLogic = boardDisableLogic
            });

            boardDisableChannel.Raise(new BoardDisableEventContext
            {
                boardDisablePhase = BoardDisablePhase.Preview,
                boardDisableLogic = boardDisableLogic
            });

            Camera.main.GetComponent<CameraShake>()?.Shake();
        }
    }

    public AttackLogic GetTierLogic(MatchTier tier) => tier switch
    {
        MatchTier.Three => logic3,
        MatchTier.Four => logic4,
        MatchTier.Five => logic5,
        _ => null
    };
}
