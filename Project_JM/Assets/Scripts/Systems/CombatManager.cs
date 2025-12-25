// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatManager.cs
// Summary: A class to manage whole combat logic.

using GemEnums;
using MatchEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] protected EnemyAttackEventChannel _enemyAttackChannel;
    [SerializeField] protected EnemySpawnedEventChannel _enemySpawnedEventChannel;
    [SerializeField] protected AttackBook _attackBook;
    [SerializeField] protected PartyRoster _party;
    [SerializeField] protected DamageMultiplierManager _damageMultiplierManager;


    [Header("Targeting")]
    [SerializeField] protected CharacterCombatant _enemy;    // @@ TODO: Need to implement enemy spawner...

    protected ICombatant lastAttackedCharacter;

    protected void OnEnable()
    {
        _enemyAttackChannel.OnRaised += OnEnemyAttack;
        _enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
    }

    protected void OnDisable()
    {
        _enemyAttackChannel.OnRaised -= OnEnemyAttack;
        _enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastAttackedCharacter = _party.Get(GemColorUtility.GetRandomGemColor());
    }

    // Update is called once per frame
    void Update()
    {

    }

    // SystemBehaviour calls it
    public void OnMatch(MatchEvent matchEvent)
    {
        if (matchEvent.Color == GemColor.None)
        {
            return;
        }

        var attacker = _party.Get(matchEvent.Color);
        if (attacker == null)
        {
            Debug.LogWarning($"No attacker for {matchEvent.Color}");
            return;
        }
        // Record last attacked character after NULL check
        lastAttackedCharacter = attacker;

        var attackLogic = _attackBook.Get(matchEvent.Color, matchEvent.Tier);
        if (attackLogic == null)
        {
            Debug.LogWarning($"No attack logic for {matchEvent.Color} {matchEvent.Tier}");
            return;
        }

        if (_enemy == null)
        {
            Debug.LogWarning($"No enemy");
            return;
        }


        var context = new AttackContext
        {
            Attacker = attacker,
            Target = _enemy,
            DamageMultiplierManager = _damageMultiplierManager
        };


        PlayAttackMotion(attackLogic, context, matchEvent.Tier);

        StartCoroutine(attackLogic.Execute(context));
    }

    protected void OnEnemyAttack(AttackLogic logic)
    {
        var enemy_context = new AttackContext
        {
            Attacker = _enemy,
            Target = lastAttackedCharacter,
            DamageMultiplierManager = _damageMultiplierManager
        };

        PlayAttackMotion(logic, enemy_context, MatchTier.Three);

        StartCoroutine(logic.Execute(enemy_context));
    }

    protected void PlayAttackMotion(AttackLogic logic, AttackContext context, MatchTier matchTier)
    {
        var attackerObject = context.Attacker as MonoBehaviour;
        var targetObject = context.Target as MonoBehaviour;

        if (attackerObject != null)
        {
            bool isEnemy = attackerObject.TryGetComponent<EnemyAttackMotion>(out _);

            if (isEnemy)
            {       // Manually move enemy by target characters.
                Vector3 offset = Vector3.zero;

                if (targetObject != null)
                {
                    offset = targetObject.transform.position - attackerObject.transform.localPosition;
                }

                attackerObject.GetComponent<EnemyAttackMotion>().PlayAttackMotion(offset);
            }
            else
            {
                attackerObject.GetComponent<AttackMotion>().EnqueueAttack(matchTier);
            }
        }

        if (targetObject != null)
        {
            bool isEnemy = targetObject.TryGetComponent<EnemyAttackMotion>(out _);

            if (isEnemy)
            {
                targetObject.GetComponent<EnemyAttackMotion>().PlayDamagedMotion(logic.GetTargetMotionOffset());
            }
            else
            {
                targetObject.GetComponent<AttackMotion>().RequestHurt(logic.GetTargetMotionOffset());
            }
        }
    }

    protected void OnEnemySpawned(GameObject enemy)
    {
        _enemy = enemy.GetComponent<CharacterCombatant>();
    }
}