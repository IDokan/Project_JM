// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ConfirmationDialog.cs
// Summary: Generic destructive-action confirmation dialog.
//          Call Show(icon, onConfirm) to present it; the callback fires
//          only when the player presses Confirm.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationDialog : Menu
{
    [SerializeField] private Image actionIcon;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;

    protected override void OnEnable()
    {
        base.OnEnable();
        confirmButton.onClick.AddListener(OnConfirmPressed);
        cancelButton.onClick.AddListener(OnCancelPressed);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        confirmButton.onClick.RemoveListener(OnConfirmPressed);
        cancelButton.onClick.RemoveListener(OnCancelPressed);
    }

    public void Show(Sprite icon, Action onConfirm)
    {
        actionIcon.sprite = icon;
        _onConfirm = onConfirm;
        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        _onConfirm = null;
    }

    private void OnConfirmPressed()
    {
        Action callback = _onConfirm;
        Hide();
        callback?.Invoke();
    }

    private void OnCancelPressed()
    {
        Hide();
    }
}
