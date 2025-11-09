// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PartyRoster.cs
// Summary: A scriptable object for party roster.

using UnityEngine;
using GemEnums;

public class PartyRoster : MonoBehaviour
{
    [System.Serializable]
    public struct Slot { public GemColor Color; public CharacterCombatant Character; }

    [SerializeField] protected Slot[] slots;

    public CharacterCombatant Get(GemColor color)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Color == color)
            {
                return slots[i].Character;
            }
        }

        return null;
    }
}
