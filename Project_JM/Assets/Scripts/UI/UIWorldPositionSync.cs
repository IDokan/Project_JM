// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIWorldPositionSync.cs
// Summary: Locks a UI RectTransform onto a world-space Transform's on-screen position, synced once whenever transitionEventChannel raises syncOnPhase — e.g. reward buttons snapping onto the board at RewardTransitionStarts, since they're hidden the rest of the time and don't need per-frame tracking.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class UIWorldPositionSync : MonoBehaviour
{
    [SerializeField] protected RectTransform uiTransform;
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Camera uiCamera;
    [SerializeField] protected Transform worldTarget;
    [SerializeField] protected Camera worldCamera;
    [SerializeField] protected Vector2 screenOffset;

    [Header("Sync Trigger")]
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected TransitionPhase syncOnPhase;

    protected virtual void Reset()
    {
        uiTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    protected virtual void Awake()
    {
        if (uiTransform == null)
        {
            uiTransform = GetComponent<RectTransform>();
        }
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }
        if (worldCamera == null)
        {
            worldCamera = Camera.main;
        }
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == syncOnPhase)
        {
            SyncToWorldTarget();
        }
    }

    protected void SyncToWorldTarget()
    {
        if (worldTarget == null || uiTransform == null || canvas == null)
        {
            return;
        }

        RectTransform canvasTransform = canvas.transform as RectTransform;
        if (canvasTransform == null)
        {
            return;
        }

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(worldCamera, worldTarget.position) + screenOffset;

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera,
            out Vector2 localPoint);

        if (success)
        {
            uiTransform.anchoredPosition = localPoint;
        }
    }
}
