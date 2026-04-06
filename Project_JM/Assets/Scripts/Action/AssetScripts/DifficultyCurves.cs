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
    public AnimationCurve HPMultiplierCurve;
    public AnimationCurve DamageMultiplierCurve;

    public StatusMultiplier GetDifficultyMultiplier(int stage)
    {
        StatusMultiplier result;
        result.HPMultiplier = HPMultiplierCurve.Evaluate(stage) *
            DamageMultiplierCurve.Evaluate(stage);
        return result;
    }
}
