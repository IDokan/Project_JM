// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/17/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DifficultyCurves.cs
// Summary: A class to manage curves for difficulty values.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyCurves", menuName = "JM/Curves/DifficultyCurves")]
public class DifficultyCurves : ScriptableObject
{
    // Raises the bar the player must clear per enemy; scales enemy HP only — no benefit to the player
    public AnimationCurve DifficultyMultiplierCurve;
    // Level-up linear formula (y = slope * stage + intercept); scales ally HP, enemy HP, and all damage proportionally — relative balance stays neutral
    [SerializeField] float levelMultiplierSlope = 0.5f;
    [SerializeField] float levelMultiplierIntercept = 1f;

    public StatusMultiplier GetDifficultyMultiplier(int stage)
    {
        StatusMultiplier result;
        result.HPMultiplier = DifficultyMultiplierCurve.Evaluate(stage) *
            GetLevelMultiplier(stage);
        return result;
    }

    public StatusMultiplier GetAllyDifficultyMultiplier(int stage)
    {
        StatusMultiplier result;
        result.HPMultiplier = GetLevelMultiplier(stage);
        return result;
    }

    public float GetLevelMultiplier(int stage)
    {
        return levelMultiplierSlope * stage + levelMultiplierIntercept;
    }
}
