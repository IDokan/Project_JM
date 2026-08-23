// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 06/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardApplyParticleMover.cs
// Summary: Flies a particle swarm from a point uniformly chosen over a
//          reward button's rectangle to one or more designated target
//          Transforms, each particle along its own spring-damped Bezier arc,
//          tinted with its own target's GemColor (white for GemColor.None).
//          Given several targets, particles are split evenly across them
//          (by index, round-robin) — e.g. a party-wide reward converging a
//          quarter of the swarm on each of four characters. The mirror of
//          RewardParticleMover's chest-to-button leg: this one carries the
//          chosen reward away from the button once it's picked. The
//          controlled ParticleSystem's own automatic emission is disabled —
//          every particle is spawned by explicit Emit() calls directly at
//          its assigned button-rect point with its radiate velocity already
//          set, so it never exists anywhere else first (spawning, then
//          overwriting position a frame later, made the Trails module draw
//          one long streak for that initial jump).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using GemEnums;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class RewardApplyParticleMover : MonoBehaviour
{
    [SerializeField] protected ParticleSystem controlledParticleSystem;

    [SerializeField] protected float delay = 1.6f;

    [Header("Burst")]
    // Automatic emission (the ParticleSystem's own burst) is disabled in
    // Awake — every particle instead comes from an explicit Emit() call in
    // EmitBurstAtButtonPoints, so this count range replaces whatever the
    // Emission module's burst used to be authored with.
    [SerializeField] protected int particleCountMin = 80;
    [SerializeField] protected int particleCountMax = 120;

    [Header("Radiate (during delay, before travel)")]
    // Every particle's speed away from the button's center while it holds
    // during delay — same magnitude for all, only direction varies per
    // particle's own start point.
    [SerializeField] protected float radiateSpeed = 0.1f;

    [Header("Travel (spring, tune these)")]
    [SerializeField] protected float travelTime = 0.8f;
    [SerializeField] protected float travelTimeScaleMin = 0.8f;
    [SerializeField] protected float travelTimeScaleMax = 1.2f;
    [SerializeField] protected float stiffness = 12f;
    [SerializeField] protected float damping = 9f;
    [SerializeField] protected float bendScale = 3f;

    [Header("Fade (proximity to target)")]
    // Squared against distance to the target each frame, not sqrt'd — a
    // particle is fully opaque once farther than this from its target and
    // fades linearly to transparent as it closes the remaining distance.
    [SerializeField] protected float fadeRadius = 2f;

    protected struct TravelInfo
    {
        public Vector3 startPos;
        public float bend;
        public float duration;
        public int targetIndex;
    }

    // Worst-case time from Init() to the last particle arriving at its
    // target: the delay before travel starts, plus travelTime scaled by the
    // slowest a particle's duration can randomize to. Callers should prefer
    // subscribing to Completed for the real finish time — this is only a
    // fail-safe upper bound for callers that need one regardless (e.g. to
    // avoid waiting forever if Completed never fires).
    public float MaxTravelDuration => delay + travelTime * travelTimeScaleMax;

    // Fires once every particle in the swarm has actually faded out and
    // died (see UpdateTravel's remainingLifetime kill), same contract as
    // ParticleBazierMover.Completed — tells a caller (RewardOfferUI) the
    // real moment the swarm finished, rather than a guessed duration.
    public event System.Action Completed;

    protected bool _completedFired;

    protected ParticleSystem.Particle[] _particles;
    protected readonly Dictionary<uint, TravelInfo> _travelInfoByID = new(64);
    protected readonly Vector3[] _cornersBuffer = new Vector3[4];

    protected RectTransform _sourceButtonRect;
    protected Transform[] _targets;
    protected float _worldDepthZ;
    protected Color32[] _targetColors;

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

        ParticleSystem.MainModule main = controlledParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        // This mover only ever spawns particles itself, via Emit() in
        // EmitBurstAtButtonPoints — the ParticleSystem's own authored burst
        // would otherwise fire alongside those and double the particle
        // count.
        ParticleSystem.EmissionModule emission = controlledParticleSystem.emission;
        emission.enabled = false;

        EnsureBuffer();
    }

    // Single-target convenience overload — see the (RectTransform, Transform[], GemColor[], float) Init for details.
    public void Init(RectTransform sourceButtonRect, Transform target, GemColor color, float worldDepthZ)
    {
        Init(sourceButtonRect, new[] { target }, new[] { color }, worldDepthZ);
    }

    // Starts the swarm's journey from a point uniformly chosen over
    // sourceButtonRect, one per particle, toward targets — split evenly
    // across them by index (round-robin), so with four targets roughly a
    // quarter of the swarm converges on each. colors is index-aligned with
    // targets — each particle is tinted with its own target's raw GemColor
    // (GemColor.None reads as white), so e.g. a Berserked swarm shows each
    // quarter in the color of the character it's headed toward rather than
    // one color for the whole burst. worldDepthZ pins those source points
    // onto the gameplay plane, since the button's own RectTransform world z
    // belongs to the UI canvas, not gameplay space. Safe to call again on
    // the same instance for a later reward — any particles and travel state
    // left over from a previous run are cleared first, so this one mover
    // can be reused for every reward pick.
    public void Init(RectTransform sourceButtonRect, Transform[] targets, GemColor[] colors, float worldDepthZ)
    {
        _sourceButtonRect = sourceButtonRect;
        _targets = targets;
        _worldDepthZ = worldDepthZ;

        _targetColors = new Color32[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            _targetColors[i] = colors[i].ConvertGemColorOrWhite();
        }

        transform.position = sourceButtonRect.position;

        _isTraveling = false;
        _completedFired = false;
        if (_travelDelayRoutine != null)
        {
            StopCoroutine(_travelDelayRoutine);
        }

        // Claims Color over Lifetime off this shared ParticleSystem —
        // RewardDismissParticleMover turns it on for its own use of the
        // same component, and this mover's own alpha is driven entirely by
        // code (see UpdateTravel's distance-based fade), so it can't be
        // left enabled or the two would multiply together.
        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = controlledParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = false;

        controlledParticleSystem.Clear(true);
        controlledParticleSystem.Play(true);

        EmitBurstAtButtonPoints();

        _travelDelayRoutine = StartCoroutine(BeginTravelAfterDelay());
    }

    protected IEnumerator BeginTravelAfterDelay()
    {
        yield return new WaitForSeconds(delay);

        BeginTravel();
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

    // Spawns the whole burst directly via Emit(), one particle at a time,
    // each already at its own point on the button rect, tinted with its
    // target's color (_targetColors[targetIndex]), with its outward radiate
    // velocity applied from frame one — nothing is spawned elsewhere and
    // then moved, so there's no jump for Trails to draw a streak across.
    // Every other field left unset on EmitParams (lifetime, size, ...)
    // still comes from the ParticleSystem's own Main/Size modules, same as
    // an authored burst would produce.
    protected void EmitBurstAtButtonPoints()
    {
        _travelInfoByID.Clear();

        RefreshCorners();

        Vector3 buttonCenter = BilinearPoint(0.5f, 0.5f);

        int count = Random.Range(particleCountMin, particleCountMax + 1);
        for (int i = 0; i < count; i++)
        {
            // 0 tells Unity to auto-generate a seed, so ids start at 1 —
            // assigned by us instead of read back from Unity, since we
            // already know each particle's start point and don't need to
            // reverse-derive it from a seed anymore.
            uint id = (uint)(i + 1);

            Vector3 startPos = BilinearPoint(Random.value, Random.value);

            // Direction only, from the button's center out to this
            // particle's own start point — every particle gets the same
            // radiateSpeed regardless of how far it landed from center.
            Vector3 outward = startPos - buttonCenter;
            outward = outward.sqrMagnitude < 1e-8f ? Vector3.right : outward.normalized;

            float duration = travelTime * Mathf.Lerp(travelTimeScaleMin, travelTimeScaleMax, Random.value);
            int targetIndex = i % _targets.Length;

            // bend is filled in by BeginTravel once the swarm's perpendicular
            // axis (button center -> targets) is known — see there for the
            // Vector3.Dot(offset, perpendicular) * bendScale law, mirroring
            // ParticleBazierMover.CacheInfos.
            _travelInfoByID[id] = new TravelInfo { startPos = startPos, duration = duration, targetIndex = targetIndex };

            // Alpha starts fully opaque regardless of the target color's own
            // alpha — UpdateTravel drives alpha itself over the fade-out,
            // only ever touching p.startColor.a and leaving this RGB alone.
            Color32 startColor = _targetColors[targetIndex];
            startColor.a = 255;

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                position = startPos,
                velocity = outward * radiateSpeed,
                randomSeed = id,
                startColor = startColor,
            };
            controlledParticleSystem.Emit(emitParams, 1);
        }
    }

    protected void BeginTravel()
    {
        _travelStartTime = Time.time;

        RefreshCorners();

        Vector3 buttonCenter = BilinearPoint(0.5f, 0.5f);

        // Only used to orient the swarm's overall bend axis (button center →
        // targets) — each particle's actual start point (and own target)
        // was already fixed by EmitBurstAtButtonPoints. With several
        // targets, their average stands in for "the target" here since this
        // axis is just a shared visual flourish, not per-particle accurate.
        Vector3 averageTarget = Vector3.zero;
        for (int i = 0; i < _targets.Length; i++)
        {
            averageTarget += TargetPosition(i);
        }
        averageTarget /= _targets.Length;

        Vector3 direction = averageTarget - buttonCenter;
        if (direction.sqrMagnitude < 1e-8f)
        {
            direction = Vector3.right;
        }
        direction.Normalize();
        _travelPerpendicularAtStart = new Vector3(-direction.y, direction.x, 0f);

        // Same as ParticleBazierMover.CacheInfos: re-anchor startPos to each
        // particle's actual current position (after radiating outward during
        // the delay) rather than reusing its stale pre-radiate spawn point —
        // otherwise the spring's guide curve starts behind where the
        // particle already is, and the first bit of travel snaps it
        // backward toward that stale point before swinging back out.
        // bend follows the same law as CacheInfos too: this refreshed
        // startPos's offset from center, projected onto the swarm's
        // perpendicular axis, so a particle now further off to one side
        // bends further along with it.
        EnsureBuffer();
        int liveCount = controlledParticleSystem.GetParticles(_particles);
        for (int i = 0; i < liveCount; i++)
        {
            ParticleSystem.Particle p = _particles[i];
            if (!_travelInfoByID.TryGetValue(p.randomSeed, out TravelInfo info))
            {
                continue;
            }

            info.startPos = p.position;
            info.bend = Vector3.Dot(info.startPos - buttonCenter, _travelPerpendicularAtStart) * bendScale;
            _travelInfoByID[p.randomSeed] = info;
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

        int count = controlledParticleSystem.GetParticles(_particles);

        for (int i = 0; i < count; i++)
        {
            ref ParticleSystem.Particle p = ref _particles[i];
            TravelInfo info = _travelInfoByID[p.randomSeed];

            // Live-read every frame (not cached), so the swarm still
            // converges correctly even if its target is itself moving. Each
            // particle resolves its own target by info.targetIndex, set at
            // spawn time by EmitBurstAtButtonPoints.
            Vector3 endPosition = TargetPosition(info.targetIndex);

            // Travels at its own duration (info.duration, randomized ±20%
            // around travelTime), so u/eased and the fade below are
            // per-particle.
            float u = info.duration <= 1e-6f ? 1f : Mathf.Clamp01((Time.time - _travelStartTime) / info.duration);
            float eased = u * u * (3f - 2f * u);

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

            // Opacity tracks actual remaining distance to the target (not
            // travel time), same as ParticleBazierMover — fully opaque until
            // within fadeRadius, then fades to fully transparent by the time
            // it's down to a quarter of that distance, rather than
            // stretching the fade all the way down to 0 distance.
            float distSqr = (pos - endPosition).sqrMagnitude;
            float quarterFadeRadius = fadeRadius * 0.25f;
            float alpha = Mathf.InverseLerp(quarterFadeRadius * quarterFadeRadius, fadeRadius * fadeRadius, distSqr);
            byte alphaByte = (byte)Mathf.RoundToInt(255f * alpha);

            Color32 color = p.startColor;
            color.a = alphaByte;
            p.startColor = color;

            // Fully faded means invisible — kill it here rather than
            // waiting on its own Start Lifetime, so Completed below fires
            // as soon as the swarm is actually done rather than whenever
            // the ParticleSystem's authored lifetime happens to run out.
            if (distSqr <= quarterFadeRadius * quarterFadeRadius)
            {
                p.remainingLifetime = 0f;
            }
        }

        controlledParticleSystem.SetParticles(_particles, count);

        if (!_completedFired && controlledParticleSystem.particleCount <= 0)
        {
            _completedFired = true;
            _isTraveling = false;
            Completed?.Invoke();
        }
    }

    protected static void IntegrateSpring(ref Vector3 pos, ref Vector3 vel, Vector3 guide, float dt, float stiffnessValue, float dampingValue)
    {
        Vector3 to = guide - pos;

        vel += to * (stiffnessValue * dt);
        vel *= Mathf.Exp(-dampingValue * dt);
        pos += vel * dt;
    }

    // A target's world position, pinned onto the gameplay plane like
    // RefreshCorners pins the button corners — needed because targets aren't
    // always gameplay Transforms (party characters); hpIconTarget and
    // comboIconTarget are RectTransforms on the UI canvas, whose own world z
    // belongs to that canvas, not gameplay space, and would otherwise throw
    // off both the travel curve and the proximity-based fade in UpdateTravel.
    protected Vector3 TargetPosition(int targetIndex)
    {
        Vector3 position = _targets[targetIndex].position;
        position.z = _worldDepthZ;
        return position;
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
