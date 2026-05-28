// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ScaleByCurve.cs
// Summary: Tweens the transform scale using a serialized animation curve.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

public class ScaleByCurve : MonoBehaviour
{
    [SerializeField] private AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField] private float duration = 1f;
    [SerializeField] private bool playOnStart = false;

    private Vector3 _initialScale;

    protected void Awake()
    {
        _initialScale = transform.localScale;
    }

    protected void Start()
    {
        if (playOnStart)
            Play();
    }

    public void Play()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(_initialScale, duration).SetEase(curve).SetLink(gameObject);
    }
}
