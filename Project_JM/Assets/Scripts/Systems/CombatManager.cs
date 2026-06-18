// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatManager.cs
// Summary: A class to manage whole combat logic.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using MatchEnums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] protected GemPowerArrivedEventChannel gemPowerArrivedChannel;
    [SerializeField] protected EnemyAttackEventChannel enemyAttackChannel;
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected PartyRoster party;
    [SerializeField] protected DamageMultiplierManager damageMultiplierManager;

    protected CharacterCombatant _enemy;    // @@ TODO: Need to implement enemy spawner...
    public CharacterCombatant Enemy => _enemy;
    public Transform EnemyTransform => Enemy != null ? Enemy.transform : null;

    protected ICombatant _lastAttackedCharacter;

    protected void OnEnable()
    {
        gemPowerArrivedChannel.OnRaised += OnPowerArrived;
        enemyAttackChannel.OnRaised += OnEnemyAttack;
        enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
        characterDeathEventChannel.OnRaised += OnAnyoneDied;
    }

    protected void OnDisable()
    {
        gemPowerArrivedChannel.OnRaised -= OnPowerArrived;
        enemyAttackChannel.OnRaised -= OnEnemyAttack;
        enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
        characterDeathEventChannel.OnRaised -= OnAnyoneDied;
    }

    protected void Awake()
    {
        _lastAttackedCharacter = party.Get(GemColorUtility.GetRandomGemColor(true));
    }
    
    void Update()
    {

    }

    // SystemBehaviour calls it
    public void OnPowerArrived(MatchEvent matchEvent)
    {
        if (matchEvent.Color == GemColor.None || party == null)
        {
            return;
        }

        var attacker = party.Get(matchEvent.Color);
        if (attacker == null)
        {
            Debug.LogWarning($"No attacker for {matchEvent.Color}");
            return;
        }
        // Record last attacked character after NULL check
        _lastAttackedCharacter = attacker;

        if (_enemy == null)
        {
            Debug.LogWarning($"No enemy");
            return;
        }


        var context = new AttackContext
        {
            Attacker = attacker,
            Target = _enemy,
            DamageMultiplierManager = damageMultiplierManager,
            Tier = matchEvent.Tier
        };
        Debug.Log($"Color {matchEvent.Color} : Tier {matchEvent.Tier} happened");

        PlayAttackMotion(context, matchEvent.Tier);
    }

    protected void OnEnemyAttack()
    {
        if (_lastAttackedCharacter == null)
        {
            return;
        }

        var enemy_context = new AttackContext
        {
            Attacker = _enemy,
            Target = _lastAttackedCharacter,
            DamageMultiplierManager = damageMultiplierManager
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

    protected void OnAnyoneDied(CharacterStatus stat)
    {
        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            _enemy = null;
        }
    }
}