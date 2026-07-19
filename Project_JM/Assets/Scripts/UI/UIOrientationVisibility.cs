// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIOrientationVisibility.cs
// Summary: Hides and disables this CanvasGroup at startup when the screen is in portrait orientation, so a UI element can be excluded from mobile portrait layouts.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UIOrientationVisibility : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        bool isPortrait = Screen.height > Screen.width;
        if (isPortrait)
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
        else
        {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
    }
}
