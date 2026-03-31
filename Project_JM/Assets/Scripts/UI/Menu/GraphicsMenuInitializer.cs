// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/30/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GraphicsMenuInitializer.cs
// Summary: Pre-initializes GraphicsMenu at scene load to avoid first-open lag.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class GraphicsMenuInitializer : MonoBehaviour
{
    [SerializeField] protected GraphicsMenu graphicsMenu;

    protected void Awake()
    {
        graphicsMenu.PreInitialize();
    }
}
