// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardDefinition.cs
// Summary: An abstract scriptable object for reward logics.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using RewardEnums;
using UnityEngine;

public struct RewardContext
{
    public DamageMultiplierManager DamageMultiplierManager;

    // The party's single shared HP/crit status. See CharacterStatus — the party
    // has one status instance shared by all four colors, not one status each.
    public CharacterStatus PartyStatus;

    // Lets a reward reach every color's CharacterCombatant, e.g. to play hit
    // feedback (flash/SFX/damage popup) on all four when the shared HP changes.
    public PartyRoster Roster;

    public ComboManager ComboManager;
}

public abstract class RewardDefinition : ScriptableObject
{
    [SerializeField] protected RewardId id;

    public RewardId Id => id;

    // Used later to tint the selectable UI; GemColor.None for rewards with no single color.
    public abstract GemColor AssociatedColor { get; }

    public abstract IEnumerator Apply(RewardContext context);
}
