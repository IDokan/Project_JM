// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MatchEventChannel.cs
// Summary: A scriptable object for match event channel.


using System;
using UnityEngine;
using MatchEnums;

[CreateAssetMenu(menuName = "JM/Events/Match Event Channel")]
public class MatchEventChannel : ScriptableObject
{
    public event Action<MatchEvent> OnRaised;
    public void Raise(MatchEvent e) => OnRaised?.Invoke(e);
}
