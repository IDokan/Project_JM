// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 15/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ArrowProjectile.cs
// Summary: Collision-based arrow projectile. Travels at a constant speed in a fixed direction
//          and triggers AttackExecutor.HandleHitWithTransform on first contact with an enemy.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using MatchEnums;
using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [SerializeField] private Rigidbody2D projectileRigidbody;
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private Transform hitTransform;

    private AttackExecutor _bowmanAttackExecutor;
    private MatchTier _tier;
    private Vector2 _direction;
    private float _speed;
    private float _dieAt;
    private bool _hitOnce;

    public void Init(AttackExecutor attackExecutor, MatchTier tier, Vector2 direction, float speed, float lifeTime)
    {
        _bowmanAttackExecutor = attackExecutor;
        _tier = tier;
        _direction = direction;
        _speed = speed;
        _dieAt = Time.time + lifeTime;

        if (projectileRigidbody == null)
        {
            projectileRigidbody = GetComponent<Rigidbody2D>();
        }
        if (projectileRigidbody != null)
        {
            projectileRigidbody.bodyType = RigidbodyType2D.Kinematic;
            projectileRigidbody.gravityScale = 0f;
            projectileRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            projectileRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void FixedUpdate()
    {
        Vector2 next = projectileRigidbody.position + (_direction * _speed * Time.fixedDeltaTime);
        projectileRigidbody.MovePosition(next);

        if (Time.time >= _dieAt)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_hitOnce)
        {
            return;
        }

        if (((1 << other.gameObject.layer) & hitMask) == 0 &&
            other.gameObject.TryGetComponent<EnemyTag>(out _) == false)
        {
            return;
        }

        _hitOnce = true;

        Transform resolvedHitTransform = hitTransform != null ? hitTransform : transform;
        _bowmanAttackExecutor.HandleHitWithTransform(_tier, resolvedHitTransform);

        Destroy(gameObject);
    }
}
