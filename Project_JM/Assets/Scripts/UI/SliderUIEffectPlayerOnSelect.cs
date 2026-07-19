// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SliderUIEffectPlayerOnSelect.cs
// Summary: Plays UI effects and SFX for sliders; standalone to avoid IPointerClickHandler conflicts from UIEffectPlayerOnSelect.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderUIEffectPlayerOnSelect : MonoBehaviour, ISelectHandler, IDeselectHandler, ICancelHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] protected GameObject selectedEffect;
    [SerializeField] protected Animator animator;
    [SerializeField] private AudioCueSO pressSFX;
    [SerializeField] private AudioCueSO cancelSFX;

    [Header("Style")]
    [SerializeField] private SliderStyleSO style;
    [SerializeField] private Transform handleTransform;

    private Slider _slider;
    private Vector3 _handleInitialScale;
    private bool _pointerDragging;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        if (handleTransform != null)
        {
            _handleInitialScale = handleTransform.localScale;
        }
    }

    private void OnEnable()
    {
        if (_slider != null)
        {
            _slider.onValueChanged.AddListener(OnValueChanged);
        }
    }

    private void OnDisable()
    {
        if (_slider != null)
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

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
        if (animator != null)
        {
            animator.SetBool("Selected", false);
        }
        if (selectedEffect != null)
        {
            selectedEffect.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDragging = true;
        AudioManager.Instance.PlayUISFX(pressSFX);
        if (style != null && handleTransform != null)
        {
            handleTransform.DOKill();
            handleTransform.localScale = _handleInitialScale;
            handleTransform.DOPunchScale(Vector3.one * style.punchScale, style.punchDuration)
                .SetUpdate(true)
                .SetLink(gameObject);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pointerDragging = false;
    }

    public void OnCancel(BaseEventData eventData)
    {
        AudioManager.Instance.PlayUISFX(cancelSFX);
        ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, eventData, ExecuteEvents.cancelHandler);
    }

    private void OnValueChanged(float value)
    {
        if (!_pointerDragging)
        {
            AudioManager.Instance.PlayUISFX(pressSFX);
        }

        if (style == null || handleTransform == null)
        {
            return;
        }

        handleTransform.DOKill();
        handleTransform.localScale = _handleInitialScale;
        handleTransform.DOPunchScale(Vector3.one * style.punchScale, style.punchDuration)
            .SetUpdate(true)
            .SetLink(gameObject);
    }
}
