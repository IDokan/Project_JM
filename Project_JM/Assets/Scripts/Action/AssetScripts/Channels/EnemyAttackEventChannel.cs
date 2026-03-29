// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackEventChannel.cs
// Summary: A scriptable object for enemy attacks event channel.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Events/Enemy Attack Event Channel")]
public class EnemyAttackEventChannel : ScriptableObject
{
    public event Action OnRaised;
    public void Raise() => OnRaised?.Invoke();
}
