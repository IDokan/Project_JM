// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/25/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MenuOpener.cs
// Summary: A script to focus input context to this object when enabled.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuOpener : MonoBehaviour
{
    private Coroutine _deferredSelectRoutine;

    protected void OnEnable()
    {
        // A parent Menu's pop-in tween starts scale at zero in Awake() before
        // DOTween ticks it forward. Selecting this frame would hand
        // EventSystem a zero-scaled transform to project to screen space,
        // producing the "-nan(ind)" screen position warning for one frame.
        if (HasZeroScale(transform))
        {
            _deferredSelectRoutine = StartCoroutine(SelectOnceScaleIsValid());
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    protected void OnDisable()
    {
        if (_deferredSelectRoutine != null)
        {
            StopCoroutine(_deferredSelectRoutine);
            _deferredSelectRoutine = null;
        }
    }

    private IEnumerator SelectOnceScaleIsValid()
    {
        while (HasZeroScale(transform))
        {
            yield return null;
        }

        EventSystem.current.SetSelectedGameObject(gameObject);
        _deferredSelectRoutine = null;
    }

    private static bool HasZeroScale(Transform target)
    {
        Vector3 scale = target.lossyScale;
        return scale.x == 0f || scale.y == 0f || scale.z == 0f;
    }
}
