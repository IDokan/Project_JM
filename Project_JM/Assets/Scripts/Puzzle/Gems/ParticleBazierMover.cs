// SPDX-License-Identifier: MIT
// Copyright (c) 01/05/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParticleBazierMover.cs
// Summary: A script to move particle to a position using Bazier curve.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleBazierMover : MonoBehaviour
{
    [SerializeField] protected ParticleSystem controlledParticleSystem;
    [SerializeField] protected Transform target;        // @@ TODO: Change it characters later.
    [SerializeField] protected Vector3 targetPosition = new Vector3(5f, 0f, 0f);
    [SerializeField] protected float delay = 1f;

    [Header("Spring (tune these)")]
    [Tooltip("Bigger = pulls harder toward desired position.")]
    [SerializeField] private float stiffness = 60f;

    [Tooltip("Bigger = damps motion more quickly (units: 1/sec).")]
    [SerializeField] protected float damping = 12f;


    [Header("Optional: make outer particles behave differently (still deterministic)")]
    [Tooltip("0 = no difference. Positive = outer particles pull harder.")]
    [SerializeField] private float outerStiffnessBoost = 1f;
    private float _maxRadius = 1e-6f;

    protected ParticleSystem.Particle[] _particles;

    // randomSeed -> cached offset at the moment follow begins
    protected readonly Dictionary<uint, Vector3> _offsetByID = new(64);

    protected Vector3 _centerStart;
    protected bool _following;

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
        CacheOffsetsFromCenter(_centerStart);

        _following = true;
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

    protected void CacheOffsetsFromCenter(Vector3 center)
    {
        _offsetByID.Clear();
        _maxRadius = 1e-6f;

        int count = controlledParticleSystem.GetParticles(_particles);

        for (int i = 0; i < count; i++)
        {
            uint id = _particles[i].randomSeed;
            Vector3 offset = _particles[i].position - center;

            _offsetByID[id] = offset;
            float r = offset.magnitude;
            if (r > _maxRadius)
            {
                _maxRadius = r;
            }
        }

        controlledParticleSystem.SetParticles(_particles, count);
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

        Vector3 targetPos = target.position;

        int count = controlledParticleSystem.GetParticles(_particles);

        for (int i = 0; i < count; i++)
        {
            ref ParticleSystem.Particle p = ref _particles[i];
            uint id = p.randomSeed;

            if (!_offsetByID.TryGetValue(id, out Vector3 offset))
            {
                offset = p.position - _centerStart;
                _offsetByID[id] = offset;

                float r = offset.magnitude;
                if (r > _maxRadius)
                {
                    _maxRadius = r;
                }
            }

            Vector3 desired = targetPos;

            desired.z = p.position.z;

            float k = stiffness;
            if (outerStiffnessBoost > 0f)
            {
                float radius01 = Mathf.Clamp01(offset.magnitude / _maxRadius);
                k *= (1f + outerStiffnessBoost * radius01);
            }

            Vector3 pos = p.position;
            Vector3 vel = p.velocity;

            Vector3 to = desired - pos;

            vel += to * (k * dt);
            vel *= Mathf.Exp(-damping * dt);
            pos += vel * dt;

            p.velocity = vel;
            p.position = pos;

            controlledParticleSystem.SetParticles(_particles, count);
        }
    }
}
