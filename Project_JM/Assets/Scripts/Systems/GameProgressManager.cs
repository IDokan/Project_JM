// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/18/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GameProgressManager.cs
// Summary: A class to manage whole game progress.
//          Handles and judges game data produced during game progress (e.g. enemy defeat count,
//          enrage state, party HP ratio) to evaluate save-worthy milestones.
//
//              Easy    condition: defeat the 4th enemy with party HP above 60% of max HP
//              Medium  condition: defeat the 4th enemy with party HP above 40% of max HP
//              Hard    condition: defeat 4 enemies
//              Enemy defeat achievements: defeat a specific enemy type 10 times total,
//                  tracked per CharacterId across runs via SaveDataManager
//              Character damage achievements: deal 10,000 / 100,000 lifetime damage
//                  as a given party class (Knight/Bowman/Mage/Cleric, keyed by GemColor),
//                  flushed from DamageRecordManager into SaveDataManager on every
//                  character death (ally or enemy)
//              Steam Timeline markers: dropped on every enemy defeat, and on every
//                  tier 5 gem match (via MatchEventChannel, independent of whether
//                  the match's attack has resolved yet)
//
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using AchievementEnums;
using CharacterEnums;
using GemEnums;
using MatchEnums;
using TutorialEnums;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterDeathEventChannel deathChannel;
    [SerializeField] protected MatchEventChannel matchEventChannel;
    [SerializeField] protected DifficultyCurvesSelector curvesSelector;
    [SerializeField] protected CharacterStatus partyStatus;
    [SerializeField] protected DamageRecordManager damageRecordManager;

    protected int _numEnemyDefeated = 0;

    private TutorialProgress _progressAtRunStart;


    protected void OnEnable()
    {
        if (deathChannel != null)
        {
            deathChannel.OnRaised += OnCharacterDied;
        }
        else
        {
            Debug.LogWarning("CharacterDeathEventChannel is null", this);
        }

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised += OnTransitionEvent;
        }
        else
        {
            Debug.LogWarning("TransitionEventChannel is null", this);
        }

        if (matchEventChannel != null)
        {
            matchEventChannel.OnRaised += OnMatchRaised;
        }
        else
        {
            Debug.LogWarning("MatchEventChannel is null", this);
        }

    }

    protected void OnDisable()
    {
        deathChannel.OnRaised -= OnCharacterDied;

        if (transitionEventChannel != null)
        {
            transitionEventChannel.OnRaised -= OnTransitionEvent;
        }

        if (matchEventChannel != null)
        {
            matchEventChannel.OnRaised -= OnMatchRaised;
        }

    }

    public void Clear()
    {
        _numEnemyDefeated = 0;
        _progressAtRunStart = SaveDataManager.Instance.Progress;
    }

    protected void OnCharacterDied(CharacterStatus stat)
    {
        FlushDamageRecordToSaveData();

        if (stat.TryGetComponent<EnemyTag>(out _))
        {
            HandleEnemyDied(stat);
        }
        else if (stat.TryGetComponent<AllyTag>(out _))
        {
            HandleAllyDied(stat);
        }
        else
        {
            // CharacterStat must have at least one Tag class.
        }
    }

    protected void HandleAllyDied(CharacterStatus stat)
    {
        if (_numEnemyDefeated > 1)
        {
            return;
        }

        if (_progressAtRunStart == TutorialProgress.Hard)
        {
            SaveDataManager.Instance.ResetToMedium();
        }
        else if (_progressAtRunStart == TutorialProgress.Medium)
        {
            SaveDataManager.Instance.ResetToEasy();
        }
        // Challenge is permanent — no reset.
    }

    protected void HandleEnemyDied(CharacterStatus stat)
    {
        ++_numEnemyDefeated;
        partyStatus.Initialize(curvesSelector.ActiveCurves.GetAllyDifficultyMultiplier(_numEnemyDefeated));

        TryUnlockEnemyDefeatAchievement(stat.CharacterId);

        if (SteamManager.Instance != null)
        {
            SteamManager.Instance.AddInstantaneousTimelineMarker(
                $"{stat.CharacterId} Defeated", "Enemy defeated", "steam_flag");
        }

        if (_progressAtRunStart == TutorialProgress.Easy)
        {
            if (_numEnemyDefeated == 4 && partyStatus.CurrentHP > partyStatus.maxHP * 0.6f)
            {
                SaveDataManager.Instance.SetMedium();

                if (SteamManager.Instance != null)
                {
                    SteamManager.Instance.UnlockAchievement(AchievementId.Welcome);
                }
            }
        }

        if (_progressAtRunStart == TutorialProgress.Medium
            && _numEnemyDefeated >= 4
            && partyStatus.CurrentHP > partyStatus.maxHP * 0.4f)
        {
            SaveDataManager.Instance.SetHard();

            if (SteamManager.Instance != null)
            {
                SteamManager.Instance.UnlockAchievement(AchievementId.FastLearner);
            }
        }

        if (_progressAtRunStart == TutorialProgress.Hard && _numEnemyDefeated >= 4)
        {
            SaveDataManager.Instance.SetChallenge();
        }
    }

    protected void TryUnlockEnemyDefeatAchievement(CharacterId characterId)
    {
        int defeatCount = SaveDataManager.Instance.IncrementEnemyDefeatCount(characterId);
        if (defeatCount != 10)
        {
            return;
        }

        AchievementId? achievementId = GetEnemyDefeatAchievementId(characterId);
        if (achievementId.HasValue && SteamManager.Instance != null)
        {
            SteamManager.Instance.UnlockAchievement(achievementId.Value);
        }
    }

    protected static AchievementId? GetEnemyDefeatAchievementId(CharacterId characterId)
    {
        switch (characterId)
        {
            case CharacterId.SlimeKing:
                return AchievementId.SlimeKingSlayer;
            case CharacterId.AntGladiator:
                return AchievementId.AntGladiatorConqueror;
            case CharacterId.MushroomBully:
                return AchievementId.MushroomBullySubduer;
            case CharacterId.FoxThief:
                return AchievementId.FoxThiefHunter;
            case CharacterId.SnailWizard:
                return AchievementId.SnailWizardVanquisher;
            case CharacterId.DandelionToad:
                return AchievementId.DandelionToadExterminator;
            default:
                return null;
        }
    }

    protected void FlushDamageRecordToSaveData()
    {
        List<KeyValuePair<GemColor, int>> encounterDamage = damageRecordManager.GetSortedDamage();
        for (int i = 0; i < encounterDamage.Count; i++)
        {
            GemColor color = encounterDamage[i].Key;
            int amount = encounterDamage[i].Value;
            if (amount <= 0)
            {
                continue;
            }

            int previousTotal = SaveDataManager.Instance.GetDamageDealt(color);
            int newTotal = SaveDataManager.Instance.AddDamageDealt(color, amount);

            TryUnlockDamageAchievement(color, previousTotal, newTotal);
        }
    }

    protected void TryUnlockDamageAchievement(GemColor color, int previousTotal, int newTotal)
    {
        if (SteamManager.Instance == null)
        {
            return;
        }

        if (previousTotal < 10000 && newTotal >= 10000)
        {
            AchievementId? id = GetDamage10KAchievementId(color);
            if (id.HasValue)
            {
                SteamManager.Instance.UnlockAchievement(id.Value);
            }
        }

        if (previousTotal < 100000 && newTotal >= 100000)
        {
            AchievementId? id = GetDamage100KAchievementId(color);
            if (id.HasValue)
            {
                SteamManager.Instance.UnlockAchievement(id.Value);
            }
        }
    }

    protected static AchievementId? GetDamage10KAchievementId(GemColor color)
    {
        switch (color)
        {
            case GemColor.Red:
                return AchievementId.KnightDamage10K;
            case GemColor.Yellow:
                return AchievementId.BowmanDamage10K;
            case GemColor.Blue:
                return AchievementId.MageDamage10K;
            case GemColor.Green:
                return AchievementId.ClericDamage10K;
            default:
                return null;
        }
    }

    protected static AchievementId? GetDamage100KAchievementId(GemColor color)
    {
        switch (color)
        {
            case GemColor.Red:
                return AchievementId.KnightDamage100K;
            case GemColor.Yellow:
                return AchievementId.BowmanDamage100K;
            case GemColor.Blue:
                return AchievementId.MageDamage100K;
            case GemColor.Green:
                return AchievementId.ClericDamage100K;
            default:
                return null;
        }
    }

    protected void OnMatchRaised(MatchEvent matchEvent)
    {
        if (matchEvent.Tier != MatchTier.Five || SteamManager.Instance == null)
        {
            return;
        }

        SteamManager.Instance.AddInstantaneousTimelineMarker(
            GetTierFiveMatchTitle(matchEvent.Color), "Tier 5 gem match", "steam_starburst");
    }

    protected static string GetTierFiveMatchTitle(GemColor color)
    {
        switch (color)
        {
            case GemColor.Red:
                return "Knight Tier 5 Match";
            case GemColor.Blue:
                return "Mage Tier 5 Match";
            case GemColor.Green:
                return "Cleric Tier 5 Match";
            case GemColor.Yellow:
                return "Bowman Tier 5 Match";
            default:
                return "Tier 5 Match";
        }
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
        }
    }

}
