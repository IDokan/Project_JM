// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/27/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Menu.cs
// Summary: A script for parent and abstract class of menu script.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour, ICancelHandler
{
    [Header("Default close interfaces")]
    [SerializeField] protected Button title;
    [SerializeField] protected Button backgroundCatcher;

    [Header("Gamepad navigation")]
    [SerializeField] private Selectable firstSelected;
    public virtual Selectable GetFirstSelectable()
        => firstSelected != null ? firstSelected : GetComponentInChildren<Selectable>();

    [Header("Style")]
    [SerializeField] private MenuStyleSO style;

    [Header("Initial state")]
    [SerializeField] private bool showOnAwake = false;

    [SerializeField] protected CanvasGroup canvasGroup;
    private Selectable _returnSelected;

    protected virtual void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup)
        {
            if (showOnAwake)
            {
                Show();
            }
            else
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                transform.localScale = Vector3.zero;
            }
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

    public void Show()
    {
        Show(null);
    }

    public virtual void Show(Selectable returnTo)
    {
        _returnSelected = returnTo;

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (style != null)
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, style.showDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(OnShowComplete);
        }
        else
        {
            OnShowComplete();
        }
    }

    protected virtual void OnShowComplete()
    {
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Selected only once the pop-in tween finishes, not at the start of
        // Show() — selecting earlier put the selected object's transform
        // scale at zero for the tween's first frame(s), a singular matrix
        // that made EventSystem's per-frame navigation math divide by zero
        // and spam "Screen position out of view frustum (-nan)" warnings.
        Selectable selected = GetFirstSelectable();
        if (selected != null)
        {
            EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }
    }

    public virtual void OnCancel(BaseEventData eventData)
    {
        Hide();
    }

    public virtual void Hide()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        EventSystem.current.SetSelectedGameObject(
            _returnSelected != null ? _returnSelected.gameObject : null);

        if (style != null)
        {
            transform.DOKill();
            transform.DOScale(Vector3.zero, style.hideDuration)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .SetLink(gameObject)
                .OnComplete(() => canvasGroup.alpha = 0f);
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
}
