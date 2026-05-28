// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 04/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PlayerInputExtensions.cs
// Summary: Extension methods for PlayerInput to switch action maps.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine.InputSystem;

public static class PlayerInputExtensions
{
    public static void SwitchToUIMap(this PlayerInput playerInput)
    {
        playerInput.SwitchCurrentActionMap("UI");
        playerInput.actions.FindActionMap("Gameplay").FindAction("press").Enable();
    }

    public static void SwitchToGameplayMap(this PlayerInput playerInput)
    {
        playerInput.SwitchCurrentActionMap("Gameplay");
        playerInput.actions.FindActionMap("UI").FindAction("Click").Enable();
        playerInput.actions.FindActionMap("UI").FindAction("Point").Enable();
    }
}
