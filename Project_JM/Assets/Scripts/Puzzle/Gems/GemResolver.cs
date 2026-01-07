// SPDX-License-Identifier: MIT
// Copyright (c) 01/04/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemResolver.cs
// Summary: A script to resolving gems.

using UnityEngine;
using GemEnums;
using System.Collections;

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

    protected void Awake()
    {
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGemType(GemColor gemColor)
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
        StartCoroutine(DestroySelf(resolverLifetime));

    }

    public void SetTargetTransform(Transform transform)
    {
        mover.SetTargetTransform(transform);
    }

    protected IEnumerator DestroySelf(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }

    protected IEnumerator DisableSprite(float delay)
    {
        yield return new WaitForSeconds(delay);
        shinySP.gameObject.SetActive(false);
    }
}
