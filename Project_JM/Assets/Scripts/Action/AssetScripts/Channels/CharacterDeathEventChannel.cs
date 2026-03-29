// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterDeathEventChannel.cs
// Summary: A channel scriptable object to broadcast when a character died.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDeathEventChannel", menuName = "JM/Events/Character Death Event Channel")]
public class CharacterDeathEventChannel : ScriptableObject
{
    public event Action<CharacterStatus> OnRaised;
    public void Raise(CharacterStatus stat) => OnRaised?.Invoke(stat);
}
