// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SliderStyleSO.cs
// Summary: Tuning data for SliderUIEffectPlayerOnSelect — handle punch animation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(menuName = "JM/UI/Style/Slider")]
public class SliderStyleSO : ScriptableObject
{
    [Header("Punch Animation")]
    public float punchScale    = 0.2f;
    public float punchDuration = 0.15f;
}
