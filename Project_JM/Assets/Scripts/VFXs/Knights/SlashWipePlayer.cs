// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlashWipePlayer.cs
// Summary: A script to modify parameters of material to display slash wipe.


using UnityEngine;

public class SlashWipePlayer : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] protected float duration = 0.12f;
    [SerializeField] protected AnimationCurve progressCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] protected float fadeOutTail = 0.04f;

    [Header("Look")]
    [SerializeField, Range(0.001f, 0.2f)] protected float softness = 0.04f;
    [SerializeField, Range(0f, 1f)] protected float invert = 0f;
    [SerializeField] protected Color tint = Color.white;

    protected static readonly int ProgressID = Shader.PropertyToID("_Progress");
    protected static readonly int SoftnessID = Shader.PropertyToID("_Softness");
    protected static readonly int InvertID = Shader.PropertyToID("_Invert");
    protected static readonly int AlphaID = Shader.PropertyToID("_Alpha");
    protected static readonly int TintID = Shader.PropertyToID("_Tint");

    protected SpriteRenderer _spriteRenderer;
    protected MaterialPropertyBlock _materialPropertyBlock;
    protected float _t;


    void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();

        Apply(0f, 1f);
    }

    void OnEnable()
    {
        _t = 0f;
        Apply(0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        _t += Time.deltaTime;

        float p01 = duration <= 0f ? 1f : Mathf.Clamp01(_t / duration);
        float prog = progressCurve.Evaluate(p01);

        // After it fully reveals, fade alpha out a bit
        float alpha = 1f;
        if (_t > duration && fadeOutTail > 0f)
        {
            float tail01 = Mathf.Clamp01((_t - duration) / fadeOutTail);
            alpha = 1f - tail01;
        }

        Apply(prog, alpha);

        if (_t >= duration + fadeOutTail)
        {
            Destroy(gameObject);
        }
    }

    protected void Apply(float progress, float alpha)
    {
        if (!_spriteRenderer)
        {
            return;
        }

        _spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(ProgressID, progress);
        _materialPropertyBlock.SetFloat(SoftnessID, softness);
        _materialPropertyBlock.SetFloat(InvertID, invert);
        _materialPropertyBlock.SetFloat(AlphaID, alpha);
        _materialPropertyBlock.SetColor(TintID, tint);
        _spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
