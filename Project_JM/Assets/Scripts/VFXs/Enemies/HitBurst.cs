// SPDX-License-Identifier: MIT
// Copyright (c) 01/19/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: HitBurst.cs
// Summary: A script for main mechanism of hit burst.

using UnityEngine;

public class HitBurst : MonoBehaviour
{
    [SerializeField] protected SpriteRenderer[] spriteRenderers;
    [SerializeField] protected ParticleSystem[] particleSystems;

    protected void Awake()
    {
        // Auto-fill if nothing assigned
        if (spriteRenderers == null || spriteRenderers.Length == 0)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(includeInactive: true);

        if (particleSystems == null || particleSystems.Length == 0)
            particleSystems = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color color)
    {    
        // 1) SpriteRenderers tint
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            var spriteRenderer = spriteRenderers[i];
            if (!spriteRenderer) continue;
            spriteRenderer.color = color;
        }

        // 2) ParticleSystems start color (MOST COMMON)
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (!particleSystem) continue;

            var main = particleSystem.main;
            main.startColor = color;
        }
    }
}
