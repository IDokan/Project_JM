// SPDX-License-Identifier: MIT
// Copyright (c) 03/27/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Menu.cs
// Summary: A script for parent and abstract class of menu script.

using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Default close interfaces")]
    [SerializeField] protected Button title;
    [SerializeField] protected Button backgroundCatcher;


    protected virtual void OnEnable()
    {
        if (title != null)
        {
            title.onClick.AddListener(Hide);
        }
        if (backgroundCatcher != null)
        {
            backgroundCatcher.onClick.AddListener(Hide);
        }
    }

    protected virtual void OnDisable()
    {
        if (title != null)
        {
            title.onClick.RemoveListener(Hide);
        }
        if (backgroundCatcher != null)
        {
            backgroundCatcher.onClick.RemoveListener(Hide);
        }
    }

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
}
