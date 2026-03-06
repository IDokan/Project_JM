// SPDX-License-Identifier: MIT
// Copyright (c) 03/04/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemShake.cs
// Summary: A script to shake gems.

using System.Collections;
using UnityEngine;

public class GemShake : MonoBehaviour
{
    [SerializeField] protected float shakeMagnitude = 0.4f;
    [SerializeField] protected float frequency = 25f; // Shakes per second
    [SerializeField] protected float lerpSpeed = 25f;

    public bool IsShaking => _routine != null;

    protected Vector3 _originLocalPosition = new Vector3(0f, 0f, 0f);
    protected Coroutine _routine = null;

    protected Vector3 _targetPositionOffset = new Vector3(0f, 0f, 0f);

    public void Awake()
    {
    }

    public void StartShake()
    {
        if (_routine == null)
        {
            _routine = StartCoroutine(ShakeRoutine());
        }
    }

    public void StopShake()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            transform.localPosition = _originLocalPosition;
        }
        _routine = null;
    }

    protected IEnumerator ShakeRoutine()
    {
        _originLocalPosition = transform.localPosition;

        float interval = Mathf.Max(0.001f, 1f / Mathf.Max(0.001f, frequency));

        while (true)
        {
            // Pick new random targets at a fixed inverval
            _targetPositionOffset = Random.insideUnitCircle * shakeMagnitude;

            float t = 0f;
            while (t < interval)
            {
                float dt = Time.unscaledDeltaTime;
                t += dt;

                Vector3 currentOffset = transform.localPosition - _originLocalPosition;
                currentOffset = Vector3.Lerp(currentOffset, _targetPositionOffset, 1f - Mathf.Exp(-lerpSpeed * dt));
                transform.localPosition = _originLocalPosition + currentOffset;

                yield return null;
            }
        }
    }
}
