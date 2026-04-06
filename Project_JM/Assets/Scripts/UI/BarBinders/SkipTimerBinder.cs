// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/16/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SkipTimerBinder.cs
// Summary: A script to bind skip timer to the UI.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class SkipTimerBinder : MonoBehaviour
{
    [SerializeField] protected Slider slider;
    [SerializeField] protected TransitionManager transitionManager;

    [SerializeField] protected FadeGradually fadeScript;
    [SerializeField] protected float fadeThreshold = 0.4f;

    protected void OnEnable()
    {
        transitionManager.OnSkipTimerChanged += UpdateSkipTimer;
    }

    protected void OnDisable()
    {
        transitionManager.OnSkipTimerChanged -= UpdateSkipTimer;
    }

    protected void UpdateSkipTimer(float current, float max)
    {
        slider.maxValue = max;
        slider.value = current;

        if (fadeScript != null && max > 0f)
        {
            float normalized = current / max;
            if (normalized >= fadeThreshold)
            {
                fadeScript.IncreaseAlphaMultiplier();
            }
        }
    }
}
