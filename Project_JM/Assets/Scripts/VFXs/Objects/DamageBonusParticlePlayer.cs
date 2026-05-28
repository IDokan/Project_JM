// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageBonusParticlePlayer.cs
// Summary: Plays or stops a particle system and scales its emission rate by the damage bonus.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class DamageBonusParticlePlayer : MonoBehaviour
{
    [SerializeField] private DamageBonusChangedEventChannel damageBonusChangedEventChannel;
    [SerializeField] private ParticleSystem particle;
    // 14/4/2026 edited: damage bonus per blessing 15%, expected particle num per blessig 5, =>> 100/15*5 =~ 30.
    [SerializeField] private float baseEmissionRate = 30f;

    protected void Awake()
    {
        if (particle == null)
        {
            particle = GetComponent<ParticleSystem>();
        }
    }

    protected void OnEnable()  => damageBonusChangedEventChannel.OnRaised += OnDamageBonusChanged;
    protected void OnDisable() => damageBonusChangedEventChannel.OnRaised -= OnDamageBonusChanged;

    private void OnDamageBonusChanged(float newBonus)
    {
        if (newBonus > 1f)
        {
            var emission = particle.emission;
            emission.rateOverTime = (newBonus - 1f) * baseEmissionRate;
            particle.Play();
        }
        else
        {
            particle.Stop();
        }
    }
}
