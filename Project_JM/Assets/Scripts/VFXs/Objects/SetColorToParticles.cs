// SPDX-License-Identifier: MIT
// Copyright (c) 03/18/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SetColorToParticles.cs
// Summary: A script to set color to the all particles.

using UnityEngine;
using GemEnums;

public class SetColorToParticles : MonoBehaviour
{
    [SerializeField] protected ParticleSystem[] particleSystems;

    protected void Awake()
    {
        if (particleSystems == null)
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
    }

    protected void SetColor(GemColor gemColor)
    {
        SetColor(GemColorUtility.ConvertGemColor(gemColor));
    }

    protected void SetColor(Color color)
    {
        foreach (var particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            main.startColor = color;
            particleSystem.Play(true);
        }
    }
}
