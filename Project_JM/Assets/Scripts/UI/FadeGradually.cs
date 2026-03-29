// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/16/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FadeGradually.cs
// Summary: A script to fade over time.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class FadeGradually : MonoBehaviour
{
    [SerializeField] protected float increaseAlphaScaler = 0.1f;
    [SerializeField] protected float decreaseAlphaScaler = 1f;

    protected float _alphaMultiplier = 0f;

    protected Image[] _images;
    protected float[] _maxAlphas;

    protected void Awake()
    {
        _images = GetComponentsInChildren<Image>(true);

        _maxAlphas = new float[_images.Length];

        for (int i = 0; i < _images.Length; ++i)
        {
            _maxAlphas[i] = _images[i].color.a;
        }

        SetAlpha(0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (_alphaMultiplier <= 0f)
        {
            SetAlpha(0f);
            _alphaMultiplier = 0f;

            return;
        }


        SetAlpha(_alphaMultiplier);

        _alphaMultiplier -= decreaseAlphaScaler * Time.unscaledDeltaTime;
        _alphaMultiplier = Mathf.Max(0f, _alphaMultiplier);
    }

    protected void SetAlpha(float alpha)
    {
        for (int i = 0; i < _images.Length; ++i)
        {
            Color c = _images[i].color;
            c.a = _maxAlphas[i] * alpha;
            _images[i].color = c;
        }
    }

    public void IncreaseAlphaMultiplier()
    {
        _alphaMultiplier = Mathf.Clamp01(increaseAlphaScaler + increaseAlphaScaler);
    }
}
