// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MenuOpener.cs
// Summary: A script to focus input context to this object when enabled.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;

public class MenuOpener : MonoBehaviour
{
    protected void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
