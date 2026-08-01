// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardChosenEventChannel.cs
// Summary: A scriptable object for reward chosen event channel.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Events/Reward Chosen Event Channel")]
public class RewardChosenEventChannel : ScriptableObject
{
    public event Action OnRaised;
    public void Raise() => OnRaised?.Invoke();
}
