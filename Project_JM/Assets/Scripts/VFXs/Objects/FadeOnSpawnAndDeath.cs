// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FadeOnSpawnAndDeath.cs
// Summary: A script to fade object on its spawn and death.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOnSpawnAndDeath : MonoBehaviour
{
    [SerializeField] protected float fadeDuration = 0.2f;
    [SerializeField] protected float defaultTargetAlpha = 0.5f;

    [SerializeField] protected bool changeParticleSystemColor = false;

    protected SpriteRenderer[] _spriteRenderers;
    protected ParticleSystem[] _particleSystems;
    protected Coroutine _fadeRoutine;

    protected void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        if (changeParticleSystemColor)
        {
            _particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
        }
    }

    protected void OnEnable()
    {
        SetSpriteAlpha(0f);
        if (changeParticleSystemColor)
        {
            SetParticleAlpha(0f);
        }
        StartFade(defaultTargetAlpha);
    }

    public void FadeOutAndDestroy()
    {
        StartFade(0f, true);
    }

    protected void StartFade(float targetAlpha, bool destroyOnComplete = false)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }

        _fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, destroyOnComplete));
    }

    protected IEnumerator FadeRoutine(float targetAlpha, bool destroyOnComplete)
    {
        List<float> startAlphas = new();

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            float startAlpha = _spriteRenderers[i].color.a;

            startAlphas.Add(startAlpha);
        }

        List<float> startParticleAlphas = new();
        if (changeParticleSystemColor)
        {

            for (int i = 0; i < _particleSystems.Length; i++)
            {
                float startAlpha = _particleSystems[i].main.startColor.color.a;

                startParticleAlphas.Add(startAlpha);
            }
        }

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            for (int i = 0; i < startAlphas.Count; i++)
            {
                float alpha = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                SetSpriteAlpha(alpha, i);
            }

            if (changeParticleSystemColor)
            {
                for (int i = 0; i < startParticleAlphas.Count; i++)
                {
                    float alpha = Mathf.Lerp(startParticleAlphas[i], targetAlpha, t);
                    SetParticleAlpha(alpha, i);
                }
            }

            yield return null;
        }

        SetSpriteAlpha(targetAlpha);

        if (changeParticleSystemColor)
        {
            SetParticleAlpha(targetAlpha);
        }

        _fadeRoutine = null;

        if(destroyOnComplete)
        {
            Destroy(gameObject);
        }

    }

    protected void SetSpriteAlpha(float alpha, int index = -1)
    {
        if (index < 0)
        {
            for (int i = 0; i < _spriteRenderers.Length; i++)
            {
                Color colorSprite = _spriteRenderers[i].color;
                colorSprite.a = alpha;
                _spriteRenderers[i].color = colorSprite;
            }

            return;
        }

        Color color = _spriteRenderers[index].color;
        color.a = alpha;
        _spriteRenderers[index].color = color;
    }

    protected void SetParticleAlpha(float alpha, int index = -1)
    {
        if (index < 0)
        {
            for (int i = 0; i < _particleSystems.Length; i++)
            {
                var mainParticle = _particleSystems[i].main;
                Color particleColor = mainParticle.startColor.color;
                particleColor.a = alpha;
                mainParticle.startColor = particleColor;
            }

            return;
        }

        var main = _particleSystems[index].main;
        Color color = main.startColor.color;
        color.a = alpha;
        main.startColor = color;
    }
}
