// SPDX-License-Identifier: MIT
// Copyright (c) 02/09/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FlyToTarget.cs
// Summary: A script to move object to the target transform.

using UnityEngine;
using MatchEnums;

public class FlyToTarget : MonoBehaviour
{
    [SerializeField] protected bool velocityOriented = true;
    [SerializeField] protected Transform hitTransform;
    [SerializeField] protected LayerMask hitMask;
    [SerializeField] protected float _speed = 0f;
    [SerializeField] private float spriteFacingAngleOffset = 0f;

    [Header("AfterImpact")]
    [SerializeField] protected BrokenEggController brokenEggController;

    protected AttackExecutor _clericAttackExecutor;
    protected Transform _target;
    protected bool _initialized;

    protected Rigidbody2D _rigidbody2D;
    protected Collider2D _collider2D;

    void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _collider2D = GetComponent<Collider2D>();

        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _rigidbody2D.interpolation = RigidbodyInterpolation2D.Interpolate;

        _collider2D.isTrigger = true;
    }

    public void Init(AttackExecutor attackExecutor, Transform target)
    {
        _clericAttackExecutor = attackExecutor;
        _target = target;
        _initialized = true;
    }

    void FixedUpdate()
    {
        if (_initialized == false)
        {
            return;
        }

        if (_target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (velocityOriented)
        {
            Vector2 current = _rigidbody2D.position;
            Vector2 targetPos = _target.position;

            Vector2 toTarget = targetPos - current;
            float distSqr = toTarget.sqrMagnitude;

            // If we are basically at the target, don't rotate (prevents jitter).
            if (distSqr > 0.0001f)
            {
                Vector2 dir = toTarget.normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + spriteFacingAngleOffset;
                _rigidbody2D.MoveRotation(angle);
            }
        }

        Vector2 next = Vector2.MoveTowards(_rigidbody2D.position, _target.position, _speed * Time.fixedDeltaTime);
        _rigidbody2D.MovePosition(next);
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        // Layer filter
        if (((1 << other.gameObject.layer) & hitMask) == 0 &&
            other.gameObject.TryGetComponent<EnemyTag>(out _) == false)
        {
            return;
        }

        if (_clericAttackExecutor != null)
        {
            _clericAttackExecutor.HandleHitWithTransform(MatchTier.Three, hitTransform);
        }

        BrokenEggController brokenEgg = Instantiate(brokenEggController);
        brokenEgg.Init(transform, _speed);

        Destroy(gameObject);
    }
}
