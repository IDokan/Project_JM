// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardManager.cs
// Summary: Submits the player's end-of-run score to Firestore on Challenge difficulty or above.
//          Triggered automatically when EndTransitionBegin fires.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using System.Globalization;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using TutorialEnums;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
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

            _auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsFaulted || authTask.IsCanceled)
                {
                    Debug.LogError("Firebase anonymous sign-in failed.");
                    return;
                }

                _isReady = true;
            });
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

        SubmitScore(ScoreManager.Instance.TotalScore);
    }

    private void SubmitScore(int score)
    {
        if (!_isReady)
        {
            Debug.LogWarning("LeaderboardManager: Firebase not ready, score not submitted.");
            return;
        }

        var data = new Dictionary<string, object>
        {
            { "score", score },
            { "userId", _auth.CurrentUser.UserId },
        };

        _db.Collection(WeeklyCollectionName()).AddAsync(data).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError($"Score submission failed: {task.Exception?.Message}");
                return;
            }

            Debug.Log($"Score {score} submitted.");
        });
    }

    private static string WeeklyCollectionName()
    {
        var now = System.DateTime.UtcNow;
        int week = ISOWeek.GetWeekOfYear(now);
        return $"leaderboard_{now.Year}_W{week:D2}";
    }
}
