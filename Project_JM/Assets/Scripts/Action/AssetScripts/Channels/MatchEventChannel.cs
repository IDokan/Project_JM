// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MatchEventChannel.cs
// Summary: A scriptable object for match event channel.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using System;
using UnityEngine;
using MatchEnums;

[CreateAssetMenu(menuName = "JM/Events/Match Event Channel")]
public class MatchEventChannel : ScriptableObject
{
    public event Action<MatchEvent> OnRaised;
    public void Raise(MatchEvent e) => OnRaised?.Invoke(e);
}
