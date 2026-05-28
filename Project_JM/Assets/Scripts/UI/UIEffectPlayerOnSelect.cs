// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIEffectPlayerOnSelect.cs
// Summary: A script to play effect of button selected.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;

public class UIEffectPlayerOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler, ICancelHandler, IPointerClickHandler, ISubmitHandler
{
    [SerializeField] protected GameObject selectedEffect;
    [SerializeField] protected Animator animator;
    [SerializeField] protected AudioCueSO pressSFX;
    [SerializeField] private AudioCueSO cancelSFX;

    public void OnSelect(BaseEventData eventData)
    {
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(true);
        }
        if (animator != null)
        {
            animator.SetBool("Selected", true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        AudioManager.Instance.PlayUISFX(pressSFX);
    }

    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.Instance.PlayUISFX(pressSFX);
    }

    public void OnCancel(BaseEventData eventData)
    {
        AudioManager.Instance.PlayUISFX(cancelSFX);
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.cancelHandler);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (animator != null)
        {
            animator.SetBool("Selected", false);
        }
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(false);
        }
    }
}
