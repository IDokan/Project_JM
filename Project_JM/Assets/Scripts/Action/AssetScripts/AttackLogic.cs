// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackLogic.cs
// Summary: An abstract scriptable object for attack logics.

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
