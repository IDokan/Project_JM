// SPDX-License-Identifier: MIT
// Copyright (c) 01/05/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParticleBazierMover.cs
// Summary: A script to move particle to a position using Bazier curve.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleBazierMover : MonoBehaviour
{
    [SerializeField] protected ParticleSystem controlledParticleSystem;
    [SerializeField] protected float delay = 1f;
    [SerializeField] protected float travelTime = 0.8f;

    [Header("Spring (tune these)")]
    [Tooltip("Bigger = pulls harder toward desired position.")]
    [SerializeField] private float stiffness = 60f;

    [Tooltip("Bigger = damps motion more quickly (units: 1/sec).")]
    [SerializeField] protected float damping = 12f;

    [SerializeField] protected float endOffsetWeight;
    [SerializeField] protected float bendScale = 1f;

    public event Action Completed;
    bool _completedFired = false;

    protected ParticleSystem.Particle[] _particles;
    protected Transform target;

    protected struct Info
    {
        public Vector3 startPos;
        public Vector3 offsetFromCenter;
        public float bend;
    }

    // randomSeed -> cached offset at the moment follow begins
    protected readonly Dictionary<uint, Info> _infoByID = new(64);

    protected Vector3 _centerStart;
    protected Vector3 _perpendicularAtStart;
    protected float _moveStartTime;
    protected bool _following = false;

    protected void Awake()
    {
        if (!controlledParticleSystem)
        {
            controlledParticleSystem = GetComponent<ParticleSystem>();
        }

        var main = controlledParticleSystem.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        EnsureBuffer();
    }

    protected void OnEnable()
    {
        StartCoroutine(MoveParticlesBazierAfterDelay());
    }

    protected IEnumerator MoveParticlesBazierAfterDelay()
    {
        // Wait until emitter works.
        yield return new WaitForSeconds(delay);

        if (!controlledParticleSystem || !target)
        {
            yield break;
        }

        EnsureBuffer();

        _centerStart = controlledParticleSystem.transform.position;

        Vector3 direction = (target.position - _centerStart);
        if (direction.sqrMagnitude < 1e-8f)
        {
            direction = Vector3.right;
        }

        direction.Normalize();
        _perpendicularAtStart = new Vector3(-direction.y, direction.x, 0f);

        CacheInfos();

        _moveStartTime = Time.time;
        _following = true;
    }

    protected void CacheInfos()
    {
        _infoByID.Clear();

        int count = controlledParticleSystem.GetParticles(_particles);
        for (int i = 0; i < count; i++)
        {
            uint id = _particles[i].randomSeed;

            Vector3 startPos = _particles[i].position;
            Vector3 offset = startPos - _centerStart;

            float bend = Vector3.Dot(offset, _perpendicularAtStart) * bendScale;

            _infoByID[id] = new Info
            {
                startPos = startPos,
                offsetFromCenter = offset,
                bend = bend
            };
        }
    }


    protected void EnsureBuffer()
    {
        var main = controlledParticleSystem.main;
        int max = Mathf.Max(4, main.maxParticles);

        if (_particles == null || _particles.Length < max)
        {
            _particles = new ParticleSystem.Particle[max];
        }
    }

    protected void LateUpdate()
    {
        if (!_following || !controlledParticleSystem || !target)
        {
            return;
        }

        float dt = Time.deltaTime;
        if (dt <= 0f)
        {
            return;
        }

        EnsureBuffer();

        float u = travelTime <= 1e-6f ? 1f : Mathf.Clamp01((Time.time - _moveStartTime) / travelTime);
        u = u * u * (3f - 2f * u);

        Vector3 targetPos = target.position;

        int count = controlledParticleSystem.GetParticles(_particles);

        for (int i = 0; i < count; i++)
        {
            ref ParticleSystem.Particle p = ref _particles[i];
            uint id = p.randomSeed;

            if (!_infoByID.TryGetValue(id, out Info info))
            {
                Vector3 startPos = p.position;
                Vector3 offset = startPos - _centerStart;
                float bend = Vector3.Dot(offset, _perpendicularAtStart) * bendScale;

                info = new Info
                {
                    startPos = startPos,
                    offsetFromCenter = offset,
                    bend = bend,
                };

                _infoByID[id] = info;
            }

            Vector3 endPosition = targetPos + info.offsetFromCenter * endOffsetWeight;
            endPosition.z = p.position.z;

            Vector3 direction = endPosition - info.startPos;
            Vector3 perpendicular;
            if (direction.sqrMagnitude < 1e-8f)
            {
                perpendicular = _perpendicularAtStart;
            }
            else
            {
                direction.Normalize();
                perpendicular = new Vector3(-direction.y, direction.x, 0f);
            }

            Vector3 control = (info.startPos + endPosition) * 0.5f + perpendicular * info.bend;

            Vector3 guide = BezierQuadratic(info.startPos, control, endPosition, u);

            Vector3 pos = p.position;
            Vector3 vel = p.velocity;

            Vector3 to = guide - pos;

            vel += to * (stiffness * dt);
            vel *= Mathf.Exp(-damping * dt);
            pos += vel * dt;

            p.velocity = vel;
            p.position = pos;

            float velocitySqrMagnitude = p.velocity.sqrMagnitude;
            // Kill particles if they arrived
            if (velocitySqrMagnitude <= 0.1f)
            {
                p.remainingLifetime = 0f;
            }

            // Update particle's opacity by velocity when they began floating to destination.
            if (u > 0.5f)
            {
                float clampedVelocitySqrMagnitude = Mathf.Clamp01(velocitySqrMagnitude / 20f);
                byte alpha = (byte)Mathf.RoundToInt(255 * clampedVelocitySqrMagnitude);

                Color32 c = p.startColor;
                c.a = alpha;
                p.startColor = c;
            }
        }

        controlledParticleSystem.SetParticles(_particles, count);

        if (_completedFired == false && u >= 1f && controlledParticleSystem.particleCount <= 0)
        {
            _completedFired = true;
            _following = false;
            Completed?.Invoke();
        }
    }

    protected Vector3 BezierQuadratic(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        float u = 1f - t;

        return (u * u) * p0 + (2f * u * t) * p1 + (t * t) * p2;
    }

    public void SetTargetTransform(Transform transform)
    {
        target = transform;
    }
}