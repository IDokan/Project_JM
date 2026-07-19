// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 08/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIOrientationScaler.cs
// Summary: Applies the correct local scale for the current screen orientation at startup, so a UI element can be sized differently for portrait versus landscape.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIOrientationScaler : MonoBehaviour
{
    [SerializeField] protected Vector3 landscapeScale = Vector3.one;
    [SerializeField] protected Vector3 portraitScale = Vector3.one;

    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        bool isPortrait = Screen.height > Screen.width;
        _rectTransform.localScale = isPortrait ? portraitScale : landscapeScale;
    }
}
