// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialWorldTargetPositioner.cs
// Summary: Positions and sizes a tutorial overlay RectTransform to align with a world-space Transform's current on-screen position, using the target's lossyScale as its world-space width/height. Call SyncToWorldTarget() once (e.g. when a tutorial step begins) rather than every frame, since tutorial time is frozen.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class TutorialWorldTargetPositioner : MonoBehaviour
{
    [SerializeField] protected RectTransform targetRect;
    [SerializeField] protected RectTransform parentContainer;
    [SerializeField] protected Transform worldTarget;
    [SerializeField] protected Camera worldCamera;
    [SerializeField] protected Canvas canvas;

    public void SyncToWorldTarget()
    {
        Camera canvasCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

        if (TryWorldToLocalPoint(worldTarget.position, canvasCamera, out Vector2 localPoint))
        {
            targetRect.anchoredPosition = localPoint;
        }

        SyncSizeToLossyScale(canvasCamera);
    }

    private void SyncSizeToLossyScale(Camera canvasCamera)
    {
        Vector3 center = worldTarget.position;
        Vector3 halfExtents = worldTarget.lossyScale * 0.5f;

        if (!TryWorldToLocalPoint(center - new Vector3(halfExtents.x, 0f, 0f), canvasCamera, out Vector2 leftLocal))
        {
            return;
        }

        if (!TryWorldToLocalPoint(center + new Vector3(halfExtents.x, 0f, 0f), canvasCamera, out Vector2 rightLocal))
        {
            return;
        }

        if (!TryWorldToLocalPoint(center - new Vector3(0f, halfExtents.y, 0f), canvasCamera, out Vector2 bottomLocal))
        {
            return;
        }

        if (!TryWorldToLocalPoint(center + new Vector3(0f, halfExtents.y, 0f), canvasCamera, out Vector2 topLocal))
        {
            return;
        }

        targetRect.sizeDelta = new Vector2(
            Mathf.Abs(rightLocal.x - leftLocal.x),
            Mathf.Abs(topLocal.y - bottomLocal.y)
        );
    }

    private bool TryWorldToLocalPoint(Vector3 worldPoint, Camera canvasCamera, out Vector2 localPoint)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldPoint);
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentContainer, screenPoint, canvasCamera, out localPoint);
    }
}
