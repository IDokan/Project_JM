// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FadeOut.cs
// Summary: A script to spawn shield VFX.


using UnityEngine;

public class FadeOut : MonoBehaviour
{
    [SerializeField] protected float duration = 1f;
    [SerializeField] protected AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    protected SpriteRenderer[] _spriteRenderers;
    protected float[] _startAlphas;
    protected float _t;

    void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        _startAlphas = new float[_spriteRenderers.Length];

        for (int i = 0; i < _startAlphas.Length; i++)
        {
            _startAlphas[i] = _spriteRenderers[i] ? _spriteRenderers[i].color.a : 1f;
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
        _t += Time.deltaTime;

        float p01 = duration <= 0 ? 1f : Mathf.Clamp01(_t / duration);

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

        if (_t >= duration)
        {
            Destroy(gameObject);
        }
    }
}
