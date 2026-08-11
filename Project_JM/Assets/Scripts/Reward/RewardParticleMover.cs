// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardParticleMover.cs
// Summary: Flies a particle swarm from wherever this GameObject is placed
//          (the reward chest) to a point uniformly chosen over an offer
//          button's rectangle, each particle along its own spring-damped
//          Bezier arc, tinted with a single GemColor for the whole burst
//          (white for GemColor.None) — unlike RewardApplyParticleMover,
//          this leg never splits across multiple targets/colors, since it
//          only ever has the one button as its destination. The controlled
//          ParticleSystem is a single burst — every particle exists by the
//          time BeginTravel runs, so the whole swarm shares one travel timer.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using GemEnums;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RewardParticleMover : MonoBehaviour
{
    [SerializeField] protected ParticleSystem controlledParticleSystem;
    [SerializeField] protected ParticleSystemRenderer controlledRenderer;

    [SerializeField] protected float delay = 1f;

    [Header("Travel (spring, tune these)")]
    [SerializeField] protected float travelTime = 0.8f;
    [SerializeField] protected float travelTimeScaleMin = 0.8f;
    [SerializeField] protected float travelTimeScaleMax = 1.2f;
    [SerializeField] protected float stiffness = 60f;
    [SerializeField] protected float damping = 12f;
    [SerializeField] protected float bendScale = 1f;

    [Header("Fade (proximity to target)")]
    // Squared against distance to destination each frame, not sqrt'd — a
    // particle is fully opaque once farther than this from its target and
    // fades linearly to transparent as it closes the remaining distance.
    [SerializeField] protected float fadeRadius = 2f;

    // Earliest/latest a particle in this swarm can arrive at its
    // destination, measured from Init() — delay (the pre-travel hold
    // BeginTravelAfterDelay waits out before BeginTravel actually starts
    // moving anything) plus travelTime scaled by the fastest/slowest a
    // particle's own duration can randomize to (see
    // TravelTimeScaleFromSeed). RewardOfferUI uses this pair to time its
    // buttons' fade-in so they reach full opacity in step with the swarm
    // actually arriving.
    public float MinTravelDuration => delay + travelTime * travelTimeScaleMin;
    public float MaxTravelDuration => delay + travelTime * travelTimeScaleMax;

    protected struct TravelInfo
    {
        public Vector3 startPos;
        public float bend;
        public float destU;
        public float destV;
        public float duration;
    }

    protected ParticleSystem.Particle[] _particles;
    protected readonly Dictionary<uint, TravelInfo> _travelInfoByID = new(64);
    protected readonly Vector3[] _cornersBuffer = new Vector3[4];

    protected RectTransform _buttonRect;
    protected float _worldDepthZ;

    protected Vector3 _travelPerpendicularAtStart;
    protected float _travelStartTime;
    protected bool _isTraveling;
    protected Coroutine _travelDelayRoutine;

    protected void Awake()
    {
        if (!controlledParticleSystem)
        {
            controlledParticleSystem = GetComponent<ParticleSystem>();
        }
        if (!controlledRenderer)
        {
            controlledRenderer = GetComponent<ParticleSystemRenderer>();
        }

        ParticleSystem.MainModule main = controlledParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        EnsureBuffer();
    }

    // Starts the swarm's journey from wherever this GameObject was placed
    // (the chest) toward a point uniformly chosen over buttonRect, one per
    // particle, every particle tinted with color (GemColor.None reads as
    // white). worldDepthZ pins those points onto the gameplay plane, since
    // the button's own RectTransform world z belongs to the UI canvas, not
    // gameplay space. Safe to call again on the same instance for a later
    // chest — any particles and travel state left over from a previous run
    // are cleared first, so this one mover can be reused for every reward
    // offer.
    public void Init(RectTransform buttonRect, GemColor color, float worldDepthZ)
    {
        _buttonRect = buttonRect;
        _worldDepthZ = worldDepthZ;

        _isTraveling = false;
        if (_travelDelayRoutine != null)
        {
            StopCoroutine(_travelDelayRoutine);
        }

        controlledRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;

        // Applied through the Main module (rather than an explicit Emit()
        // per particle, like RewardApplyParticleMover) since this mover
        // still relies on the ParticleSystem's own authored burst to spawn
        // particles — every particle Play() spawns picks up this startColor.
        ParticleSystem.MainModule main = controlledParticleSystem.main;
        main.startColor = new ParticleSystem.MinMaxGradient(color.ConvertGemColorOrWhite());

        controlledParticleSystem.Clear(true);
        controlledParticleSystem.Play(true);
        _travelDelayRoutine = StartCoroutine(BeginTravelAfterDelay());
    }

    protected IEnumerator BeginTravelAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        controlledRenderer.maskInteraction = SpriteMaskInteraction.None;

        BeginTravel(controlledParticleSystem.transform.position);
        _isTraveling = true;
        _travelDelayRoutine = null;
    }

    protected void LateUpdate()
    {
        if (_isTraveling)
        {
            UpdateTravel();
        }
    }

    protected void BeginTravel(Vector3 centerStart)
    {
        EnsureBuffer();

        _travelInfoByID.Clear();
        _travelStartTime = Time.time;

        RefreshCorners();

        // Only used to orient the swarm's overall bend axis (chest → button
        // center) — each particle's actual destination is its own point,
        // assigned below.
        Vector3 direction = BilinearPoint(0.5f, 0.5f) - centerStart;
        if (direction.sqrMagnitude < 1e-8f)
        {
            direction = Vector3.right;
        }
        direction.Normalize();
        _travelPerpendicularAtStart = new Vector3(-direction.y, direction.x, 0f);

        // The burst has already fully spawned by the time delay elapses, so
        // every particle that will ever exist for this run is captured here.
        int count = controlledParticleSystem.GetParticles(_particles);
        for (int i = 0; i < count; i++)
        {
            ParticleSystem.Particle p = _particles[i];
            uint id = p.randomSeed;
            UvFromSeed(id, out float destU, out float destV);
            float duration = travelTime * TravelTimeScaleFromSeed(id);

            _travelInfoByID[id] = new TravelInfo { startPos = p.position, bend = BendFromSeed(id), destU = destU, destV = destV, duration = duration };
        }
    }

    protected void UpdateTravel()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        EnsureBuffer();

        RefreshCorners();

        int count = controlledParticleSystem.GetParticles(_particles);

        for (int i = 0; i < count; i++)
        {
            ref ParticleSystem.Particle p = ref _particles[i];
            TravelInfo info = _travelInfoByID[p.randomSeed];

            // Every particle shares the same start (the burst), but travels
            // at its own duration (info.duration, randomized ±20% around
            // travelTime), so u/eased and the fade below are per-particle.
            float u = info.duration <= 1e-6f ? 1f : Mathf.Clamp01((Time.time - _travelStartTime) / info.duration);
            float eased = u * u * (3f - 2f * u);

            // Live-recomputed from this frame's corners (not cached), so a
            // particle's destination stays proportionally correct within
            // the button even if the layout reflows mid-travel.
            Vector3 endPosition = BilinearPoint(info.destU, info.destV);
            endPosition.z = p.position.z;

            Vector3 direction = endPosition - info.startPos;
            Vector3 perpendicular;
            if (direction.sqrMagnitude < 1e-8f)
            {
                perpendicular = _travelPerpendicularAtStart;
            }
            else
            {
                direction.Normalize();
                perpendicular = new Vector3(-direction.y, direction.x, 0f);
            }

            Vector3 control = (info.startPos + endPosition) * 0.5f + perpendicular * info.bend;
            Vector3 guide = BezierQuadratic(info.startPos, control, endPosition, eased);

            Vector3 pos = p.position;
            Vector3 vel = p.velocity;
            IntegrateSpring(ref pos, ref vel, guide, dt, stiffness, damping);
            p.velocity = vel;
            p.position = pos;

            // Opacity tracks actual remaining distance to the destination
            // (not travel time), same as ParticleBazierMover — fully opaque
            // until within fadeRadius, then fades to fully transparent by
            // the time it's down to a quarter of that distance, rather than
            // stretching the fade all the way down to 0 distance.
            float distSqr = (pos - endPosition).sqrMagnitude;
            float quarterFadeRadius = fadeRadius * 0.25f;
            float alpha = Mathf.InverseLerp(quarterFadeRadius * quarterFadeRadius, fadeRadius * fadeRadius, distSqr);
            byte alphaByte = (byte)Mathf.RoundToInt(255f * alpha);

            Color32 color = p.startColor;
            color.a = alphaByte;
            p.startColor = color;
        }

        controlledParticleSystem.SetParticles(_particles, count);
    }

    protected static void IntegrateSpring(ref Vector3 pos, ref Vector3 vel, Vector3 guide, float dt, float stiffnessValue, float dampingValue)
    {
        Vector3 to = guide - pos;

        vel += to * (stiffnessValue * dt);
        vel *= Mathf.Exp(-dampingValue * dt);
        pos += vel * dt;
    }

    // A deterministic sideways bend for this particle, independent of how
    // much it may have physically scattered since spawning. randomSeed is
    // already a well-distributed pseudo-random value Unity assigns per
    // particle, so mapping it linearly into [-1, 1] gives every particle a
    // stable, varied curve without depending on emission timing/spread.
    protected float BendFromSeed(uint seed)
    {
        float normalized = seed / (float)uint.MaxValue;
        return (normalized * 2f - 1f) * bendScale;
    }

    // Refreshes _cornersBuffer for the current frame, pinned onto the
    // gameplay plane (see Init's summary for why: the button's own world z
    // belongs to the UI canvas, not gameplay space).
    protected void RefreshCorners()
    {
        _buttonRect.GetWorldCorners(_cornersBuffer);

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

    // Each particle's fixed destination point within the button, as
    // bilinear (u, v). The two hashes need to be decorrelated — reusing the
    // same normalized seed for both would put every particle on the
    // diagonal instead of spread across the whole rectangle.
    protected static void UvFromSeed(uint seed, out float u, out float v)
    {
        u = seed / (float)uint.MaxValue;
        v = (seed * 2654435761u) / (float)uint.MaxValue;
    }

    // This particle's own travel-time multiplier, spread across
    // [travelTimeScaleMin, travelTimeScaleMax]. Yet another hash constant
    // than BendFromSeed/UvFromSeed use, so a particle's speed doesn't
    // visibly correlate with its bend or destination.
    protected float TravelTimeScaleFromSeed(uint seed)
    {
        float normalized = (seed * 2246822519u) / (float)uint.MaxValue;
        return Mathf.Lerp(travelTimeScaleMin, travelTimeScaleMax, normalized);
    }

    protected void EnsureBuffer()
    {
        if (_particles != null && controlledParticleSystem.particleCount <= _particles.Length)
        {
            return;
        }

        ParticleSystem.MainModule main = controlledParticleSystem.main;
        int max = Mathf.Max(4, main.maxParticles);

        if (_particles == null || _particles.Length < max)
        {
            _particles = new ParticleSystem.Particle[max];
        }
    }

    protected static Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;

        return (u * u) * p0 + (2f * u * t) * p1 + (t * t) * p2;
    }
}
