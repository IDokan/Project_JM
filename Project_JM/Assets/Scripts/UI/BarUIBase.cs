// SPDX-License-Identifier: MIT
// Copyright (c) 01/08/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BarUIBase.cs
// Summary: An abstract class to be parent for all types of Bar UI.

using UnityEngine;

public abstract class BarUIBase : MonoBehaviour
{
    public void UpdateValue(float current, float max, bool displayMaxValue = true)
        => OnUpdateValue(current, max, displayMaxValue);
    public void UpdateValue(float current, float max, string givenText)
        => OnUpdateValue(current, max, givenText);
    

    protected abstract void OnUpdateValue(float current, float max, bool displayMaxValue);
    protected abstract void OnUpdateValue(float current, float max, string givenText);
}