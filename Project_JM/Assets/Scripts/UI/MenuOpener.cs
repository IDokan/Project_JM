// SPDX-License-Identifier: MIT
// Copyright (c) 03/25/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MenuOpener.cs
// Summary: A script to focus input context to this object when enabled.

using UnityEngine;
using UnityEngine.EventSystems;

public class MenuOpener : MonoBehaviour
{
    protected void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}
