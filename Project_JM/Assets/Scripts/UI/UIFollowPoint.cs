// SPDX-License-Identifier: MIT
// Copyright (c) 11/05/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIFollowPoint.cs
// Summary: A script to move UI to follow point of PlayerController.

using UnityEngine;

public class UIFollowPoint : MonoBehaviour
{
    [SerializeField] protected RectTransform uiTransform;
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected Camera uiCamera;
    [SerializeField] protected PlayerController playerController;

    [SerializeField] protected Vector2 offset;

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
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Vector2 screenPoint = playerController.GetCurrentFollowPoint();
        MoveToScreenPoint(screenPoint + offset);
    }

    protected void MoveToScreenPoint(Vector2 screenPoint)
    {
        if (uiTransform == null || canvas == null)
        {
            return;
        }

        RectTransform canvasTransform = canvas.transform as RectTransform;

        if (canvasTransform == null)
        {
            return;
        }

        Vector2 localPoint;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasTransform,
            screenPoint,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCamera,
            out localPoint);

        if (success)
        {
            uiTransform.anchoredPosition = localPoint;

            Vector3[] corners = new Vector3[4];
            uiTransform.GetWorldCorners(corners);

            // corners:
            // 0 = bottom-left
            // 1 = top-left
            // 2 = top-right
            // 3 = bottom-right     (clock-wise)

            float offsetX = 0f;
            float offsetY = 0f;

            if (corners[0].x < 0f)
            {
                offsetX = -corners[0].x;
            }
            else if (corners[2].x > Screen.width)
            {
                offsetX = Screen.width - corners[2].x;
            }


            if (corners[0].y < 0f)
            {
                offsetY = -corners[0].y;
            }
            else if(corners[2].y > Screen.height) 
            {
                offsetY = Screen.height - corners[2].y;
            }

            uiTransform.position += new Vector3(offsetX, offsetY, 0f);
        }
    }
}
