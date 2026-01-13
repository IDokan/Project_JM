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
    [SerializeField] protected GemPowerArrivedEventChannel _gemPowerArrivedChannel;
    [SerializeField] protected EnemyAttackEventChannel _enemyAttackChannel;
    [SerializeField] protected EnemySpawnedEventChannel _enemySpawnedEventChannel;
    [SerializeField] protected PartyRoster _party;
    [SerializeField] protected DamageMultiplierManager _damageMultiplierManager;


    [Header("Targeting")]
    [SerializeField] protected CharacterCombatant _enemy;    // @@ TODO: Need to implement enemy spawner...

    protected ICombatant lastAttackedCharacter;

    protected void OnEnable()
    {
        _gemPowerArrivedChannel.OnRaised += OnPowerArrived;
        _enemyAttackChannel.OnRaised += OnEnemyAttack;
        _enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
    }

    protected void OnDisable()
    {
        _gemPowerArrivedChannel.OnRaised -= OnPowerArrived;
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
    public void OnPowerArrived(MatchEvent matchEvent)
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
        Debug.Log($"Color {matchEvent.Color} : Tier {matchEvent.Tier} happened");

        PlayAttackMotion(context, matchEvent.Tier);
    }

    protected void OnEnemyAttack()
    {
        var enemy_context = new AttackContext
        {
            Attacker = _enemy,
            Target = lastAttackedCharacter,
            DamageMultiplierManager = _damageMultiplierManager
        };

        PlayAttackMotion(enemy_context, null);
    }

    protected void PlayAttackMotion(AttackContext context, MatchTier? matchTier)
    {
        var attackerObject = context.Attacker as MonoBehaviour;
        AttackExecutor executor = attackerObject.GetComponent<AttackExecutor>();

        if (matchTier.HasValue)
        {
            executor.ExecuteAttack(context, matchTier.Value);
        }
        else      // Enemy attack
        {
            executor.ExecuteEnemyAttack(context);
        }
    }

    protected void OnEnemySpawned(GameObject enemy)
    {
        _enemy = enemy.GetComponent<CharacterCombatant>();
    }
}