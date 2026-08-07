// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardDismissParticleMover.cs
// Summary: Bursts a particle swarm outward from a point uniformly chosen
//          over a reward button's rectangle, one burst per Init() call, for
//          an unpicked reward button being dismissed. Unlike
//          RewardApplyParticleMover/RewardParticleMover, there is no travel
//          leg — each particle keeps the outward radiate velocity it's
//          emitted with for its whole life, so Unity's own particle
//          simulation carries it the rest of the way with no per-frame
//          script involvement. Fade-out is likewise left to the controlled
//          ParticleSystem's own authored Color-over-Lifetime gradient rather
//          than being computed here. Meant to live on the same GameObject as
//          RewardApplyParticleMover, sharing its ParticleSystem — the two
//          are mutually exclusive per offer (a button is either picked and
//          applies, or isn't and dismisses).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RewardDismissParticleMover : MonoBehaviour
{
    [SerializeField] protected ParticleSystem controlledParticleSystem;

    [Header("Burst")]
    // Automatic emission (the ParticleSystem's own burst) is disabled in
    // Awake — every particle instead comes from an explicit Emit() call in
    // EmitBurstAtButtonPoints, so this count range replaces whatever the
    // Emission module's burst used to be authored with.
    [SerializeField] protected int particleCountMin = 40;
    [SerializeField] protected int particleCountMax = 60;

    [Header("Radiate")]
    // Every particle's speed away from the button's center — same magnitude
    // for all, only direction varies per particle's own start point. Carried
    // for the particle's whole life by Unity's own simulation, since nothing
    // here overrides its velocity afterward.
    [SerializeField] protected float radiateSpeed = 0.6f;

    // Doubles as both "how long it keeps radiating" and "how long until it
    // disappears" — there's no separate travel phase to hand off to, so a
    // particle's startLifetime alone decides both. The controlled
    // ParticleSystem's own Color-over-Lifetime module is expected to fade
    // its alpha to 0 across that same span.
    [SerializeField] protected float lifetime = 0.6f;

    protected readonly Vector3[] _cornersBuffer = new Vector3[4];

    protected RectTransform _sourceButtonRect;
    protected float _worldDepthZ;

    protected void Awake()
    {
        if (!controlledParticleSystem)
        {
            controlledParticleSystem = GetComponent<ParticleSystem>();
        }

        ParticleSystem.MainModule main = controlledParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // This mover only ever spawns particles itself, via Emit() in
        // EmitBurstAtButtonPoints — the ParticleSystem's own authored burst
        // would otherwise fire alongside those and double the particle
        // count.
        ParticleSystem.EmissionModule emission = controlledParticleSystem.emission;
        emission.enabled = false;
    }

    // Bursts particles outward from a point uniformly chosen over
    // sourceButtonRect, one per particle, tinted with color (GemColor.None
    // reads as white). worldDepthZ pins those points onto the gameplay
    // plane, since the button's own RectTransform world z belongs to the UI
    // canvas, not gameplay space. Safe to call again on the same instance
    // for a later offer — any particles left over from a previous dismiss
    // are cleared first.
    public void Init(RectTransform sourceButtonRect, GemColor color, float worldDepthZ)
    {
        _sourceButtonRect = sourceButtonRect;
        _worldDepthZ = worldDepthZ;

        transform.position = sourceButtonRect.position;

        // Claims Color over Lifetime on this shared ParticleSystem —
        // RewardApplyParticleMover turns it off for its own use of the same
        // component (its alpha is driven entirely by code), so this mover
        // has to explicitly re-enable it here rather than assuming it's
        // still on from however the prefab was authored.
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = controlledParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;

        controlledParticleSystem.Clear(true);
        controlledParticleSystem.Play(true);

        EmitBurstAtButtonPoints(color);
    }

    // Spawns the whole burst directly via Emit(), one particle at a time,
    // each already at its own point on the button rect, with its outward
    // radiate velocity applied from frame one — nothing is spawned
    // elsewhere and then moved. startLifetime is set explicitly per
    // particle so it's driven by lifetime rather than whatever the
    // ParticleSystem's own Main module start lifetime is authored with.
    protected void EmitBurstAtButtonPoints(GemColor color)
    {
        RefreshCorners();

        Vector3 buttonCenter = BilinearPoint(0.5f, 0.5f);

        Color32 startColor = color.ConvertGemColorOrWhite();
        startColor.a = 255;

        int count = Random.Range(particleCountMin, particleCountMax + 1);
        for (int i = 0; i < count; i++)
        {
            Vector3 startPos = BilinearPoint(Random.value, Random.value);

            // Direction only, from the button's center out to this
            // particle's own start point — every particle gets the same
            // radiateSpeed regardless of how far it landed from center.
            Vector3 outward = startPos - buttonCenter;
            outward = outward.sqrMagnitude < 1e-8f ? Vector3.right : outward.normalized;

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = startPos,
                velocity = outward * radiateSpeed,
                startColor = startColor,
                startLifetime = lifetime,
            };
            controlledParticleSystem.Emit(emitParams, 1);
        }
    }

    // Refreshes _cornersBuffer for the current frame, pinned onto the
    // gameplay plane (see Init's summary for why: the button's own world z
    // belongs to the UI canvas, not gameplay space).
    protected void RefreshCorners()
    {
        _sourceButtonRect.GetWorldCorners(_cornersBuffer);

        for (int i = 0; i < _cornersBuffer.Length; i++)
        {
            Vector3 corner = _cornersBuffer[i];
            corner.z = _worldDepthZ;
            _cornersBuffer[i] = corner;
        }
    }

    // Bilinear point within the button's rectangle for the current frame's
    // corners — (0,0) is bottom-left, (1,1) is top-right, (0.5,0.5) is
    // center. Uniform (u, v) in [0,1) gives a uniform spread of points over
    // the button image, since a RectTransform's corners form a plain
    // rectangle (or a rotated one, never a sheared quad).
    protected Vector3 BilinearPoint(float u, float v)
    {
        // GetWorldCorners order: 0 bottom-left, 1 top-left, 2 top-right, 3 bottom-right.
        Vector3 bottom = Vector3.Lerp(_cornersBuffer[0], _cornersBuffer[3], u);
        Vector3 top = Vector3.Lerp(_cornersBuffer[1], _cornersBuffer[2], u);
        return Vector3.Lerp(bottom, top, v);
    }
}
