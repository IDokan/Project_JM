// SPDX-License-Identifier: MIT
// Copyright (c) 11/17/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DifficultyCurves.cs
// Summary: A class to manage curves for difficulty values.


using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyCurves", menuName = "JM/Curves/DifficultyCurves")]
public class DifficultyCurves : ScriptableObject
{
    public AnimationCurve HPMultiplierCurve;
    public AnimationCurve DamageMultiplierCurve;
}
