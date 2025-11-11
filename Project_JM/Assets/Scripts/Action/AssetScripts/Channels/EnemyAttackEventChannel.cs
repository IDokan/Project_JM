// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackEventChannel.cs
// Summary: A scriptable object for enemy attacks event channel.

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Events/Enemy Attack Event Channel")]
public class EnemyAttackEventChannel : ScriptableObject
{
    public event Action<AttackLogic> OnRaised;
    public void Raise(AttackLogic logic) => OnRaised?.Invoke(logic);
}
