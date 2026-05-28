// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JuicyDropdownStyleSO.cs
// Summary: Tuning data for JuicyDropdown — button state colors and panel animation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/UI/Style/Juicy Dropdown")]
public class JuicyDropdownStyleSO : ScriptableObject
{
    [Header("Button State Colors")]
    public Color normalColor   = Color.white;
    public Color hoveredColor  = Color.white;
    public Color pressedColor  = Color.white;
    public Color disabledColor = Color.gray;
    public float colorDuration = 0.1f;

    [Header("Panel Animation")]
    public float animDuration = 0.2f;
    public Ease  showEase     = Ease.OutCubic;
}
