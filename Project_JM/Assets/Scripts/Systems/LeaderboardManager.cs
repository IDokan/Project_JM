// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardManager.cs
// Summary: Submits the player's end-of-run score to Firestore on Challenge difficulty or above.
//          Schema: one document per qualifying score entry (docId tracked locally).
//          Three-layer offline resilience: Firestore persistence, sign-in retry before submit,
//          and PlayerPrefs queue for first-ever offline launch (supports multiple pending scores).
//          Local top-6 leaderboard in PlayerPrefs avoids Firestore reads on submission.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TutorialEnums;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    private const string PendingEntriesKey = "Leaderboard_PendingEntries";
    private const string LocalLeaderboardKey = "Leaderboard_LocalTop6";
    private const int MaxScoresPerPlayer = 6;

    // Entry for the data not pushed to leader board becuase it was offline.
    [Serializable]
    private class PendingEntry
    {
        public int score;
        public int icon0;
        public int icon1;
        public int icon2;
        public string collection;
    }

    [Serializable]
    private class PendingEntryList
    {
        public List<PendingEntry> entries = new List<PendingEntry>();
    }

    // Entry for data already in leader board.
    [Serializable]
    private class LocalEntry
    {
        public int score;
        public int icon0;
        public int icon1;
        public int icon2;
        public string docId;
        public string collection;
    }

    [Serializable]
    private class LocalLeaderboard
    {
        public List<LocalEntry> entries = new List<LocalEntry>();
    }

    [SerializeField] private TransitionEventChannel transitionEventChannel;

    private FirebaseAuth _auth;
    private FirebaseFirestore _db;
    private bool _isReady;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase unavailable: {task.Result}");
                return;
            }

            _auth = FirebaseAuth.DefaultInstance;
            _db = FirebaseFirestore.DefaultInstance;

            _db.Settings.PersistenceEnabled = true;

            TrySignIn();
        });
    }

    private void OnEnable() => transitionEventChannel.OnRaised += OnTransitionEvent;
    private void OnDisable() => transitionEventChannel.OnRaised -= OnTransitionEvent;

    private void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase != TransitionPhase.EndBoardMoveEnd)
        {
            return;
        }

        if (SaveDataManager.Instance.Progress < TutorialProgress.Challenge)
        {
            return;
        }

        SubmitScore(ScoreManager.Instance.TotalScore, 0, 0, 0);
    }

    private void TrySignIn(Action onSuccess = null, Action onFirstLaunchOffline = null)
    {
        if (_auth == null)
        {
            Debug.LogWarning("LeaderboardManager: Firebase not initialized.");
            return;
        }

        if (_auth.CurrentUser != null)
        {
            _isReady = true;
            SyncLocalLeaderboard(() => { FlushPendingScores(); onSuccess?.Invoke(); });
            return;
        }

        _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask =>
        {
            if (authTask.IsFaulted || authTask.IsCanceled)
            {
                Debug.LogWarning($"LeaderboardManager: Sign-in failed: {authTask.Exception?.Message}");
                onFirstLaunchOffline?.Invoke();
                return;
            }

            _isReady = true;
            SyncLocalLeaderboard(() => { FlushPendingScores(); onSuccess?.Invoke(); });
        });
    }

    private void SubmitScore(int score, int icon0, int icon1, int icon2)
    {
        if (!_isReady)
        {
            TrySignIn(
                onSuccess: () => SendToFirestore(score, WeeklyCollectionName(), icon0, icon1, icon2),
                onFirstLaunchOffline: () => SavePendingScore(score, icon0, icon1, icon2)
            );
            return;
        }

        SendToFirestore(score, WeeklyCollectionName(), icon0, icon1, icon2);
    }

    private void SendToFirestore(int score, string collection, int icon0, int icon1, int icon2)
    {
        LocalLeaderboard local = LoadLocalLeaderboard();
        List<LocalEntry> weekEntries = local.entries.Where(e => e.collection == collection).ToList();

        bool qualifies = weekEntries.Count < MaxScoresPerPlayer
            || score > weekEntries.Min(e => e.score);

        if (!qualifies)
        {
            return;
        }

        LocalEntry displaced = weekEntries.Count >= MaxScoresPerPlayer
            ? weekEntries.OrderBy(e => e.score).First()
            : null;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "score", score },
            { "userId", _auth.CurrentUser.UserId },
            { "icon0", icon0 },
            { "icon1", icon1 },
            { "icon2", icon2 },
        };

        _db.Collection(collection).AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Score submission failed: {task.Exception?.Message}");
                return;
            }

            if (displaced != null)
            {
                _db.Collection(displaced.collection).Document(displaced.docId).DeleteAsync();
                local.entries.Remove(displaced);
            }

            local.entries.Add(new LocalEntry
            {
                score = score,
                icon0 = icon0,
                icon1 = icon1,
                icon2 = icon2,
                docId = task.Result.Id,
                collection = collection,
            });
            SaveLocalLeaderboard(local);

            Debug.Log($"Score {score} submitted to {collection}.");
        });
    }

    private void SyncLocalLeaderboard(Action onComplete)
    {
        string collection = WeeklyCollectionName();

        _db.Collection(collection)
            .WhereEqualTo("userId", _auth.CurrentUser.UserId)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (!task.IsFaulted)
                {
                    LocalLeaderboard local = new LocalLeaderboard();

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        local.entries.Add(new LocalEntry
                        {
                            score = (int)doc.GetValue<long>("score"),
                            icon0 = (int)doc.GetValue<long>("icon0"),
                            icon1 = (int)doc.GetValue<long>("icon1"),
                            icon2 = (int)doc.GetValue<long>("icon2"),
                            docId = doc.Id,
                            collection = collection,
                        });
                    }

                    List<LocalEntry> excess = local.entries
                        .OrderBy(e => e.score)
                        .Take(local.entries.Count - MaxScoresPerPlayer)
                        .ToList();

                    foreach (LocalEntry entry in excess)
                    {
                        _db.Collection(entry.collection).Document(entry.docId).DeleteAsync();
                        local.entries.Remove(entry);
                    }

                    SaveLocalLeaderboard(local);
                }

                onComplete?.Invoke();
            });
    }

    private void SavePendingScore(int score, int icon0, int icon1, int icon2)
    {
        string collection = WeeklyCollectionName();
        PendingEntryList list = LoadPendingEntries();
        List<PendingEntry> weekEntries = list.entries.Where(e => e.collection == collection).ToList();

        bool qualifies = weekEntries.Count < MaxScoresPerPlayer
            || score > weekEntries.Min(e => e.score);

        if (!qualifies)
        {
            return;
        }

        if (weekEntries.Count >= MaxScoresPerPlayer)
        {
            list.entries.Remove(weekEntries.OrderBy(e => e.score).First());
        }

        list.entries.Add(new PendingEntry { score = score, icon0 = icon0, icon1 = icon1, icon2 = icon2, collection = collection });
        PlayerPrefs.SetString(PendingEntriesKey, JsonUtility.ToJson(list));
        PlayerPrefs.Save();
    }

    private void FlushPendingScores()
    {
        if (!PlayerPrefs.HasKey(PendingEntriesKey))
        {
            return;
        }

        PendingEntryList list = LoadPendingEntries();

        // Clear before SendToFirestore calls — Firestore persistence owns retry from this point
        PlayerPrefs.DeleteKey(PendingEntriesKey);
        PlayerPrefs.Save();

        foreach (PendingEntry entry in list.entries)
        {
            SendToFirestore(entry.score, entry.collection, entry.icon0, entry.icon1, entry.icon2);
        }
    }

    private PendingEntryList LoadPendingEntries()
    {
        string json = PlayerPrefs.GetString(PendingEntriesKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return new PendingEntryList();
        }

        return JsonUtility.FromJson<PendingEntryList>(json) ?? new PendingEntryList();
    }

    private LocalLeaderboard LoadLocalLeaderboard()
    {
        string json = PlayerPrefs.GetString(LocalLeaderboardKey, "");
        if (string.IsNullOrEmpty(json))
        {
            return new LocalLeaderboard();
        }

        return JsonUtility.FromJson<LocalLeaderboard>(json) ?? new LocalLeaderboard();
    }

    private void SaveLocalLeaderboard(LocalLeaderboard local)
    {
        PlayerPrefs.SetString(LocalLeaderboardKey, JsonUtility.ToJson(local));
        PlayerPrefs.Save();
    }

    private static string WeeklyCollectionName()
    {
        System.DateTime now = System.DateTime.UtcNow;
        int week = ISOWeek.GetWeekOfYear(now);
        return $"leaderboard_{now.Year}_W{week:D2}";
    }
}
