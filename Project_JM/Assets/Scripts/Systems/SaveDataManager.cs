// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SaveDataManager.cs
// Summary: Manages all cross-session persistent data via PlayerPrefs, the
//          single source of truth on every platform. Every write commits to
//          PlayerPrefs immediately, but the Steam Cloud push (Windows/Steam
//          build only) is deliberately infrequent - PushToSteamCloud() is
//          called only at scene-exit/quit checkpoints (see OnApplicationQuit
//          here, and MainMenu/PauseMenu/RetryMenu's scene-transition call
//          sites) rather than on every write, to keep FileWrite calls rare.
//          A fresh install with no local data bootstraps itself from the
//          Cloud snapshot on first launch.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections.Generic;
using System.Text;
using CharacterEnums;
using GemEnums;
using MatchEnums;
using RewardEnums;
using TutorialEnums;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    [SerializeField] private LeaderboardIconContainer leaderboardIconContainer;

    private static SaveDataManager _instance;

    public static SaveDataManager Instance
    {
        get
        {
            if (_instance == null)
            {
                InitializeInstance();
            }
            return _instance;
        }
    }

    private static void InitializeInstance()
    {
        _instance = FindFirstObjectByType<SaveDataManager>();
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Runs once per install, not once per session: guarded internally by
        // PlayerPrefs.HasKey per slot, so re-entering this scene never re-rolls
        // an icon the player (or a prior bootstrap) already assigned.
        LeaderboardIconPrefs.EnsureRandomDefaultsAssigned(leaderboardIconContainer);
    }

    // Runs in Start(), not Awake(): Unity guarantees every active scene object's
    // Awake() completes before any object's Start() runs, so SteamManager.Awake()
    // (which synchronously runs SteamAPI.Init()) is always done by the time this
    // fires, without relying on Awake-vs-Awake ordering between the two singletons.
    private void Start()
    {
        if (PlayerPrefs.HasKey(KeyHasLocalSaveData))
        {
            return;
        }

        if (SteamManager.Instance == null || !SteamManager.Instance.TryPullSaveSnapshot(out string json))
        {
            return;
        }

        if (!ApplySnapshotJson(json))
        {
            return;
        }

        PlayerPrefs.SetInt(KeyHasLocalSaveData, 1);
        PlayerPrefs.Save();
        Debug.Log("[SaveDataManager] Imported save data from Steam Cloud (fresh install).");
    }

    private const string KeyHasLocalSaveData = "SaveData_HasLocalData";

    // Local persistence only - cheap, so this stays on every write. The Steam
    // Cloud push is intentionally separate (see PushToSteamCloud()) and only
    // fires at scene-exit/quit checkpoints, not on every write.
    private void Commit()
    {
        PlayerPrefs.SetInt(KeyHasLocalSaveData, 1);
        PlayerPrefs.Save();
    }

    // Called from scene-exit/quit checkpoints only (OnApplicationQuit below,
    // and MainMenu.OnGameStartClicked / PauseMenu.OnHomeButtonPressed /
    // RetryMenu.OnMainMenuButtonPressed), not from every write - keeps
    // FileWrite calls rare instead of firing on every match/hit/defeat.
    public void PushToSteamCloud()
    {
        // Checked before BuildSnapshotJson(), not after: IsInitialized is false on
        // every non-Windows platform and whenever Steam isn't actually running, so
        // this skips building (and immediately discarding) the snapshot in both cases.
        if (SteamManager.Instance != null && SteamManager.Instance.IsInitialized)
        {
            SteamManager.Instance.PushSaveSnapshot(BuildSnapshotJson());
        }
    }

    // Safety net covering every quit path, including abrupt/OS-level closes
    // (Alt+F4, window close, Steam overlay "Exit Game") that never reach
    // SceneTransition.FadeAndQuit()'s explicit push.
    private void OnApplicationQuit()
    {
        PushToSteamCloud();
    }

    // ── Audio ─────────────────────────────────────────────────────────────────

    private const string MasterVolumePref = "Audio_MasterVolume";
    private const string BGMVolumePref    = "Audio_BGMVolume";
    private const string SFXVolumePref    = "Audio_SFXVolume";

    public const float DefaultVolume = 0.5f;

    public float LoadMasterVolume() => PlayerPrefs.GetFloat(MasterVolumePref, DefaultVolume);
    public float LoadBGMVolume()    => PlayerPrefs.GetFloat(BGMVolumePref,    DefaultVolume);
    public float LoadSFXVolume()    => PlayerPrefs.GetFloat(SFXVolumePref,    DefaultVolume);

    public void SaveAudioVolumes(float master, float bgm, float sfx)
    {
        PlayerPrefs.SetFloat(MasterVolumePref, master);
        PlayerPrefs.SetFloat(BGMVolumePref,    bgm);
        PlayerPrefs.SetFloat(SFXVolumePref,    sfx);
        Commit();
    }

    // ── Tutorial / Progress ───────────────────────────────────────────────────

    private const string KeyProgress = "tutorialProgress";

    public TutorialProgress Progress => (TutorialProgress)PlayerPrefs.GetInt(KeyProgress, 0);

    public void SetMedium()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Medium);
        Commit();
    }

    public void SetHard()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Hard);
        Commit();
    }

    public void SetChallenge()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Challenge);
        Commit();
    }

    public void ResetToEasy()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Easy);
        Commit();
    }

    public void ResetToMedium()
    {
        PlayerPrefs.SetInt(KeyProgress, (int)TutorialProgress.Medium);
        Commit();
    }

    // No ResetToChallenge — Challenge unlocks the ranking system and is permanent.

    private const string KeyTutorialCompleted = "tutorialCompleted_";

    public bool IsTutorialCompleted(TutorialProgress level)
    {
        return PlayerPrefs.GetInt(KeyTutorialCompleted + (int)level, 0) == 1;
    }

    public void SetTutorialCompleted(TutorialProgress level)
    {
        PlayerPrefs.SetInt(KeyTutorialCompleted + (int)level, 1);
        Commit();
    }

    // ── Enemy Defeat Counts ───────────────────────────────────────────────────

    private const string KeyEnemyDefeatCountPrefix = "enemyDefeatCount_";

    // Precomputed once per enum value - string + enum concatenation boxes the
    // enum and calls Enum.ToString(), allocating twice per call; doing that once
    // here instead of on every Get/Set avoids paying it on every CommitAndPush().
    private static readonly string[] EnemyDefeatCountKeys = BuildEnemyDefeatCountKeys();

    private static string[] BuildEnemyDefeatCountKeys()
    {
        CharacterId[] values = (CharacterId[])Enum.GetValues(typeof(CharacterId));
        string[] keys = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            keys[(int)values[i]] = KeyEnemyDefeatCountPrefix + values[i];
        }
        return keys;
    }

    public int GetEnemyDefeatCount(CharacterId characterId)
    {
        return PlayerPrefs.GetInt(EnemyDefeatCountKeys[(int)characterId], 0);
    }

    public int IncrementEnemyDefeatCount(CharacterId characterId)
    {
        int count = GetEnemyDefeatCount(characterId) + 1;
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)characterId], count);
        Commit();
        return count;
    }

    // ── Damage Dealt ──────────────────────────────────────────────────────────

    private const string KeyDamageDealtPrefix = "damageDealt_";

    private static readonly string[] DamageDealtKeys = BuildDamageDealtKeys();

    private static string[] BuildDamageDealtKeys()
    {
        GemColor[] values = (GemColor[])Enum.GetValues(typeof(GemColor));
        string[] keys = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            keys[(int)values[i]] = KeyDamageDealtPrefix + values[i];
        }
        return keys;
    }

    public int GetDamageDealt(GemColor color)
    {
        return PlayerPrefs.GetInt(DamageDealtKeys[(int)color], 0);
    }

    public int AddDamageDealt(GemColor color, int damage)
    {
        int total = GetDamageDealt(color) + damage;
        PlayerPrefs.SetInt(DamageDealtKeys[(int)color], total);
        Commit();
        return total;
    }

    // ── Jewel Match Counts ────────────────────────────────────────────────────

    private const string KeyJewelMatchCountPrefix = "jewelMatchCount_";

    // [colorOrdinal] holds the None-style aggregate key (tier omitted) - only ever
    // read for GemColor.None, where BuildJewelMatchCountKey ignores tier entirely.
    private static readonly string[] JewelMatchColorKeys = BuildJewelMatchColorKeys();
    // [colorOrdinal, tierIndex] holds the per-tier key for every other color.
    private static readonly string[,] JewelMatchColorTierKeys = BuildJewelMatchColorTierKeys();

    private static string[] BuildJewelMatchColorKeys()
    {
        GemColor[] colors = (GemColor[])Enum.GetValues(typeof(GemColor));
        string[] keys = new string[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            keys[(int)colors[i]] = KeyJewelMatchCountPrefix + colors[i];
        }
        return keys;
    }

    private static string[,] BuildJewelMatchColorTierKeys()
    {
        GemColor[] colors = (GemColor[])Enum.GetValues(typeof(GemColor));
        MatchTier[] tiers = (MatchTier[])Enum.GetValues(typeof(MatchTier));
        string[,] keys = new string[colors.Length, tiers.Length];
        for (int c = 0; c < colors.Length; c++)
        {
            for (int t = 0; t < tiers.Length; t++)
            {
                keys[(int)colors[c], TierIndex(tiers[t])] = KeyJewelMatchCountPrefix + colors[c] + "_" + tiers[t];
            }
        }
        return keys;
    }

    private static int TierIndex(MatchTier tier) => (int)tier - (int)MatchTier.Three;

    public int GetJewelMatchCount(GemColor color, MatchTier tier)
    {
        return PlayerPrefs.GetInt(BuildJewelMatchCountKey(color, tier), 0);
    }

    public int AddJewelMatchCount(GemColor color, MatchTier tier, int delta)
    {
        string key = BuildJewelMatchCountKey(color, tier);
        int total = PlayerPrefs.GetInt(key, 0) + delta;
        PlayerPrefs.SetInt(key, total);
        Commit();
        return total;
    }

    // GemColor.None collapses all tiers into one counter, so its key omits the tier suffix.
    private static string BuildJewelMatchCountKey(GemColor color, MatchTier tier)
    {
        return color == GemColor.None
            ? JewelMatchColorKeys[(int)color]
            : JewelMatchColorTierKeys[(int)color, TierIndex(tier)];
    }

    // ── Max Combo ─────────────────────────────────────────────────────────────

    private const string KeyMaxCombo = "maxComboRecorded";

    public int GetMaxCombo() => PlayerPrefs.GetInt(KeyMaxCombo, 0);

    public void TrySetMaxCombo(int combo)
    {
        if (combo > GetMaxCombo())
        {
            PlayerPrefs.SetInt(KeyMaxCombo, combo);
            Commit();
        }
    }

    // ── Reward Pick Counts ────────────────────────────────────────────────────

    private const string KeyRewardPickCountPrefix = "rewardPickCount_";

    private static readonly string[] RewardPickCountKeys = BuildRewardPickCountKeys();

    private static string[] BuildRewardPickCountKeys()
    {
        RewardId[] values = (RewardId[])Enum.GetValues(typeof(RewardId));
        string[] keys = new string[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            keys[(int)values[i]] = KeyRewardPickCountPrefix + values[i];
        }
        return keys;
    }

    public int GetRewardPickCount(RewardId rewardId)
    {
        return PlayerPrefs.GetInt(RewardPickCountKeys[(int)rewardId], 0);
    }

    public int IncrementRewardPickCount(RewardId rewardId)
    {
        int count = GetRewardPickCount(rewardId) + 1;
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)rewardId], count);
        Commit();
        return count;
    }

    // ── Best Score ────────────────────────────────────────────────────────────

    private const string KeyBestScore = "bestScoreLifetime";

    public int GetBestScore() => PlayerPrefs.GetInt(KeyBestScore, 0);

    // Returns whether score was actually a new best - callers use this to decide
    // whether to also persist that run's reward pick order via
    // SetBestScoreRewardHistory (see DefeatedTransitionController).
    public bool TrySetBestScore(int score)
    {
        if (score > GetBestScore())
        {
            PlayerPrefs.SetInt(KeyBestScore, score);
            Commit();
            return true;
        }
        return false;
    }

    // ── Best Score Reward History ────────────────────────────────────────────

    // Which rewards were picked, in order, during the run that set the current
    // best score - see ScoreStatsBinder's reward history grid. Stored as a
    // comma-separated list of RewardId ordinals since PlayerPrefs has no
    // native list/array support.
    private const string KeyBestScoreRewardHistory = "bestScoreRewardHistory";

    public RewardId[] GetBestScoreRewardHistory()
    {
        return ParseRewardHistoryCsv(PlayerPrefs.GetString(KeyBestScoreRewardHistory, string.Empty));
    }

    public void SetBestScoreRewardHistory(IReadOnlyList<RewardId> rewardHistory)
    {
        PlayerPrefs.SetString(KeyBestScoreRewardHistory, BuildRewardHistoryCsv(rewardHistory));
        Commit();
    }

    private static string BuildRewardHistoryCsv(IReadOnlyList<RewardId> rewardHistory)
    {
        if (rewardHistory.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < rewardHistory.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(',');
            }
            builder.Append((int)rewardHistory[i]);
        }
        return builder.ToString();
    }

    private static RewardId[] ParseRewardHistoryCsv(string csv)
    {
        if (string.IsNullOrEmpty(csv))
        {
            return Array.Empty<RewardId>();
        }

        string[] tokens = csv.Split(',');
        RewardId[] rewardIds = new RewardId[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
        {
            rewardIds[i] = (RewardId)int.Parse(tokens[i]);
        }
        return rewardIds;
    }

    // ── Steam Cloud Snapshot ──────────────────────────────────────────────────

    // Named fields, not enum-ordinal arrays: JsonUtility leaves missing fields at
    // their C# default when deserializing an older/shorter snapshot, so this shape
    // safely tolerates new fields being appended later. An ordinal-indexed array
    // would silently misalign every value if an enum member were ever inserted
    // mid-sequence instead of appended, with no compiler or runtime signal.
    [Serializable]
    private class SaveSnapshot
    {
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;

        public int tutorialProgress;
        public bool tutorialCompletedEasy;
        public bool tutorialCompletedMedium;
        public bool tutorialCompletedHard;
        public bool tutorialCompletedChallenge;

        public int enemyDefeatCountTroop;
        public int enemyDefeatCountSlimeKing;
        public int enemyDefeatCountAntGladiator;
        public int enemyDefeatCountMushroomBully;
        public int enemyDefeatCountFoxThief;
        public int enemyDefeatCountSnailWizard;
        public int enemyDefeatCountDandelionToad;
        public int enemyDefeatCountFlyingFish;

        public int damageDealtRed;
        public int damageDealtGreen;
        public int damageDealtBlue;
        public int damageDealtYellow;

        public int jewelMatchNone;
        public int jewelMatchRedThree;
        public int jewelMatchRedFour;
        public int jewelMatchRedFive;
        public int jewelMatchGreenThree;
        public int jewelMatchGreenFour;
        public int jewelMatchGreenFive;
        public int jewelMatchBlueThree;
        public int jewelMatchBlueFour;
        public int jewelMatchBlueFive;
        public int jewelMatchYellowThree;
        public int jewelMatchYellowFour;
        public int jewelMatchYellowFive;

        public int maxCombo;
        public int bestScore;

        public int rewardPickCountPowerUpRed;
        public int rewardPickCountPowerUpGreen;
        public int rewardPickCountPowerUpBlue;
        public int rewardPickCountPowerUpYellow;
        public int rewardPickCountSharpAttackRed;
        public int rewardPickCountSharpAttackGreen;
        public int rewardPickCountSharpAttackBlue;
        public int rewardPickCountSharpAttackYellow;
        public int rewardPickCountBerserked;
        public int rewardPickCountBlessings;
        public int rewardPickCountFocus;
        public int rewardPickCountFortify;

        public string bestScoreRewardHistory;
    }

    private string BuildSnapshotJson()
    {
        SaveSnapshot snapshot = new SaveSnapshot
        {
            masterVolume = LoadMasterVolume(),
            bgmVolume    = LoadBGMVolume(),
            sfxVolume    = LoadSFXVolume(),

            tutorialProgress           = (int)Progress,
            tutorialCompletedEasy      = IsTutorialCompleted(TutorialProgress.Easy),
            tutorialCompletedMedium    = IsTutorialCompleted(TutorialProgress.Medium),
            tutorialCompletedHard      = IsTutorialCompleted(TutorialProgress.Hard),
            tutorialCompletedChallenge = IsTutorialCompleted(TutorialProgress.Challenge),

            enemyDefeatCountTroop         = GetEnemyDefeatCount(CharacterId.Troop),
            enemyDefeatCountSlimeKing     = GetEnemyDefeatCount(CharacterId.SlimeKing),
            enemyDefeatCountAntGladiator  = GetEnemyDefeatCount(CharacterId.AntGladiator),
            enemyDefeatCountMushroomBully = GetEnemyDefeatCount(CharacterId.MushroomBully),
            enemyDefeatCountFoxThief      = GetEnemyDefeatCount(CharacterId.FoxThief),
            enemyDefeatCountSnailWizard   = GetEnemyDefeatCount(CharacterId.SnailWizard),
            enemyDefeatCountDandelionToad = GetEnemyDefeatCount(CharacterId.DandelionToad),
            enemyDefeatCountFlyingFish    = GetEnemyDefeatCount(CharacterId.FlyingFish),

            damageDealtRed    = GetDamageDealt(GemColor.Red),
            damageDealtGreen  = GetDamageDealt(GemColor.Green),
            damageDealtBlue   = GetDamageDealt(GemColor.Blue),
            damageDealtYellow = GetDamageDealt(GemColor.Yellow),

            jewelMatchNone        = GetJewelMatchCount(GemColor.None, MatchTier.Three),
            jewelMatchRedThree    = GetJewelMatchCount(GemColor.Red, MatchTier.Three),
            jewelMatchRedFour     = GetJewelMatchCount(GemColor.Red, MatchTier.Four),
            jewelMatchRedFive     = GetJewelMatchCount(GemColor.Red, MatchTier.Five),
            jewelMatchGreenThree  = GetJewelMatchCount(GemColor.Green, MatchTier.Three),
            jewelMatchGreenFour   = GetJewelMatchCount(GemColor.Green, MatchTier.Four),
            jewelMatchGreenFive   = GetJewelMatchCount(GemColor.Green, MatchTier.Five),
            jewelMatchBlueThree   = GetJewelMatchCount(GemColor.Blue, MatchTier.Three),
            jewelMatchBlueFour    = GetJewelMatchCount(GemColor.Blue, MatchTier.Four),
            jewelMatchBlueFive    = GetJewelMatchCount(GemColor.Blue, MatchTier.Five),
            jewelMatchYellowThree = GetJewelMatchCount(GemColor.Yellow, MatchTier.Three),
            jewelMatchYellowFour  = GetJewelMatchCount(GemColor.Yellow, MatchTier.Four),
            jewelMatchYellowFive  = GetJewelMatchCount(GemColor.Yellow, MatchTier.Five),

            maxCombo  = GetMaxCombo(),
            bestScore = GetBestScore(),

            rewardPickCountPowerUpRed      = GetRewardPickCount(RewardId.PowerUpRed),
            rewardPickCountPowerUpGreen    = GetRewardPickCount(RewardId.PowerUpGreen),
            rewardPickCountPowerUpBlue     = GetRewardPickCount(RewardId.PowerUpBlue),
            rewardPickCountPowerUpYellow   = GetRewardPickCount(RewardId.PowerUpYellow),
            rewardPickCountSharpAttackRed    = GetRewardPickCount(RewardId.SharpAttackRed),
            rewardPickCountSharpAttackGreen  = GetRewardPickCount(RewardId.SharpAttackGreen),
            rewardPickCountSharpAttackBlue   = GetRewardPickCount(RewardId.SharpAttackBlue),
            rewardPickCountSharpAttackYellow = GetRewardPickCount(RewardId.SharpAttackYellow),
            rewardPickCountBerserked = GetRewardPickCount(RewardId.Berserked),
            rewardPickCountBlessings = GetRewardPickCount(RewardId.Blessings),
            rewardPickCountFocus     = GetRewardPickCount(RewardId.Focus),
            rewardPickCountFortify   = GetRewardPickCount(RewardId.Fortify),

            bestScoreRewardHistory = PlayerPrefs.GetString(KeyBestScoreRewardHistory, string.Empty),
        };

        return JsonUtility.ToJson(snapshot);
    }

    // Does not call PlayerPrefs.Save() itself - the caller commits once all fields
    // are written (see Start()'s bootstrap-import path).
    private bool ApplySnapshotJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return false;
        }

        SaveSnapshot snapshot;
        try
        {
            snapshot = JsonUtility.FromJson<SaveSnapshot>(json);
        }
        catch (ArgumentException exception)
        {
            Debug.LogError($"[SaveDataManager] Cloud save data failed to parse: {exception.Message}");
            return false;
        }

        if (snapshot == null)
        {
            Debug.LogError("[SaveDataManager] Cloud save data parsed to null; ignoring.");
            return false;
        }

        PlayerPrefs.SetFloat(MasterVolumePref, snapshot.masterVolume);
        PlayerPrefs.SetFloat(BGMVolumePref, snapshot.bgmVolume);
        PlayerPrefs.SetFloat(SFXVolumePref, snapshot.sfxVolume);

        PlayerPrefs.SetInt(KeyProgress, snapshot.tutorialProgress);
        PlayerPrefs.SetInt(KeyTutorialCompleted + (int)TutorialProgress.Easy,      snapshot.tutorialCompletedEasy      ? 1 : 0);
        PlayerPrefs.SetInt(KeyTutorialCompleted + (int)TutorialProgress.Medium,    snapshot.tutorialCompletedMedium    ? 1 : 0);
        PlayerPrefs.SetInt(KeyTutorialCompleted + (int)TutorialProgress.Hard,      snapshot.tutorialCompletedHard      ? 1 : 0);
        PlayerPrefs.SetInt(KeyTutorialCompleted + (int)TutorialProgress.Challenge, snapshot.tutorialCompletedChallenge ? 1 : 0);

        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.Troop],         snapshot.enemyDefeatCountTroop);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.SlimeKing],     snapshot.enemyDefeatCountSlimeKing);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.AntGladiator],  snapshot.enemyDefeatCountAntGladiator);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.MushroomBully], snapshot.enemyDefeatCountMushroomBully);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.FoxThief],      snapshot.enemyDefeatCountFoxThief);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.SnailWizard],   snapshot.enemyDefeatCountSnailWizard);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.DandelionToad], snapshot.enemyDefeatCountDandelionToad);
        PlayerPrefs.SetInt(EnemyDefeatCountKeys[(int)CharacterId.FlyingFish],    snapshot.enemyDefeatCountFlyingFish);

        PlayerPrefs.SetInt(DamageDealtKeys[(int)GemColor.Red],    snapshot.damageDealtRed);
        PlayerPrefs.SetInt(DamageDealtKeys[(int)GemColor.Green],  snapshot.damageDealtGreen);
        PlayerPrefs.SetInt(DamageDealtKeys[(int)GemColor.Blue],   snapshot.damageDealtBlue);
        PlayerPrefs.SetInt(DamageDealtKeys[(int)GemColor.Yellow], snapshot.damageDealtYellow);

        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.None, MatchTier.Three),   snapshot.jewelMatchNone);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Red, MatchTier.Three),    snapshot.jewelMatchRedThree);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Red, MatchTier.Four),     snapshot.jewelMatchRedFour);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Red, MatchTier.Five),     snapshot.jewelMatchRedFive);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Green, MatchTier.Three),  snapshot.jewelMatchGreenThree);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Green, MatchTier.Four),   snapshot.jewelMatchGreenFour);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Green, MatchTier.Five),   snapshot.jewelMatchGreenFive);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Blue, MatchTier.Three),   snapshot.jewelMatchBlueThree);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Blue, MatchTier.Four),    snapshot.jewelMatchBlueFour);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Blue, MatchTier.Five),    snapshot.jewelMatchBlueFive);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Yellow, MatchTier.Three), snapshot.jewelMatchYellowThree);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Yellow, MatchTier.Four),  snapshot.jewelMatchYellowFour);
        PlayerPrefs.SetInt(BuildJewelMatchCountKey(GemColor.Yellow, MatchTier.Five),  snapshot.jewelMatchYellowFive);

        PlayerPrefs.SetInt(KeyMaxCombo, snapshot.maxCombo);
        PlayerPrefs.SetInt(KeyBestScore, snapshot.bestScore);

        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.PowerUpRed],      snapshot.rewardPickCountPowerUpRed);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.PowerUpGreen],    snapshot.rewardPickCountPowerUpGreen);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.PowerUpBlue],     snapshot.rewardPickCountPowerUpBlue);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.PowerUpYellow],   snapshot.rewardPickCountPowerUpYellow);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.SharpAttackRed],    snapshot.rewardPickCountSharpAttackRed);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.SharpAttackGreen],  snapshot.rewardPickCountSharpAttackGreen);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.SharpAttackBlue],   snapshot.rewardPickCountSharpAttackBlue);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.SharpAttackYellow], snapshot.rewardPickCountSharpAttackYellow);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.Berserked], snapshot.rewardPickCountBerserked);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.Blessings], snapshot.rewardPickCountBlessings);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.Focus],     snapshot.rewardPickCountFocus);
        PlayerPrefs.SetInt(RewardPickCountKeys[(int)RewardId.Fortify],   snapshot.rewardPickCountFortify);

        PlayerPrefs.SetString(KeyBestScoreRewardHistory, snapshot.bestScoreRewardHistory ?? string.Empty);

        return true;
    }
}
