// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RectTransformRotator.cs
// Summary: On enable, fades opacity from 0 to its initial value, waits, then rotates
//          by a given degree — all driven by DOTween with InOutBack easing on rotation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class RectTransformRotator : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float waitDuration = 2f;
    [SerializeField] private float rotateDuration = 1f;
    [SerializeField] private float rotateDegree = 360f;
    [SerializeField] private float targetAlpha = 1f;
    [SerializeField] private float opacityPulseDuration = 1f;

    private CanvasGroup _canvasGroup;
    private Quaternion _initialRotation;
    private Sequence _sequence;
    private Tween _rotateTween;
    private Tween _opacityTween;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _initialRotation = transform.localRotation;
    }

    private void OnEnable()
    {
        _canvasGroup.alpha = 0f;
        transform.localRotation = _initialRotation;

        KillTweens();
        _sequence = DOTween.Sequence()
            .Append(_canvasGroup.DOFade(targetAlpha, fadeDuration))
            .AppendInterval(waitDuration)
            .OnComplete(StartLooping)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        KillTweens();
    }

    private void StartLooping()
    {
        _rotateTween = DOTween.Sequence()
            .Append(transform.DOLocalRotate(new Vector3(0f, 0f, rotateDegree), rotateDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutBack))
            .AppendInterval(waitDuration)
            .SetLoops(-1)
            .SetLink(gameObject);

        _opacityTween = _canvasGroup
            .DOFade(0.5f, opacityPulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject);
    }

    private void KillTweens()
    {
        _sequence?.Kill();
        _rotateTween?.Kill();
        _opacityTween?.Kill();
    }
}
