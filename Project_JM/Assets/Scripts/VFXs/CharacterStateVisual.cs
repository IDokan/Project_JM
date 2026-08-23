// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterStateVisual.cs
// Summary: Base class for ally-specific visual responses (face part swaps) to shared states driven by AttackMotion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class CharacterStateVisual : MonoBehaviour
{
    protected bool IsVictory { get; private set; }

    public virtual void OnVictory() => IsVictory = true;
    public virtual void OnVictoryEnd() => IsVictory = false;
    public virtual void OnDamagedBegin() { }
    public virtual void OnDamagedEnd() { }
    public virtual void OnDied() => OnDamagedBegin();
}
