// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SnailWizardStateVisual.cs
// Summary: SnailWizard-specific visual responses — eye/mouth sprite changes and attack flash.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class SnailWizardStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string deadEyeLabel;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string normalAttackMouthLabel;
    [SerializeField] private string enragedMouthLabel;
    [SerializeField] private string enragedAttackMouthLabel;
    [SerializeField] private string damagedMouthLabel;

    [Header("Attack Flash")]
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float maxAmount = 0.6f;
    [SerializeField] private AnimationCurve glowCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );
    [SerializeField] private Material targetMaterial;

    private bool _isEnraged;
    private Material _material;
    private Coroutine _routine;

    private void Awake()
    {
        _material = targetMaterial;
    }

    private void OnEnable()
    {
        if (_material != null)
        {
            _material.SetFloat("_FlashAmount", 0f);
        }
    }

    private void OnDisable()
    {
        if (_material != null)
        {
            _material.SetFloat("_FlashAmount", 0f);
        }
    }

    public override void OnEnraged()
    {
        _isEnraged = true;
        SetMouth(enragedMouthLabel);
    }

    public override void OnStunBegin()
    {
        SetEye(deadEyeLabel);
        SetMouth(damagedMouthLabel);
    }

    public override void OnStunEnd()
    {
        SetEye(normalEyeLabel);
        SetMouth(_isEnraged ? enragedMouthLabel : normalMouthLabel);
    }

    public override void OnDied()
    {
        SetEye(deadEyeLabel);
    }

    public override void OnAttack(Vector3 moveOffset)
    {
        SetMouth(_isEnraged ? enragedAttackMouthLabel : normalAttackMouthLabel);

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }
        _routine = StartCoroutine(GlowRoutine());
    }

    public override void OnAttackEnd()
    {
        SetMouth(_isEnraged ? enragedMouthLabel : normalMouthLabel);
    }

    private IEnumerator GlowRoutine()
    {
        if (_material == null)
        {
            yield break;
        }

        _material.SetColor("_FlashColor", glowColor);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _material.SetFloat("_FlashAmount", glowCurve.Evaluate(t) * maxAmount);
            yield return null;
        }
        _material.SetFloat("_FlashAmount", 0f);
        _routine = null;
    }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }
}
