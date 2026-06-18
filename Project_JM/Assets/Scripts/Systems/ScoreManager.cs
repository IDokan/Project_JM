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

    [Header("Tuning")]
    [SerializeField] private int damageScoreScale = 1000;
    [SerializeField] private int enrageBonusScale = 1000;
    [SerializeField] private int progressionBonusPerEnemy = 1000;

    public int TotalScore { get; private set; }

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
        float normalized = (damage / enemyMaxHP) * tierMultiplier * damageScoreScale;
        TotalScore += Mathf.RoundToInt(normalized);
    }

    private void OnCharacterDied(CharacterStatus stat)
    {
        if (!stat.TryGetComponent<EnemyTag>(out _))
        {
            return;
        }

        TotalScore += progressionBonusPerEnemy;

        if (!stat.TryGetComponent<EnemyAttackBehaviour>(out var behaviour) || behaviour.IsEnraged)
        {
            return;
        }

        float bonus = (Mathf.Max(0f, behaviour.EnrageTimer) / behaviour.EnrageDelay) * enrageBonusScale;
        TotalScore += Mathf.RoundToInt(bonus);
    }

    private void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            TotalScore = 0;
        }
    }
}
