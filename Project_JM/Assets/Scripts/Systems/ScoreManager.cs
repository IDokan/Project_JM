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
    [SerializeField] private float damageScoreScale = 1000f;
    [SerializeField] private float enrageBonusScale = 1000f;
    [SerializeField] private float progressionBonusPerEnemy = 1000f;

    public int TotalScore { get; private set; }

    private int _enemiesDefeated;

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

    // Divides by LevelMultiplier only, leaving DifficultyMultiplier intact so harder
    // enemies reward proportionally more total score while score per hit stays consistent.
    // tier multiplier: Three→×1, Four→×2, Five→×3
    public void AddDamageScore(int damage, float levelMultiplier, MatchTier tier)
    {
        float normalized = (damage / levelMultiplier) * ((int)tier - 2) * damageScoreScale;
        TotalScore += Mathf.RoundToInt(normalized);
    }

    private void OnCharacterDied(CharacterStatus stat)
    {
        if (!stat.TryGetComponent<EnemyTag>(out _))
        {
            return;
        }

        _enemiesDefeated++;
        TotalScore += Mathf.RoundToInt(_enemiesDefeated * progressionBonusPerEnemy);

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
            _enemiesDefeated = 0;
        }
    }
}
