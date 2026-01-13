// SPDX-License-Identifier: MIT
// Copyright (c) 01/04/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemResolver.cs
// Summary: A script to resolving gems.

using UnityEngine;
using GemEnums;
using System.Collections;
using System;

public class GemResolver : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer shinySP;
    [SerializeField] protected ParticleSystem bubblePS;
    [SerializeField] protected ParticleBazierMover mover;
    [SerializeField] protected Gem gem;
    [SerializeField] protected float resolverLifetime = 1f;
    [SerializeField] protected float spriteDisableDelay = 0.1f;

    static readonly int TintColorID = Shader.PropertyToID("_TintColor");

    MaterialPropertyBlock _materialPropertyBlock;

    Action<GemColor> _onAbsorbed;
    bool _absorbedCalled;

    protected void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    public void Init(GemColor gemColor, Transform target, Action<GemColor> onAbsorbed)
    {
        _onAbsorbed = onAbsorbed;

        SetGemType(gemColor);
        mover.SetTargetTransform(target);

        mover.Completed += HandleAbsorbed;

        StartCoroutine(FailSafeAbsorb(3f));
    }

    protected void HandleAbsorbed()
    {
        if (_absorbedCalled) return;
        _absorbedCalled = true;

        mover.Completed -= HandleAbsorbed;

        _onAbsorbed?.Invoke(gem.Color);
        Destroy(gameObject);
    }

    protected void SetGemType(GemColor gemColor)
    {
        gem.Init(gemColor);

        Color color = GemColorUtility.ConvertGemColor(gemColor);
        shinySP.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor(TintColorID, color);
        shinySP.SetPropertyBlock(_materialPropertyBlock);

        var main = bubblePS.main;
        main.startColor = color;
        bubblePS.Play(true);

        StartCoroutine(DisableSprite(spriteDisableDelay));
    }

    protected IEnumerator FailSafeAbsorb(float maxSeconds)
    {
        yield return new WaitForSeconds(maxSeconds);

        HandleAbsorbed();
    }

    protected IEnumerator DisableSprite(float delay)
    {
        yield return new WaitForSeconds(delay);
        shinySP.gameObject.SetActive(false);
    }
}
