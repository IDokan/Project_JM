// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackLogic.cs
// Summary: An abstract scriptable object for attack logics.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using MatchEnums;
using System.Collections;

public struct AttackContext
{
    public ICombatant Attacker;
    public ICombatant Target;

    public Transform HitTransform;

    public DamageMultiplierManager DamageMultiplierManager;

    public GameObject ImpactAttachPrefab;
    public Vector3 ImpactAttachLocalOffset;
}

public abstract class AttackLogic : ScriptableObject
{
    public abstract IEnumerator Execute(AttackContext context);

    // Simple getter for get attacked movement offset
    public abstract Vector3 GetTargetMotionOffset();
}
