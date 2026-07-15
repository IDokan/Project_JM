// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ScreenshotManager.cs
// Summary: Saves a PNG of the current Game/Simulator view when the capture key is pressed, so
//          QA can screenshot Device Simulator output without hunting for a non-existent
//          built-in button. Editor/development-build only.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotManager : MonoBehaviour
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [SerializeField] protected Key captureKey = Key.F12;

    protected void Update()
    {
        if (Keyboard.current != null && Keyboard.current[captureKey].wasPressedThisFrame)
        {
            string fileName = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            ScreenCapture.CaptureScreenshot(fileName);
            Debug.Log($"Screenshot saved: {fileName}");
        }
    }
#endif
}
