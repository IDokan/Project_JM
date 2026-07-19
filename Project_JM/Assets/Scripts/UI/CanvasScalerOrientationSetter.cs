// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CanvasScalerOrientationSetter.cs
// Summary: Applies the correct Canvas Scaler reference resolution and match value for the current screen orientation at startup.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerOrientationSetter : MonoBehaviour
{
    [SerializeField] protected CanvasScaler[] canvasScalers;
    [SerializeField] protected Vector2 landscapeReferenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] protected Vector2 portraitReferenceResolution = new Vector2(1080f, 1920f);
    [SerializeField] protected float landscapeMatchWidthOrHeight = 0f;
    [SerializeField] protected float portraitMatchWidthOrHeight = 0f;

    private void Awake()
    {
        if (canvasScalers == null)
        {
            return;
        }

        bool isPortrait = Screen.height > Screen.width;
        Vector2 referenceResolution = isPortrait ? portraitReferenceResolution : landscapeReferenceResolution;
        float matchWidthOrHeight = isPortrait ? portraitMatchWidthOrHeight : landscapeMatchWidthOrHeight;

        foreach (CanvasScaler canvasScaler in canvasScalers)
        {
            if (canvasScaler == null)
            {
                continue;
            }

            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
        }
    }
}
