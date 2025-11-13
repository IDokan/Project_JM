// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatManager.cs
// Summary: A class to manage whole combat logic.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MatchEnums;
using GemEnums;

public class CombatManager : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] protected EnemyAttackEventChannel _enemyAttackChannel;
    [SerializeField] protected MatchEventChannel _matchEvents;
    [SerializeField] protected AttackBook _attackBook;
    [SerializeField] protected PartyRoster _party;

    [Header("Targeting")]
    [SerializeField] protected CharacterCombatant _enemy;    // @@ TODO: Need to implement enemy spawner...

    protected ICombatant lastAttackedCharacter;

    protected void OnEnable()
    {
        _matchEvents.OnRaised += OnMatch;
        _enemyAttackChannel.OnRaised += OnEnemyAttack;
    }

    protected void OnDisable()
    {
        _matchEvents.OnRaised -= OnMatch;
        _enemyAttackChannel.OnRaised -= OnEnemyAttack;
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

    protected void OnMatch(MatchEvent matchEvent)
    {

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
            Target = _enemy
        };

        PlayAttackMotion(context, attackLogic);

        StartCoroutine(attackLogic.Execute(context));
    }

    protected void OnEnemyAttack(AttackLogic logic)
    {
        var enemy_context = new AttackContext
        {
            Attacker = _enemy,
            Target = lastAttackedCharacter
        };

        PlayAttackMotion(enemy_context, logic);
        
        StartCoroutine(logic.Execute(enemy_context));
    }

    protected void PlayAttackMotion(AttackContext context, AttackLogic logic)
    {
        if(context.Attacker is MonoBehaviour attackerObject)
        {
            attackerObject.GetComponent<AttackMotion>().PlayAttackMotion(logic.GetAttackerMotionOffset());
        }

        if (context.Target is MonoBehaviour targetObject)
        {
            targetObject.GetComponent<AttackMotion>().PlayAttackMotion(logic.GetTargetMotionOffset());
        }
    }
}