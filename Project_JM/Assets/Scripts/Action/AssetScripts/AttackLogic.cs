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

    // Must always be a valid (non-null) Transform before TakeDamage is called.
    public Transform HitTransform;
public Vector3 HitTransformOffset;

    public Vector3 GetHitPosition() => HitTransform.position + HitTransformOffset;

    public DamageMultiplierManager DamageMultiplierManager;
    public DamageRecordManager DamageRecordManager;

    // Null for enemy attacks; set to the match tier that triggered this ally attack.
    public MatchTier? Tier;

    public GameObject ImpactAttachPrefab;
    public Vector3 ImpactAttachLocalOffset;
}

public abstract class AttackLogic : ScriptableObject
{
    public abstract IEnumerator Execute(AttackContext context);

    // Simple getter for get attacked movement offset
    public abstract Vector3 GetTargetMotionOffset();
}
