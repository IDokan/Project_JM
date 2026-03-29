// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/18/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SetColorToParticles.cs
// Summary: A script to set color to the all particles.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

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

    public void SetColor(GemColor gemColor)
    {
        SetColor(GemColorUtility.ConvertGemColor(gemColor));
    }

    public void SetColor(Color color)
    {
        foreach (var particleSystem in particleSystems)
        {
            var main = particleSystem.main;
            main.startColor = color;
            particleSystem.Play(true);
        }
    }
}
