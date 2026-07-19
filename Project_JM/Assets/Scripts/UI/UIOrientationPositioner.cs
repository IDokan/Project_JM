// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIOrientationPositioner.cs
// Summary: Applies the correct anchored position for the current screen orientation at startup, so a UI element can sit in different places for portrait versus landscape.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIOrientationPositioner : MonoBehaviour
{
    [SerializeField] protected Vector2 landscapeAnchoredPosition;
    [SerializeField] protected Vector2 portraitAnchoredPosition;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        bool isPortrait = Screen.height > Screen.width;
        _rectTransform.anchoredPosition = isPortrait ? portraitAnchoredPosition : landscapeAnchoredPosition;
    }
}
