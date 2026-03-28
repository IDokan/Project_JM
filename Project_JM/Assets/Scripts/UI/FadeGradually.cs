// SPDX-License-Identifier: MIT
// Copyright (c) 03/16/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FadeGradually.cs
// Summary: A script to fade over time.

using UnityEngine;
using UnityEngine.UI;

public class FadeGradually : MonoBehaviour
{
    [SerializeField] protected float increaseAlphaScaler = 0.1f;
    [SerializeField] protected float decreaseAlphaScaler = 1f;

    protected float alphaMultiplier = 0f;

    protected Image[] images;
    protected float[] maxAlphas;

    protected void Awake()
    {
        images = GetComponentsInChildren<Image>(true);

        maxAlphas = new float[images.Length];

        for (int i = 0; i < images.Length; ++i)
        {
            maxAlphas[i] = images[i].color.a;
        }

        SetAlpha(0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (alphaMultiplier <= 0f)
        {
            SetAlpha(0f);
            alphaMultiplier = 0f;

            return;
        }


        SetAlpha(alphaMultiplier);

        alphaMultiplier -= decreaseAlphaScaler * Time.unscaledDeltaTime;
        alphaMultiplier = Mathf.Max(0f, alphaMultiplier);
    }

    protected void SetAlpha(float alpha)
    {
        for (int i = 0; i < images.Length; ++i)
        {
            Color c = images[i].color;
            c.a = maxAlphas[i] * alpha;
            images[i].color = c;
        }
    }

    public void IncreaseAlphaMultiplier()
    {
        alphaMultiplier = Mathf.Clamp01(increaseAlphaScaler + increaseAlphaScaler);
    }
}
