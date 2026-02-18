// SPDX-License-Identifier: MIT
// Copyright (c) 01/30/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IceShardProjectile.cs
// Summary: A script for ice shard projectile. It manages movement of the ice shard and damage if overlapped.

using MatchEnums;
using UnityEngine;

public class IceShardProjectile : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D projectileRigidbody;
    [SerializeField] protected LayerMask hitMask;
    [SerializeField] protected Transform hitTransform;

    protected AttackExecutor _mageAttackExecutor;
    protected Vector2 _direction;
    protected float _speed;
    protected float _dieAt;
    protected bool _hitOnce = false;

    public void Init(AttackExecutor attackExecutor, Vector2 direction, float speed, float lifeTime)
    {
        _mageAttackExecutor = attackExecutor;

        _direction = direction;
        _speed = speed;
        _dieAt = lifeTime + Time.time;

        if (projectileRigidbody == null)
        {
            projectileRigidbody = GetComponent<Rigidbody2D>();
        }
        if (projectileRigidbody != null)
        {
            // Consistent & cheap option.
            projectileRigidbody.bodyType = RigidbodyType2D.Kinematic;
            projectileRigidbody.gravityScale = 0f;
            projectileRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            projectileRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

    }

    protected void FixedUpdate()
    {
        Vector2 next = projectileRigidbody.position + (_direction * _speed * Time.fixedDeltaTime);
        projectileRigidbody.MovePosition(next);

        // If missed
        if (Time.time >= _dieAt)
        {
            Destroy(gameObject);
        }
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitOnce)
        {
            return;
        }

        // Layer filter
        if(((1 << other.gameObject.layer) & hitMask) == 0 &&
            other.gameObject.TryGetComponent<EnemyTag>(out _) == false)
        {
            return;
        }

        _hitOnce = true;

        _mageAttackExecutor.HandleHitWithTransform(MatchTier.Three, hitTransform);

        Destroy(gameObject);
    }
}
