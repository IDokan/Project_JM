// SPDX-License-Identifier: MIT
// Copyright (c) 11/13/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BarUI.cs
// Summary: A UI type of bar.

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarUI : BarUIBase
{
    [SerializeField] protected Slider slider;
    [SerializeField] protected TextMeshProUGUI text;

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
        
    }

    protected override void OnUpdateValue(float current, float max, bool displayMaxValue)
    {
        slider.maxValue = max;

        slider.value = current;

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
    }

    protected override void OnUpdateValue(float current, float max, string givenText)
    {
        slider.maxValue = max;

        slider.value = current;

        if (text != null)
        {
            text.text = givenText;
        }
    }
}
