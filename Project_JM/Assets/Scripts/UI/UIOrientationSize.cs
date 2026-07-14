// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIOrientationSize.cs
// Summary: Applies the correct RectTransform sizeDelta for the current screen orientation at startup, so a UI element can be sized differently for portrait versus landscape without touching localScale.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIOrientationSize : MonoBehaviour
{
    [SerializeField] protected Vector2 landscapeSizeDelta;
    [SerializeField] protected Vector2 portraitSizeDelta;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        bool isPortrait = Screen.height > Screen.width;
        _rectTransform.sizeDelta = isPortrait ? portraitSizeDelta : landscapeSizeDelta;
    }
}
