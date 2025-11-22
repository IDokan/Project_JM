// SPDX-License-Identifier: MIT
// Copyright (c) 11/14/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemySpawnedEventChannel.cs
// Summary: A channel scriptable object to broadcast when an enemy spawned.

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySpawnedEventChannel", menuName = "JM/Events/Enemy Spawned Event Channel")]
public class EnemySpawnedEventChannel : ScriptableObject
{
    public event Action<GameObject> OnRaised;
    public void Raise(GameObject gameObject) => OnRaised?.Invoke(gameObject);
}
