// SPDX-License-Identifier: MIT
// Copyright (c) 03/02/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CameraShake.cs
// Summary: A script to shake camera.

using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    protected Vector3 _originLocalPos;
    protected Coroutine _routine;

    protected void Awake()
    {
        _originLocalPos = transform.localPosition; 
    }

    public void Shake(float duration = 0.2f, float magnitude = 0.15f)
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    protected IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float t = 0f;

        _originLocalPos = transform.localPosition;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;

            Vector2 rand = Random.insideUnitCircle * magnitude;
            transform.localPosition = _originLocalPos + new Vector3(rand.x, rand.y, 0f);

            yield return null;
        }

        transform.localPosition = _originLocalPos;
        _routine = null;
    }
}
