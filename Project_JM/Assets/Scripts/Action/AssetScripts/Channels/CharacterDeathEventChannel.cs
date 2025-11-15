// SPDX-License-Identifier: MIT
// Copyright (c) 11/14/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterDeathEventChannel.cs
// Summary: A channel scriptable object to broadcast when a character died.

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDeathEventChannel", menuName = "JM/Events/Character Death Event Channel")]
public class CharacterDeathEventChannel : ScriptableObject
{
    public event Action<CharacterStatus> OnRaised;
    public void Raise(CharacterStatus stat) => OnRaised?.Invoke(stat);
}
