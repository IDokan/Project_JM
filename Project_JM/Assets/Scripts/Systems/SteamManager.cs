// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SteamManager.cs
// Summary: Persistent singleton that initializes the Steamworks API, pumps
//          SteamAPI.RunCallbacks() every frame, exposes UnlockAchievement() for
//          other systems to call, and raises OnOverlayActivated when the Steam
//          Overlay opens/closes (controller home button, Shift+Tab, Steam Deck
//          combo - all surface through this one callback). Steam calls only
//          compile when the Active Build Target is Windows Standalone
//          (UNITY_STANDALONE_WIN covers both Editor testing and real builds under
//          that target - UNITY_EDITOR_WIN must NOT be used here, it tracks the
//          Editor's host OS and stays true even when the Active Build Target is
//          switched to Android). IsInitialized stays false on every other
//          platform so calling code never needs its own platform guards.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using AchievementEnums;
#if UNITY_STANDALONE_WIN
using Steamworks;
#endif
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    public bool IsInitialized { get; private set; }

    public static event Action<bool> OnOverlayActivated;

#if UNITY_STANDALONE_WIN
    private const uint AppId = 4459610;

    private SteamAPIWarningMessageHook_t _warningMessageHook;
    private Callback<GameOverlayActivated_t> _overlayActivatedCallback;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    private static void OnSteamAPIDebugTextHook(int severity, System.Text.StringBuilder debugText)
    {
        Debug.LogWarning(debugText);
    }

    private static void OnGameOverlayActivated(GameOverlayActivated_t callback)
    {
        OnOverlayActivated?.Invoke(callback.m_bActive != 0);
    }
#endif

    public void UnlockAchievement(AchievementId achievementId)
    {
#if UNITY_STANDALONE_WIN
        if (!IsInitialized)
        {
            return;
        }

        SteamUserStats.SetAchievement(GetAchievementApiName(achievementId));
        SteamUserStats.StoreStats();
#endif
    }

#if UNITY_STANDALONE_WIN
    private static string GetAchievementApiName(AchievementId achievementId)
    {
        switch (achievementId)
        {
            case AchievementId.Welcome:
                return "ACH_WELCOME";
            case AchievementId.FastLearner:
                return "ACH_FASTLEARNER";
            case AchievementId.SlimeKingSlayer:
                return "ACH_SLIME_KING_10";
            case AchievementId.AntGladiatorConqueror:
                return "ACH_ANT_GLADIATOR_10";
            case AchievementId.MushroomBullySubduer:
                return "ACH_MUSHROOM_BULLY_10";
            case AchievementId.FoxThiefHunter:
                return "ACH_FOX_THIEF_10";
            case AchievementId.SnailWizardVanquisher:
                return "ACH_SNAIL_WIZARD_10";
            case AchievementId.DandelionToadExterminator:
                return "ACH_DANDELION_TOAD_10";
            case AchievementId.KnightDamage10K:
                return "ACH_KNIGHT_DAMAGE_10K";
            case AchievementId.KnightDamage100K:
                return "ACH_KNIGHT_DAMAGE_100K";
            case AchievementId.BowmanDamage10K:
                return "ACH_BOWMAN_DAMAGE_10K";
            case AchievementId.BowmanDamage100K:
                return "ACH_BOWMAN_DAMAGE_100K";
            case AchievementId.MageDamage10K:
                return "ACH_MAGE_DAMAGE_10K";
            case AchievementId.MageDamage100K:
                return "ACH_MAGE_DAMAGE_100K";
            case AchievementId.ClericDamage10K:
                return "ACH_CLERIC_DAMAGE_10K";
            case AchievementId.ClericDamage100K:
                return "ACH_CLERIC_DAMAGE_100K";
            case AchievementId.BronzeCompetitor:
                return "ACH_SCORE_20K";
            case AchievementId.SilverCompetitor:
                return "ACH_SCORE_25K";
            case AchievementId.GoldCompetitor:
                return "ACH_SCORE_30K";
            default:
                Debug.LogError($"[SteamManager] No API name mapped for {achievementId}");
                return string.Empty;
        }
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_STANDALONE_WIN
        if (!Packsize.Test())
        {
            Debug.LogError("[SteamManager] Packsize.Test() failed; wrong Steamworks.NET binary for this platform.", this);
        }

        // DllCheck.Test() is a stubbed no-op in Steamworks.NET 2025.163.0 (InteropHelp.cs
        // gates the real check behind an unset DISABLED symbol); it always returns true,
        // so it was removed here. Re-check that source if this package is ever updated.

        try
        {
            // Checks whether this process was validly launched through the Steam client.
            // Returns true only when it wasn't (and no steam_appid.txt is present to cover
            // dev/Editor runs) - Steam then relaunches us properly and this process must quit.
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(AppId)))
            {
                Application.Quit();
                return;
            }
        }
        catch (DllNotFoundException exception)
        {
            Debug.LogError($"[SteamManager] Could not load steam_api64.dll: {exception}", this);
            Application.Quit();
            return;
        }

        IsInitialized = SteamAPI.Init();
        if (!IsInitialized)
        {
            Debug.LogError("[SteamManager] SteamAPI.Init() failed. Steam client must be running.", this);
            return;
        }

        _warningMessageHook = new SteamAPIWarningMessageHook_t(OnSteamAPIDebugTextHook);
        SteamClient.SetWarningMessageHook(_warningMessageHook);

        _overlayActivatedCallback = Callback<GameOverlayActivated_t>.Create(OnGameOverlayActivated);
#endif
    }

#if UNITY_STANDALONE_WIN
    // Must run in Update(), not FixedUpdate(): FixedUpdate stops firing entirely when
    // GlobalTimeManager pauses via Time.timeScale = 0, which would freeze Steam callback
    // dispatch (overlay, friend events, async call results) for the whole pause duration.
    // Update() still fires every rendered frame regardless of timeScale.
    private void Update()
    {
        if (IsInitialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        Instance = null;

        _overlayActivatedCallback?.Dispose();

        if (IsInitialized)
        {
            SteamAPI.Shutdown();
        }
    }
#endif
}
