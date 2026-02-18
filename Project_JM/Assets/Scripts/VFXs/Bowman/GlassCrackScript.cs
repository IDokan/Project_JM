// SPDX-License-Identifier: MIT
// Copyright (c) 01/28/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlassCrackScript.cs
// Summary: A script for main mechs for the glass crack prefab.

using System.Collections;
using UnityEngine;

public class GlassCrackScript : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer spriteRenderer;
    [SerializeField] protected string revealProperty = "Reveal";
    [SerializeField] protected float revealDuration = 0.5f;
    [SerializeField] protected AnimationCurve revealCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField] protected float fadeOutDuration = 0.2f;
    [SerializeField] protected AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    protected MaterialPropertyBlock _materialPropertyBlock;

    protected void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    protected void OnEnable()
    {
        StartCoroutine(Run());
    }

    protected IEnumerator Run()
    {
        float revealT = 0f;
        while (revealT < revealDuration)
        {
            revealT += Time.deltaTime;
            float x = Mathf.Clamp01(revealT / revealDuration);
            SetReveal(revealCurve.Evaluate(x));
            yield return null;
        }

        float fadeOutT = 0f;
        while(fadeOutT < fadeOutDuration)
        {
            fadeOutT += Time.deltaTime;
            float y = Mathf.Clamp01(fadeOutT / fadeOutDuration);
            SetAlpha(fadeOutCurve.Evaluate(y));
            yield return null;
        }

        Destroy(gameObject);
    }

    protected void SetReveal(float v)
    {
        spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(revealProperty, v);
        spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }

    protected void SetAlpha(float a)
    {
        Color c = spriteRenderer.color;
        c.a = a;
        spriteRenderer.color = c;
    }
}
