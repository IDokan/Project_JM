// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyStateVisual.cs
// Summary: Base class for enemy-specific visual responses to AI state changes driven by EnemyAttackMotion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class EnemyStateVisual : MonoBehaviour
{
    public virtual void OnEnraged() { }
    public virtual void OnStunBegin() { }
    public virtual void OnStunEnd() { }
    public virtual void OnDied() { }
    public virtual void OnAttack(Vector3 moveOffset) { }
}
