// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MainMenuController.cs
// Summary: Player controller for the main menu scene; handles UI navigation and cancel/back.
//          Click VFX and transition skip-hold are inherited from BasePlayerController.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuController : BasePlayerController
{
    protected override void OnCancelPerformed(InputAction.CallbackContext _)
    {
        // TODO: Navigate back within the menu or quit the application.
        Application.Quit();
    }
}
