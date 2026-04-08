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
    public Selectable GetFirstSelectable()
        => firstSelected != null ? firstSelected : GetComponentInChildren<Selectable>();

    [Header("Style")]
    [SerializeField] private MenuStyleSO style;

    [Header("Initial state")]
    [SerializeField] private bool showOnAwake = false;

    protected CanvasGroup _canvasGroup;
    private Selectable _returnSelected;

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

    public void Show()
    {
        Show(null);
    }

    public virtual void Show(Selectable returnTo)
    {
        _returnSelected = returnTo;

        _canvasGroup.alpha = 1f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        Selectable selected = GetFirstSelectable();
        if (selected != null)
        {
            EventSystem.current.SetSelectedGameObject(selected.gameObject);
        }

        if (style != null)
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, style.showDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(OnShowComplete);
        }
        else
        {
            OnShowComplete();
        }
    }

    private void OnShowComplete()
    {
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
    }

    public virtual void OnCancel(BaseEventData eventData)
    {
        Hide();
    }

    public virtual void Hide()
    {
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;

        EventSystem.current.SetSelectedGameObject(
            _returnSelected != null ? _returnSelected.gameObject : null);

        if (style != null)
        {
            transform.DOKill();
            transform.DOScale(Vector3.zero, style.hideDuration)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() => _canvasGroup.alpha = 0f);
        }
        else
        {
            _canvasGroup.alpha = 0f;
        }
    }
}
