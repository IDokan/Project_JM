// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LemniscateOrbit.cs
// Summary: Moves a transform along a lemniscate (infinity-symbol) path after an initial delay.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class LemniscateOrbit : MonoBehaviour
{
    [SerializeField] protected float stayDuration = 1f;
    [SerializeField] protected float scale = 0.5f;
    [SerializeField] protected float speed = 1f;

    private float _startTime;

    protected void OnEnable()
    {
        _startTime = GlobalTimeManager.Time;
    }

    protected void LateUpdate()
    {
        float elapsed = GlobalTimeManager.Time - _startTime;

        if (elapsed < stayDuration)
            return;

        float t = (elapsed - stayDuration) * speed;
        float sinT = Mathf.Sin(t);
        float denom = 1f + sinT * sinT;
        float x = scale * Mathf.Cos(t) / denom;
        float y = scale * sinT * Mathf.Cos(t) / denom;

        transform.localPosition = new Vector3(x, y, 0f);
    }
}
