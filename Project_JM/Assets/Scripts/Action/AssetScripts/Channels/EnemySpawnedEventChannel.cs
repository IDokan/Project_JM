// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawnedEventChannel.cs
// Summary: A channel scriptable object to broadcast when an enemy spawned.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnedEventChannel", menuName = "JM/Events/Enemy Spawned Event Channel")]
public class EnemySpawnedEventChannel : ScriptableObject
{
    public event Action<GameObject> OnRaised;
    public void Raise(GameObject gameObject) => OnRaised?.Invoke(gameObject);
}
