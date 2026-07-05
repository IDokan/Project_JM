// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageRecordUIManager.cs
// Summary: Slides the damage record panel out from behind the gem board at the
//          start of the middle transition, and hides it again once the next enemy appears.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using DG.Tweening;
using GemEnums;
using UnityEngine;

public class DamageRecordUIManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected DamageRecordManager damageRecordManager;
    [SerializeField] protected DamageRecordItem[] items;

    [SerializeField] protected float hiddenX = -5.9f;
    [SerializeField] protected float shownX = -1.9f;
    [SerializeField] protected float slideDuration = 0.5f;

    protected Tweener _tween;

    protected void Awake()
    {
        Vector3 localPosition = transform.localPosition;
        localPosition.x = hiddenX;
        transform.localPosition = localPosition;
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
        enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
        enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            ShowRecords();
        }
    }

    protected void OnEnemySpawned(GameObject enemy)
    {
        HideRecords();
    }

    protected void ShowRecords()
    {
        List<KeyValuePair<GemColor, int>> sortedDamage = damageRecordManager.GetSortedDamage();
        int maxDamage = sortedDamage.Count > 0 ? sortedDamage[0].Value : 0;

        for (int i = 0; i < items.Length && i < sortedDamage.Count; i++)
        {
            items[i].SetData(sortedDamage[i].Key, sortedDamage[i].Value, maxDamage);
        }

        _tween?.Kill();
        _tween = transform.DOLocalMoveX(shownX, slideDuration).SetEase(Ease.OutCubic);
    }

    protected void HideRecords()
    {
        _tween?.Kill();
        _tween = transform.DOLocalMoveX(hiddenX, slideDuration).SetEase(Ease.InCubic);
    }
}
