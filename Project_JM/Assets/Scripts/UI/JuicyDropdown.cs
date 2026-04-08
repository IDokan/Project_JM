// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JuicyDropdown.cs
// Summary: TMP_Dropdown subclass that animates the panel scale on open and
//          drives button state color transitions via DOTween.
//          Panel alpha fade on close is handled by TMP's built-in AlphaFadeList,
//          timed to match animDuration via alphaFadeSpeed.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JuicyDropdown : TMP_Dropdown
{
    [Header("Juicy")]
    [SerializeField] private Image background;
    [SerializeField] private JuicyDropdownStyleSO style;

    public override void OnCancel(BaseEventData eventData)
    {
        if (IsExpanded)
        {
            base.OnCancel(eventData);
            return;
        }

        ExecuteEvents.ExecuteHierarchy<ICancelHandler>(
            transform.parent.gameObject, eventData, ExecuteEvents.cancelHandler);
    }

    protected override GameObject CreateDropdownList(GameObject template)
    {
        // Sync TMP's alpha fade duration to our animation duration so open/close timings match.
        alphaFadeSpeed = style != null ? style.animDuration : alphaFadeSpeed;

        GameObject list = base.CreateDropdownList(template);

        if (style != null)
        {
            RectTransform rect = list.GetComponent<RectTransform>();
            rect.localScale = new Vector3(1f, 0f, 1f);
            rect.DOScaleY(1f, style.animDuration)
                .SetEase(style.showEase)
                .SetUpdate(true);
        }

        return list;
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        if (background == null || style == null)
            return;

        Color target = state switch
        {
            SelectionState.Highlighted => style.hoveredColor,
            SelectionState.Pressed => style.pressedColor,
            SelectionState.Disabled => style.disabledColor,
            _ => style.normalColor,
        };

        background.DOKill();

        if (instant)
            background.color = target;
        else
            background.DOColor(target, style.colorDuration).SetUpdate(true);
    }
}
