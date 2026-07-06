// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 06/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAlertEventChannel.cs
// Summary: A channel scriptable object to broadcast the peeked next-enemy prefab before it spawns.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAlertEventChannel", menuName = "JM/Events/Enemy Alert Event Channel")]
public class EnemyAlertEventChannel : ScriptableObject
{
    public event Action<GameObject> OnRaised;
    public void Raise(GameObject enemyPrefab) => OnRaised?.Invoke(enemyPrefab);
}
