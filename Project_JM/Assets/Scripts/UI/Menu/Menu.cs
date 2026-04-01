// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/27/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Menu.cs
// Summary: A script for parent and abstract class of menu script.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [Header("Default close interfaces")]
    [SerializeField] protected Button title;
    [SerializeField] protected Button backgroundCatcher;

    [Header("Initial state")]
    [SerializeField] private bool showOnAwake = false;

    private CanvasGroup _canvasGroup;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        if (showOnAwake)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

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
        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public virtual void Hide()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }
}
