// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/13/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemPowerArrivedEventChannel.cs
// Summary: A scriptable object for event channel raised
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
//                  when gem power arrived to characters.


using System;
using UnityEngine;
using MatchEnums;

[CreateAssetMenu(menuName = "JM/Events/GemPowerArrivedEventChannel")]
public class GemPowerArrivedEventChannel : ScriptableObject
{
    public event Action<MatchEvent> OnRaised;
    public void Raise(MatchEvent e) => OnRaised?.Invoke(e);
}
