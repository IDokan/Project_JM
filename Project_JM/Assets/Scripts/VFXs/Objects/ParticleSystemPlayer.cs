// SPDX-License-Identifier: MIT
// Copyright (c) 02/10/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParticleSystemPlayer.cs
// Summary: A script to player particle system in code.

using UnityEngine;

public class ParticleSystemPlayer : MonoBehaviour
{
    [SerializeField] protected ParticleSystem particle;

    protected void Awake()
    {
        if ( particle == null )
        {
            particle = GetComponent<ParticleSystem>();
        }
    }

    // This method will be called by the Animation Event
    public void AnimEvent_PlayParticleSystem()
    {
        particle.Play();
    }
}
