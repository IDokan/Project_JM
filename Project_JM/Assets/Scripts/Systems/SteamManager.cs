// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SteamManager.cs
// Summary: Persistent singleton that initializes the Steamworks API, pumps
//          SteamAPI.RunCallbacks() every frame, and exposes UnlockAchievement() for
//          other systems to call. Steam calls only compile when the Active Build
//          Target is Windows Standalone (UNITY_STANDALONE_WIN covers both Editor
//          testing and real builds under that target - UNITY_EDITOR_WIN must NOT
//          be used here, it tracks the Editor's host OS and stays true even when the
//          Active Build Target is switched to Android). IsInitialized stays false on
//          every other platform so calling code never needs its own platform guards.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using AchievementEnums;
#if UNITY_STANDALONE_WIN
using System;
using Steamworks;
#endif
using UnityEngine;

public class SteamManager : MonoBehaviour
{
    public static SteamManager Instance { get; private set; }

    public bool IsInitialized { get; private set; }

#if UNITY_STANDALONE_WIN
    private const uint AppId = 4459610;

    private SteamAPIWarningMessageHook_t _warningMessageHook;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    private static void OnSteamAPIDebugTextHook(int severity, System.Text.StringBuilder debugText)
    {
        Debug.LogWarning(debugText);
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

        if (IsInitialized)
        {
            SteamAPI.Shutdown();
        }
    }
#endif
}
