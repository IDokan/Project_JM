// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyStateVisual.cs
// Summary: Base class for enemy-specific visual responses to AI state changes driven by EnemyAttackMotion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

public class EnemyStateVisual : MonoBehaviour
{
    public virtual void OnEnraged() { }
    public virtual void OnStunBegin() { }
    public virtual void OnStunEnd() { }
    public virtual void OnDied() { }
    public virtual void OnWin() { }
    // Ownership transfers to EnemyAttackMotion — caller kills, stores, and applies time scale. If non-null, call SetLink(gameObject) before returning.
    public virtual Sequence BuildAttackSequence(Vector3 moveOffset) => null;
    public virtual void OnAttackEnd() { }
    // Ownership transfers to EnemyAttackMotion — caller kills, stores, and applies time scale. If non-null, call SetLink(gameObject) before returning.
    public virtual Sequence BuildEnragedSequence() => null;
}
