// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/19/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ClickVFXSpawner.cs
// Summary: A script to spawn click VFX.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using UnityEngine;
using GemEnums;

public class ClickVFXSpawner : MonoBehaviour
{
    [SerializeField] protected Canvas canvas;
    [SerializeField] protected RectTransform touchLayer;

    [SerializeField] protected GameObject clickVFX;

    protected GemColor _lastGemColor = GemColor.None;

    private Camera UICamera
    {
        get
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return null;
            }

            return canvas.worldCamera;
        }
    }

    protected void Awake()
    {
        if (canvas == null)
        {
            Debug.LogWarning("Spawn location is null", this);
        }
    }

    public GameObject SpawnClickVFX(Vector2 screenPosition)
    {
        return SpawnClickVFX(screenPosition, GemColorUtility.GetNextGemColor(_lastGemColor));
    }

    public GameObject SpawnClickVFX(Vector2 screenPosition, GemColor gemColor)
    {
        GameObject result = Instantiate(clickVFX, touchLayer);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            touchLayer,
            screenPosition,
            UICamera,
            out Vector2 localPoint
            ))
        {
            RectTransform resultRectTransform = result.GetComponent<RectTransform>();
            if (resultRectTransform != null)
            {
                resultRectTransform.anchoredPosition = localPoint;
            }
            else
            {
                result.transform.localPosition = localPoint;
            }
        }

        SetColorToParticles helperScript;
        if (result.TryGetComponent<SetColorToParticles>(out helperScript))
        {
            helperScript.SetColor(gemColor);
            _lastGemColor = gemColor;
        }

        return result;
    }
}
