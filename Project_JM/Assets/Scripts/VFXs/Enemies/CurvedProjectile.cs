// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CurvedProjectile.cs
// Summary: Pure-visual projectile that travels along a quadratic Bezier arc, tracking a live target.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class CurvedProjectile : MonoBehaviour
{
    [SerializeField] private float arcHeight = 1.5f;
    [SerializeField] private bool rotateTowardMotion = true;

    public void Initialize(Transform target, Vector3 offset, float duration)
    {
        StartCoroutine(TravelRoutine(transform.position, target, offset, duration));
    }

    private IEnumerator TravelRoutine(Vector3 start, Transform target, Vector3 offset, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (target == null)
            {
                Destroy(gameObject);
                yield break;
            }

            elapsed += GlobalTimeManager.DeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 end = target.position + offset;
            Vector3 control = (start + end) * 0.5f + Vector3.up * arcHeight;

            Vector3 prevPos = transform.position;
            transform.position = QuadraticBezier(start, control, end, t);

            if (rotateTowardMotion)
            {
                Vector3 dir = transform.position - prevPos;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    private static Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;
        return u * u * p0 + 2f * u * t * p1 + t * t * p2;
    }
}
