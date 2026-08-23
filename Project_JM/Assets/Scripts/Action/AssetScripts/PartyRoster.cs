// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PartyRoster.cs
// Summary: A scriptable object for party roster.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using GemEnums;
using System.Collections.Generic;

public class PartyRoster : MonoBehaviour
{
    [System.Serializable]
    public struct Slot { public GemColor Color; public CharacterCombatant Character; public Transform CharacterTransform; }
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

    public CharacterCombatant[] GetAll()
    {
        CharacterCombatant[] characters = new CharacterCombatant[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            characters[i] = slots[i].Character;
        }

        return characters;
    }

    public Transform GetCharacterTransform(GemColor color)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].Color == color)
            {
                return slots[i].CharacterTransform;
            }
        }

        return null;
    }

    public Transform[] GetAllCharacterTransforms()
    {
        Transform[] transforms = new Transform[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            transforms[i] = slots[i].CharacterTransform;
        }

        return transforms;
    }

    // Index-aligned with GetAllCharacterTransforms — both iterate the same
    // slots array in the same order, so callers can zip the two results
    // together by index (e.g. tinting a particle by which character it's
    // headed toward).
    public GemColor[] GetAllCharacterColors()
    {
        GemColor[] colors = new GemColor[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            colors[i] = slots[i].Color;
        }

        return colors;
    }
}
