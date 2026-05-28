// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MenuStyleSO.cs
// Summary: Tuning data for Menu show/hide scale animations via DOTween.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(menuName = "JM/UI/Style/Menu")]
public class MenuStyleSO : ScriptableObject
{
    [Header("Timing")]
    public float showDuration = 0.2f;
    public float hideDuration = 0.2f;
}
