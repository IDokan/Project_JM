// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyBook.cs
// Summary: A scriptable object to contain enemy data.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[System.Serializable]
public struct EnemyEntry
{
    public GameObject EnemyPrefab;
}

[CreateAssetMenu(fileName = "EnemyBook", menuName = "JM/Data/EnemyBook")]
public class EnemyBook : ScriptableObject
{
    [SerializeField] protected EnemyEntry[] entries;

    public GameObject GetRandomEnemyPrefab()
    {
        if (entries.Length <= 0)
        {
            return null;
        }

        return entries[GlobalRNG.Instance.NextInt(entries.Length)].EnemyPrefab;
    }

    public GameObject GetRandomEnemyPrefabExcluding(GameObject excludePrefab)
    {
        if (entries.Length <= 0)
        {
            return null;
        }

        if (excludePrefab == null || entries.Length == 1)
        {
            return GetRandomEnemyPrefab();
        }

        int validCount = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].EnemyPrefab != excludePrefab)
            {
                validCount++;
            }
        }

        if (validCount == 0)
        {
            return GetRandomEnemyPrefab();
        }

        int pick = GlobalRNG.Instance.NextInt(validCount);
        int seen = 0;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].EnemyPrefab != excludePrefab)
            {
                if (seen == pick)
                {
                    return entries[i].EnemyPrefab;
                }
                seen++;
            }
        }

        return GetRandomEnemyPrefab();
    }
}
