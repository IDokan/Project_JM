// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EasingBarUI.cs
// Summary: A UI type of bar that easing slowly.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EasingBarUI : BarUIBase
{
    [SerializeField] protected TextMeshProUGUI text;

    [SerializeField] protected Slider healthSlider;
    [SerializeField] protected Slider easingHealthSlider;
    [SerializeField] protected float lerpSpeed = -12f;

    [SerializeField] protected UIHideShow uiHideShow = null;

    protected void Awake()
    {
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        float target = healthSlider.value;
        float v = easingHealthSlider.value;

        if (Mathf.Abs(v - target) > 0.001f)
        {
            float t = 1f - Mathf.Exp(lerpSpeed * GlobalTimeManager.DeltaTime);
            easingHealthSlider.value = Mathf.Lerp(v, target, t);
        }
    }

    protected override void OnUpdateValue(float current, float max, bool displayMaxValue = true)
    {
        healthSlider.maxValue = max;
        easingHealthSlider.maxValue = max;

        healthSlider.value = current;

        if (text != null)
        {
            if (displayMaxValue)
            {
                text.text = $"{Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
            }
            else
            {
                if (current <= 0f)
                {
                    text.CrossFadeAlpha(0f, 0.25f, false);
                }
                else
                {
                    text.CrossFadeAlpha(1f, 0.1f, false);
                    text.text = $"{Mathf.RoundToInt(current)}";

                }
            }
        }

        if (uiHideShow != null)
        {
            if (current <= 0)
            {
                uiHideShow.HideObjects();
            }
            else
            {
                uiHideShow.ShowObjects();
            }
        }
    }

    protected override void OnUpdateValue(float current, float max, string givenText, float alpha)
    {
        healthSlider.maxValue = max;
        easingHealthSlider.maxValue = max;

        healthSlider.value = current;


        if (uiHideShow != null)
        {
            if (current <= 0)
            {
                uiHideShow.HideObjects();
            }
            else
            {
                uiHideShow.ShowObjects();
            }
        }

        if (text != null)
        {
            text.alpha = alpha;
            text.text = givenText;
        }
    }
}
