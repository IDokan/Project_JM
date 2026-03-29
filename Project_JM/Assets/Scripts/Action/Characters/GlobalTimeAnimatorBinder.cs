// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlobalTimeAnimatorBinder.cs
// Summary: A binder that modify speed of animator by GlobalTimeManager's OnScaleChanged.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[RequireComponent(typeof(Animator))]
public class GlobalTimeAnimatorBinder: MonoBehaviour
{
    [SerializeField] private float baseSpeed = 1f;
    private Animator _anim;

    protected void Awake() => _anim = GetComponent<Animator>();

    protected void OnEnable()
    {
        GlobalTimeManager.OnScaleChanged += Apply;
    }

    protected void OnDisable()
    {
        GlobalTimeManager.OnScaleChanged -= Apply;
    }
    

    protected void Apply(float scale)
    {
        if (_anim)
        {
            _anim.speed = baseSpeed * scale;
        }
    }
}
