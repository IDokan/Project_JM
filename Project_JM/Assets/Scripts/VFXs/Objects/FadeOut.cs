// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/14/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FadeOut.cs
// Summary: A script to fade object and its children out.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using System.Collections.Generic;
using UnityEngine;

public class FadeOut : MonoBehaviour
{
    protected virtual float DeltaTime => Time.deltaTime;

    [SerializeField] protected float duration = 1f;
    [SerializeField] protected AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    protected SpriteRenderer[] _spriteRenderers;
    protected float[] _startAlphas;
    protected float _t;

    void Awake()
    {
        List<SpriteRenderer> renderers = new List<SpriteRenderer>();
        CollectRenderers(transform, renderers);
        _spriteRenderers = renderers.ToArray();

        _startAlphas = new float[_spriteRenderers.Length];

        for (int i = 0; i < _startAlphas.Length; i++)
        {
            _startAlphas[i] = _spriteRenderers[i] ? _spriteRenderers[i].color.a : 1f;
        }

        UpdateOpacity(0f);
    }

    // Stops descending into a child subtree that has its own FadeOut, so a
    // nested FadeOut owns its renderers independently instead of both this
    // and the nested script writing color.a to the same SpriteRenderer.
    private void CollectRenderers(Transform node, List<SpriteRenderer> renderers)
    {
        if (node != transform && node.GetComponent<FadeOut>() != null)
        {
            return;
        }

        if (node.TryGetComponent<SpriteRenderer>(out var sr))
        {
            renderers.Add(sr);
        }

        foreach (Transform child in node)
        {
            CollectRenderers(child, renderers);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _t = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        _t += DeltaTime;

        UpdateOpacity(_t);
    }


    protected void UpdateOpacity(float t)
    {

        float p01 = duration <= 0 ? 1f : Mathf.Clamp01(t / duration);

        float progress = progressCurve.Evaluate(p01);

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            var spriteRenderer = _spriteRenderers[i];
            if (spriteRenderer == null)
            {
                continue;
            }

            Color color = _spriteRenderers[i].color;
            color.a = _startAlphas[i] * progress;
            spriteRenderer.color = color;
        }

        if (t >= duration)
        {
            Destroy(gameObject);
        }
    }
}
