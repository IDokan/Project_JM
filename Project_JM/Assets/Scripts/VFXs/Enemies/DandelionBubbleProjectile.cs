// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DandelionBubbleProjectile.cs
// Summary: Moves a dandelion bubble in two phases: fly (straight-line easeOutBack + decaying
//          perpendicular oscillation), then glide (oscillation centered on target, amplitude
//          decaying to zero), then instantiates an explosion prefab and destroys itself.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class DandelionBubbleProjectile : MonoBehaviour
{
    [SerializeField] private float flyGlideCount = 0.2f;
    [SerializeField] private float glideCount = 0.03f;
    [SerializeField] private float glideAmplitude = 1.0f;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioCueSO popSFX;

    public void Initialize(Transform target, Vector3 offset, float flyDuration, float glideDuration)
    {
        Vector3 targetPosition = target.position + offset;
        StartCoroutine(TravelRoutine(transform.position, targetPosition, flyDuration, glideDuration));
    }

    private IEnumerator TravelRoutine(Vector3 start, Vector3 end, float flyDuration, float glideDuration)
    {
        Vector3 travel = end - start;
        Vector3 perp = travel.sqrMagnitude > 0.0001f
            ? new Vector3(-travel.normalized.y, travel.normalized.x, 0f)
            : Vector3.up;

        float elapsed = 0f;
        while (elapsed < flyDuration)
        {
            elapsed += GlobalTimeManager.DeltaTime;
            float t = Mathf.Clamp01(elapsed / flyDuration);

            Vector3 basePos = Vector3.Lerp(start, end, EaseOutBack(t));
            float sideOffset = Mathf.Sin(t * flyGlideCount * Mathf.PI * 2f) * glideAmplitude * (1f - t);
            transform.position = basePos + perp * sideOffset;

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < glideDuration)
        {
            elapsed += GlobalTimeManager.DeltaTime;
            float t = Mathf.Clamp01(elapsed / glideDuration);

            float sideOffset = Mathf.Sin(t * glideCount * Mathf.PI * 2f) * glideAmplitude * (1f - t);
            transform.position = end + perp * sideOffset;

            yield return null;
        }

        transform.position = end;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, end, Quaternion.identity);
        }

        AudioManager.Instance.PlayEnemyActionSFX(popSFX);

        Destroy(gameObject);
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
