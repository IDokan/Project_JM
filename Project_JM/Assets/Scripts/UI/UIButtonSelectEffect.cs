// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIButtonSelectEffect.cs
// Summary: A script to play effect of button selected.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSelectEffect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] protected GameObject selectedEffect;
    [SerializeField] protected Animator animator;

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

    public void OnDeselect(BaseEventData eventData)
    {
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(false);
        }

        if (animator != null)
        {
            animator.SetBool("Selected", false);
        }
    }
}
