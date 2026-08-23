// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 04/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageRecordManager.cs
// Summary: Accumulates damage dealt to the current enemy, grouped by gem color,
//          so the middle-transition result panel can show each character's contribution.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using GemEnums;
using UnityEngine;

public class DamageRecordManager : MonoBehaviour
{
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;

    protected readonly Dictionary<GemColor, int> _damageByColor = new Dictionary<GemColor, int>();

    protected void Awake()
    {
        ResetDamage();
    }

    protected void OnEnable()
    {
        enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
    }

    protected void OnDisable()
    {
        enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
    }

    public void RecordDamage(GemColor color, int damage)
    {
        if (color == GemColor.None || damage <= 0)
        {
            return;
        }

        _damageByColor.TryGetValue(color, out int current);
        _damageByColor[color] = current + damage;
    }

    // Highest damage first; always contains one entry per playable gem color.
    public List<KeyValuePair<GemColor, int>> GetSortedDamage()
    {
        List<KeyValuePair<GemColor, int>> result = new List<KeyValuePair<GemColor, int>>(_damageByColor);
        result.Sort((KeyValuePair<GemColor, int> a, KeyValuePair<GemColor, int> b) => b.Value.CompareTo(a.Value));
        return result;
    }

    // EnemySpawnedEventChannel fires after the new enemy has registered with all
    // systems, so this only clears the previous enemy's totals once a fresh fight
    // has actually begun. Consumers that display "last enemy" results must read
    // GetSortedDamage() before this fires (e.g. at RewardTransitionStarts).
    protected void OnEnemySpawned(GameObject enemy)
    {
        ResetDamage();
    }

    protected void ResetDamage()
    {
        _damageByColor.Clear();
        for (int i = 0; i < GemColorUtility.PlayableGemColor.Length; i++)
        {
            _damageByColor[GemColorUtility.PlayableGemColor[i]] = 0;
        }
    }
}
