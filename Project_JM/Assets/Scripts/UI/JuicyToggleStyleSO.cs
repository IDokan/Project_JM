// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JuicyToggleStyleSO.cs
// Summary: Tuning data for JuicyToggle — colors, durations, and checkmark animation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(menuName = "JM/UI/Style/Juicy Toggle")]
public class JuicyToggleStyleSO : ScriptableObject
{
    [Header("Background Colors")]
    public Color onColor      = Color.white;
    public Color offColor     = Color.gray;
    public Color hoveredColor = Color.white;
    public Color pressedColor = Color.white;
    public Color disabledColor = Color.gray;

    [Header("Timing")]
    public float colorDuration  = 0.1f;
    public float toggleDuration = 0.25f;
}
