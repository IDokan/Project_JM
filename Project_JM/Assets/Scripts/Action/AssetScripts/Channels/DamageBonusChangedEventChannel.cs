// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageBonusChangedEventChannel.cs
// Summary: Event channel raised whenever the damage bonus multiplier changes.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageBonusChangedEventChannel", menuName = "JM/Events/Damage Bonus Changed Event Channel")]
public class DamageBonusChangedEventChannel : ScriptableObject
{
    public event Action<float> OnRaised;
    public void Raise(float newBonus) => OnRaised?.Invoke(newBonus);
}
