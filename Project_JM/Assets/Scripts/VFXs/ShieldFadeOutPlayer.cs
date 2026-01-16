// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShieldFadeOutPlayer.cs
// Summary: A script to spawn shield VFX.


using UnityEngine;

public class ShieldFadeOutPlayer : MonoBehaviour
{
    [SerializeField] protected float duration = 1f;
    [SerializeField] protected AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    protected SpriteRenderer _spriteRenderer;
    protected float _t;

    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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

        Color color = _spriteRenderer.color;
        color.a = progress;
        _spriteRenderer.color = color;

        if (_t >= duration)
        {
            Destroy(gameObject);
        }
    }
}
