// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 18/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ScoreManager.cs
// Summary: Accumulates the player's score for a run.
//          Score grows by damage dealt scaled by match tier, plus a bonus
//          when an enemy dies before enraging.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using MatchEnums;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] private CharacterDeathEventChannel deathChannel;
    [SerializeField] private TransitionEventChannel transitionEventChannel;

    private const int DamageScoreScale = 1500;
    private const int EnrageBonusScale = 1500;
    private const int ProgressionBonusPerEnemy = 1500;

    private ObfuscatedInt _totalScore;
    private ObfuscatedInt _damageAccumulated;
    private ObfuscatedInt _enemiesDefeated;
    private ObfuscatedInt _enrageBonusAccumulated;

    public int TotalScore
    {
        get
        {
            int expected = _damageAccumulated.Value
                         + _enemiesDefeated.Value * ProgressionBonusPerEnemy
                         + _enrageBonusAccumulated.Value;
            if (expected != _totalScore.Value)
            {
                Debug.LogWarning("ScoreManager: score integrity check failed.");
                return 0;
            }
            return _totalScore.Value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        deathChannel.OnRaised += OnCharacterDied;
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    private void OnDisable()
    {
        deathChannel.OnRaised -= OnCharacterDied;
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    // tier multiplier: Three→×1, Four→×2, Five→×4
    public void AddDamageScore(int damage, float enemyMaxHP, MatchTier tier)
    {
        int tierMultiplier = tier switch
        {
            MatchTier.Three => 1,
            MatchTier.Four => 2,
            MatchTier.Five => 4,
            _ => 1
        };
        if (damage > enemyMaxHP)
        {
            return;
        }

        float normalized = Mathf.Clamp01(damage / enemyMaxHP) * tierMultiplier * DamageScoreScale;
        int contribution = Mathf.RoundToInt(normalized);
        _totalScore.Value += contribution;
        _damageAccumulated.Value += contribution;
    }

    private void OnCharacterDied(CharacterStatus stat)
    {
        if (!stat.TryGetComponent<EnemyTag>(out _))
        {
            return;
        }

        _totalScore.Value += ProgressionBonusPerEnemy;
        _enemiesDefeated.Value += 1;

        if (!stat.TryGetComponent<EnemyAttackBehaviour>(out var behaviour) || behaviour.IsEnraged)
        {
            return;
        }

        float bonus = (Mathf.Max(0f, behaviour.EnrageTimer) / behaviour.EnrageDelay) * EnrageBonusScale;
        int enrageContribution = Mathf.RoundToInt(bonus);
        _totalScore.Value += enrageContribution;
        _enrageBonusAccumulated.Value += enrageContribution;
    }

    private void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            _totalScore = new ObfuscatedInt(0);
            _damageAccumulated = new ObfuscatedInt(0);
            _enemiesDefeated = new ObfuscatedInt(0);
            _enrageBonusAccumulated = new ObfuscatedInt(0);
        }
    }
}
