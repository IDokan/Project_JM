// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/01/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FlashSpriteUsingMaterial.cs
// Summary: A script to flash sprite a bit.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class FlashSpriteUsingMaterial : MonoBehaviour
{
    [SerializeField] protected Material flashMaterial;

    protected Coroutine _routine;

    void Awake()
    {
        if (flashMaterial == null)
        {
            flashMaterial = GetComponentInChildren<SpriteRenderer>()?.material;
        }
    }

    protected void OnEnable()
    {
        if (flashMaterial != null)
        {
            flashMaterial.SetFloat("_FlashAmount", 0);
        }
    }

    protected void OnDisable()
    {
        if (flashMaterial != null)
        {
            flashMaterial.SetFloat("_FlashAmount", 0);
        }
    }


    public void Flash(float duration = 0.06f, float amount = 0.55f)
    {
        Flash(duration, Color.white, amount);
    }

    public void Flash(float duration, Color color, float amount)
    {
        if (flashMaterial == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(FlashRoutine(duration, color, amount));
    }

    protected IEnumerator FlashRoutine(float duration, Color color, float amount)
    {
        flashMaterial.SetFloat("_FlashAmount", amount);
        flashMaterial.SetColor("_FlashColor", Color.white);

        yield return GlobalTimeManager.WaitForGlobalSeconds(duration);


        flashMaterial.SetFloat("_FlashAmount", 0f);

        _routine = null;
    }
}
