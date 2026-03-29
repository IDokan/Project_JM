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
}
