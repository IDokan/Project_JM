// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JuicyToggle.cs
// Summary: Toggle subclass that animates checkmark scale and background color via DOTween.
//          Leave Toggle.graphic unassigned — this class manages the checkmark directly
//          to prevent Unity's built-in alpha-flip from fighting the scale animation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class JuicyToggle : Toggle
{
    [Header("Juicy")]
    [SerializeField] private RectTransform checkmark;
    [SerializeField] private Image background;
    [SerializeField] private JuicyToggleStyleSO style;

    protected override void Start()
    {
        base.Start();
        SetInitialVisuals();
        onValueChanged.AddListener(OnValueChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        onValueChanged.RemoveListener(OnValueChanged);
    }

    private void SetInitialVisuals()
    {
        if (checkmark != null)
            checkmark.localScale = isOn ? Vector3.one : Vector3.zero;

        if (background != null && style != null)
            background.color = isOn ? style.onColor : style.offColor;
    }

    private void OnValueChanged(bool value)
    {
        if (checkmark == null || style == null)
            return;

        checkmark.DOKill();

        if (background != null)
        {
            background.DOKill();
            background.DOColor(value ? style.onColor : style.offColor, style.colorDuration).SetUpdate(true);
        }

        if (value)
        {
            checkmark.localScale = Vector3.zero;
            checkmark.DOScale(Vector3.one, style.toggleDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
        else
        {
            checkmark.DOScale(Vector3.zero, style.toggleDuration)
                .SetEase(Ease.InBack)
                .SetUpdate(true);
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (background == null || style == null)
            return;

        Color target = state switch
        {
            SelectionState.Highlighted => style.hoveredColor,
            SelectionState.Pressed     => style.pressedColor,
            SelectionState.Disabled    => style.disabledColor,
            _                          => isOn ? style.onColor : style.offColor,
        };

        background.DOKill();

        if (instant)
            background.color = target;
        else
            background.DOColor(target, style.colorDuration).SetUpdate(true);
    }
}
