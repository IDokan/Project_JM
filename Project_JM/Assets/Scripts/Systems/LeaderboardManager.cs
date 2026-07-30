// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardManager.cs
// Summary: Submits the player's end-of-run score to Firestore for any difficulty.
//          Schema: one document per qualifying score entry (docId tracked locally).
//          Three-layer offline resilience: Firestore persistence, sign-in retry before submit,
//          and PlayerPrefs queue for first-ever offline launch (supports multiple pending scores).
//          Local top-6 leaderboard in PlayerPrefs avoids Firestore reads on submission.
//          Exposes FetchGlobalScores and GetMyScores for LeaderboardMenu to populate rows.
//          On sign-in, pending scores are flushed to Firestore before syncing so the local
//          cache always reflects the settled Firestore state after startup.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public struct LeaderboardEntry
{
    public int score;
    public int icon0;
    public int icon1;
    public int icon2;
    public bool isMyRecord;
    public bool isLiveEntry;
}

public class LeaderboardManager : MonoBehaviour
{
    private const string PendingEntriesKey = "Leaderboard_PendingEntries";
    private const string LocalLeaderboardKey = "Leaderboard_LocalTop6";
    public const int MaxScoresPerPlayer = 6;
    public const int GlobalFetchLimit = 50;

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

    private const float GlobalCacheExpirySeconds = 600f;

    public static LeaderboardManager Instance { get; private set; }

    private FirebaseAuth _auth;
    private FirebaseFirestore _db;
    private bool _isReady;
    private List<LeaderboardEntry> _cachedGlobalEntries;
    private bool _isFetchingGlobal;
    private float _lastGlobalFetchTime = float.MinValue;
    private readonly List<Action> _pendingGlobalCallbacks = new List<Action>();

    public bool HasCachedGlobalData => _cachedGlobalEntries != null;
    public IReadOnlyList<LeaderboardEntry> GetGlobalEntries() => _cachedGlobalEntries;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
            FlushPendingScores(() => SyncLocalLeaderboard(() => onSuccess?.Invoke()));
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
            FlushPendingScores(() => SyncLocalLeaderboard(() => onSuccess?.Invoke()));
        });
    }

    public void SubmitScore(int score, int icon0, int icon1, int icon2)
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

    private void SendToFirestore(int score, string collection, int icon0, int icon1, int icon2, Action onComplete = null)
    {
        LocalLeaderboard local = LoadLocalLeaderboard();
        List<LocalEntry> weekEntries = local.entries.Where(e => e.collection == collection).ToList();

        bool qualifies = weekEntries.Count < MaxScoresPerPlayer
            || score > weekEntries.Min(e => e.score);

        if (!qualifies)
        {
            onComplete?.Invoke();
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

        // Update local cache before the async write so GetMyScores() reflects the
    // new score immediately, even if the Firestore task doesn't complete until
    // connectivity returns. docId is empty until the server confirms.
    if (displaced != null)
    {
        local.entries.Remove(displaced);
    }
    local.entries.Add(new LocalEntry
    {
        score = score,
        icon0 = icon0,
        icon1 = icon1,
        icon2 = icon2,
        docId = string.Empty,
        collection = collection,
    });
    SaveLocalLeaderboard(local);
    UpdateGlobalCacheWithNewScore(score, icon0, icon1, icon2);

    _db.Collection(collection).AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Score submission failed: {task.Exception?.Message}");
                onComplete?.Invoke();
                return;
            }

            if (displaced != null)
            {
                _db.Collection(displaced.collection).Document(displaced.docId).DeleteAsync();
            }

            // Reload from PlayerPrefs in case other scores were written while waiting,
            // then stamp the real server-assigned docId onto the placeholder entry.
            LocalLeaderboard settled = LoadLocalLeaderboard();
            LocalEntry placeholder = settled.entries
                .FirstOrDefault(e => e.collection == collection && e.score == score && string.IsNullOrEmpty(e.docId));
            if (placeholder != null)
            {
                placeholder.docId = task.Result.Id;
                SaveLocalLeaderboard(settled);
            }

            Debug.Log($"Score {score} submitted to {collection}.");
            onComplete?.Invoke();
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
                        if (!doc.ContainsField("score"))
                        {
                            continue;
                        }

                        local.entries.Add(new LocalEntry
                        {
                            score = (int)doc.GetValue<long>("score"),
                            icon0 = doc.ContainsField("icon0") ? (int)doc.GetValue<long>("icon0") : 0,
                            icon1 = doc.ContainsField("icon1") ? (int)doc.GetValue<long>("icon1") : 0,
                            icon2 = doc.ContainsField("icon2") ? (int)doc.GetValue<long>("icon2") : 0,
                            docId = doc.Id,
                            collection = collection,
                        });
                    }

                    List<LocalEntry> excess = local.entries
                        .OrderBy(e => e.score)
                        .Take(Math.Max(0, local.entries.Count - MaxScoresPerPlayer))
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

    private void FlushPendingScores(Action onComplete = null)
    {
        if (!PlayerPrefs.HasKey(PendingEntriesKey))
        {
            onComplete?.Invoke();
            return;
        }

        PendingEntryList list = LoadPendingEntries();

        // Clear before SendToFirestore calls — Firestore persistence owns retry from this point
        PlayerPrefs.DeleteKey(PendingEntriesKey);
        PlayerPrefs.Save();

        if (list.entries.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        int remaining = list.entries.Count;
        foreach (PendingEntry entry in list.entries)
        {
            SendToFirestore(entry.score, entry.collection, entry.icon0, entry.icon1, entry.icon2, () =>
            {
                remaining--;
                if (remaining == 0)
                {
                    onComplete?.Invoke();
                }
            });
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

    private void UpdateGlobalCacheWithNewScore(int score, int icon0, int icon1, int icon2)
    {
        if (_cachedGlobalEntries == null) { return; }

        bool cacheNotFull = _cachedGlobalEntries.Count < GlobalFetchLimit;
        bool beatsLowest = _cachedGlobalEntries.Count > 0
            && score > _cachedGlobalEntries.Min(e => e.score);

        if (!cacheNotFull && !beatsLowest) { return; }

        if (!cacheNotFull)
        {
            int lowestScore = _cachedGlobalEntries.Min(e => e.score);
            int lowestIndex = _cachedGlobalEntries.FindIndex(e => e.score == lowestScore);
            _cachedGlobalEntries.RemoveAt(lowestIndex);
        }

        _cachedGlobalEntries.Add(new LeaderboardEntry
        {
            score = score,
            icon0 = icon0,
            icon1 = icon1,
            icon2 = icon2,
            isMyRecord = true,
        });

        _cachedGlobalEntries = _cachedGlobalEntries.OrderByDescending(e => e.score).ToList();
    }

    public static string WeeklyCollectionName()
    {
        System.DateTime now = System.DateTime.UtcNow;
        int week = ISOWeek.GetWeekOfYear(now);
        return $"leaderboard_{now.Year}_W{week:D2}";
    }

    public void FetchGlobalScores(Action onComplete)
    {
        if (!_isReady)
        {
            onComplete?.Invoke();
            return;
        }

        if (_cachedGlobalEntries != null
            && Time.realtimeSinceStartup - _lastGlobalFetchTime < GlobalCacheExpirySeconds)
        {
            onComplete?.Invoke();
            return;
        }

        if (_isFetchingGlobal)
        {
            if (onComplete != null) { _pendingGlobalCallbacks.Add(onComplete); }
            return;
        }

        _isFetchingGlobal = true;
        if (onComplete != null) { _pendingGlobalCallbacks.Add(onComplete); }

        string collection = WeeklyCollectionName();
        string myUserId = _auth.CurrentUser?.UserId;

        _db.Collection(collection)
            .OrderByDescending("score")
            .Limit(GlobalFetchLimit)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                _isFetchingGlobal = false;

                if (task.IsFaulted)
                {
                    Debug.LogError($"FetchGlobalScores failed: {task.Exception?.Message}");
                }
                else
                {
                    List<LeaderboardEntry> result = new List<LeaderboardEntry>();

                    foreach (DocumentSnapshot doc in task.Result.Documents)
                    {
                        if (!doc.ContainsField("score")) { continue; }

                        result.Add(new LeaderboardEntry
                        {
                            score = (int)doc.GetValue<long>("score"),
                            icon0 = doc.ContainsField("icon0") ? (int)doc.GetValue<long>("icon0") : 0,
                            icon1 = doc.ContainsField("icon1") ? (int)doc.GetValue<long>("icon1") : 0,
                            icon2 = doc.ContainsField("icon2") ? (int)doc.GetValue<long>("icon2") : 0,
                            isMyRecord = doc.ContainsField("userId") && doc.GetValue<string>("userId") == myUserId,
                        });
                    }

                    _cachedGlobalEntries = result;
                    _lastGlobalFetchTime = Time.realtimeSinceStartup;
                }

                foreach (Action cb in _pendingGlobalCallbacks) { cb?.Invoke(); }
                _pendingGlobalCallbacks.Clear();
            });
    }

    public List<LeaderboardEntry> GetMyScores()
    {
        string collection = WeeklyCollectionName();
        LocalLeaderboard local = LoadLocalLeaderboard();
        List<LeaderboardEntry> result = new List<LeaderboardEntry>();

        foreach (LocalEntry entry in local.entries
            .Where(e => e.collection == collection)
            .OrderByDescending(e => e.score))
        {
            result.Add(new LeaderboardEntry
            {
                score = entry.score,
                icon0 = entry.icon0,
                icon1 = entry.icon1,
                icon2 = entry.icon2,
                isMyRecord = true,
            });
        }

        return result;
    }
}


