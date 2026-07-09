// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIOrientationAnchor.cs
// Summary: Applies the correct RectTransform anchors for the current screen orientation at startup, so a UI element can be pinned to a different edge in portrait versus landscape.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIOrientationAnchor : MonoBehaviour
{
    [SerializeField] protected Vector2 landscapeAnchorMin;
    [SerializeField] protected Vector2 landscapeAnchorMax;
    [SerializeField] protected Vector2 portraitAnchorMin;
    [SerializeField] protected Vector2 portraitAnchorMax;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        bool isPortrait = Screen.height > Screen.width;
        _rectTransform.anchorMin = isPortrait ? portraitAnchorMin : landscapeAnchorMin;
        _rectTransform.anchorMax = isPortrait ? portraitAnchorMax : landscapeAnchorMax;
    }
}
