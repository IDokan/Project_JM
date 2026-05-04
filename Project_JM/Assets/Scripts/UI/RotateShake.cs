// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 04/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RotateShake.cs
// Summary: Oscillates the transform Z-rotation CW/CCW to produce a title vibration effect.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;

public class RotateShake : MonoBehaviour
{
    [SerializeField] private float angle = 10f;
    [SerializeField] private float swingDuration = 0.07f;
    [SerializeField] private int loops = 6;
    [SerializeField] private float startDelay = 0.5f;
    [SerializeField] private bool playOnStart = false;

    protected void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    public void Play()
    {
        DOTween.Kill(transform);
        transform.localRotation = Quaternion.identity;

        var seq = DOTween.Sequence().SetDelay(startDelay).SetLink(gameObject);
        for (int i = 0; i < loops; i++)
        {
            seq.Append(transform.DOLocalRotate(new Vector3(0f, 0f, angle), swingDuration)
                .SetEase(Ease.OutQuad));
            seq.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -angle), swingDuration)
                .SetEase(Ease.InOutQuad));
        }
        seq.Append(transform.DOLocalRotate(Vector3.zero, swingDuration).SetEase(Ease.OutQuad));
    }
}
