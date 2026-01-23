// SPDX-License-Identifier: MIT
// Copyright (c) 01/21/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RotateStunStars.cs
// Summary: A script to rotate stars to represent stun status.

using UnityEngine;

public class RotateStunStars : MonoBehaviour
{
    [SerializeField] protected GameObject starPrefab;

    [Min(1)]
    [SerializeField] protected int starCount = 8;

    [SerializeField] protected Vector2 ellipseScale = new Vector2(0.5f, 0.5f);

    [SerializeField] protected float orbitCyclesPerSecond = 0.25f;

    [SerializeField] protected float selfSpinPerDegreePerSecond = 180;

    [SerializeField] protected float duration = 5f;


    [Header("Fake Perspective")]
    [SerializeField] protected float minScale = 0.65f;
    [SerializeField] protected float maxScale = 1.15f;
    [SerializeField] protected AnimationCurve perspectiveCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] protected bool flipPerspective = true;
    [SerializeField] protected int initialDepth = 100;
    [SerializeField] protected float depthScaler = 50f;


    protected Transform[] _stars;
    protected float[] _phaseRadian;
    protected float[] _spinOffsetDegree;
    protected float _startTime;

    protected const float alpha = 4;
    protected const float beta = 2;

    public void SetDuration(float newDuration)
    {
        duration = newDuration;
    }

    protected void Awake()
    {
        BuildIfNeeded();
    }

    protected void OnEnable()
    {
            _startTime = GlobalTimeManager.Time;
            BuildIfNeeded();
    }

    protected void BuildIfNeeded()
    {
        if (starPrefab == null)
        {
            return;
        }

        // Successfully built already
        if (_stars != null && _stars.Length == starCount && _stars[0] != null)
        {
            return;
        }

        // Clean old
        if(_stars != null)
        {
            for (int i = 0; i < _stars.Length; i++)
            {
                if (_stars[i] != null)
                {
                    Destroy(_stars[i].gameObject);
                }
            }
        }

        _stars = new Transform[starCount];
        _spinOffsetDegree = new float[starCount];
        _phaseRadian = new float[starCount];

        for (int i = 0; i < starCount; i++)
        {
            var star = Instantiate(starPrefab, transform);
            star.name = $"Star_{i:00}";
            _stars[i] = star.transform;

            _phaseRadian[i] = (Mathf.PI * 2f) * (i / (float)starCount);

            _spinOffsetDegree[i] = GlobalRNG.Instance.NextFloat(0f, 360f);
        }
    }

    protected void LateUpdate()
    {
        if (_stars == null || _stars.Length == 0)
        {
            return;
        }

        float now = GlobalTimeManager.Time;
        float elapsed = now - _startTime;

        if (duration > 0f && elapsed >= duration)
        {
            Destroy(gameObject);
            return;
        }

        float a = alpha * ellipseScale.x;
        float b = beta * ellipseScale.y;

        float omega = (Mathf.PI * 2f) * orbitCyclesPerSecond * elapsed;

        for (int i = 0; i < _stars.Length; i++)
        {
            float angle = _phaseRadian[i] + omega;

            float x = a * Mathf.Cos(angle);
            float y = b * Mathf.Sin(angle);

            Transform star = _stars[i];
            star.localPosition = new Vector3(x, y, 0f);

            // Self rotation
            float spin = _spinOffsetDegree[i] + selfSpinPerDegreePerSecond * elapsed;
            star.localRotation = Quaternion.Euler(0f, 0f, spin);

            // Fake perspective
            float depth = Mathf.InverseLerp(-b, b, y);
            depth = perspectiveCurve.Evaluate(depth);
            if (flipPerspective)
            {
                depth = 1f - depth;
            }

            float scale = Mathf.Lerp(minScale, maxScale, depth);
            star.localScale = Vector3.one * scale;

            // Depth order
            SpriteRenderer renderer = star.GetComponent<SpriteRenderer>();
            if (renderer)
            {
                renderer.sortingOrder = initialDepth + Mathf.RoundToInt(depth * depthScaler);
            }
        }
    }
}
