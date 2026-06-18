// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterCombatant.cs
// Summary: A combatant script for action characters.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using MatchEnums;
using UnityEngine;

public interface ICombatant
{
    GemColor[] Colors { get; }
    void Heal(float healPercentage);
    void AddShield(float shieldPercentage);
    void TakeDamage(float damage, AttackContext attackContext);
    void AddBuffCritBonus(float value);
    void AddBuffCritBonus(float value, float duration);
    void AddBuffCritDamage(float value);
}

public class CharacterCombatant : MonoBehaviour, ICombatant
{
    [SerializeField] protected CharacterStatus status;
    [SerializeField] protected GemColor[] colors;

    [SerializeField] protected GameObject hitBurstPrefab;
    [SerializeField] protected Transform woundParentTransform;

    [SerializeField] protected FlashSpriteUsingMaterial flashSprite;
    [SerializeField] private AudioCueSO hurtSfx;
    [SerializeField] private AudioCueSO killVoiceSFX;

    public CharacterStatus Status => status;
    public GemColor[] Colors => colors;

    void Awake()
    {
        if (flashSprite == null)
        {
            flashSprite = GetComponent<FlashSpriteUsingMaterial>();
        }
    }

    public void Heal(float healPercentage)
    {
        status.Heal(healPercentage);
    }

    public void AddShield(float shieldPercentage)
    {
        status.AddShield(shieldPercentage);
    }

    public void TakeDamage(float rawDamage, AttackContext attackContext)
    {
        // Prevent any extra interaction to the enemy already died.
        if (status.IsDead)
        {
            return;
        }

        bool isEnemyAttacker = attackContext.Attacker is MonoBehaviour mb && mb.TryGetComponent<EnemyTag>(out _);
        float damage = rawDamage * (isEnemyAttacker
            ? attackContext.DamageMultiplierManager.GetEnemyMultiplier  // excludes _damageBonus; enemy must not benefit from player skills such as Cleric buff
            : attackContext.DamageMultiplierManager.GetMultiplier);

        bool isCritical = false;
        // Critical hit calculation
        if (attackContext.Attacker is CharacterCombatant attackerObject)
        {
            isCritical = attackerObject.Status.IsCriticalHit();
            if (isCritical)
            {
                damage *= attackerObject.Status.CriticalDamage;
            }
        }

        float colorDamageMultiplier = GemColorUtility.GetGemColorDamageMultiplier(attackContext.Attacker.Colors, attackContext.Target.Colors);
        damage *= colorDamageMultiplier;


        // Multiply its size by 2 when the attacker is an enemy && not blocked
        bool shieldReduced = status.Shield > 0f && damage > 0f;
        int calculatedDamage = Mathf.RoundToInt(Mathf.Min(status.CurrentHP, Mathf.Max(damage - status.Shield, 0f)));
        if (calculatedDamage > 0 && isEnemyAttacker)
        {
            colorDamageMultiplier *= 2f;
        }
        // Spawn Damageui slightly above the origin of attacked target
        DamageUIManager.Instance.SpawnDamage(calculatedDamage, attackContext, isCritical, shieldReduced, colorDamageMultiplier);

        if (!isEnemyAttacker && TryGetComponent<EnemyTag>(out _) && attackContext.Tier.HasValue)
        {
            ScoreManager.Instance?.AddDamageScore(calculatedDamage, attackContext.DamageMultiplierManager.GetEnemyMultiplier, attackContext.Tier.Value);
        }

        status.TakeDamage(damage);

        if (status.IsDead
            && attackContext.Attacker is CharacterCombatant attackerCombatant
            && attackerCombatant.TryGetComponent<AllyTag>(out _))
        {
            attackerCombatant.OnKilledEnemy();
        }

        if (flashSprite != null)
        {
            flashSprite.Flash();
        }

        if (hurtSfx != null)
        {
            AudioManager.Instance.PlayActionSFX(hurtSfx);
        }

        SpawnHitBurstParticle(attackContext);
        SpawnImpactAttachment(attackContext);
    }

    public void AddBuffCritBonus(float value)
    {
        status.AddBuffCritBonus(value);
    }

    public void AddBuffCritBonus(float value, float duration)
    {
        status.AddBuffCritBonus(value, duration);
    }

    public void AddBuffCritDamage(float value)
    {
        status.AddBuffCritDamage(value);
    }

    private void OnKilledEnemy()
    {
        if (killVoiceSFX != null)
        {
            AudioManager.Instance.PlayActionSFX(killVoiceSFX);
        }
    }

    protected void SpawnHitBurstParticle(AttackContext attackContext)
    {
        if (hitBurstPrefab == null)
        {
            return;
        }

        var hitBurst = Instantiate(hitBurstPrefab, attackContext.GetHitPosition(), Quaternion.identity,
            woundParentTransform == null ? gameObject.transform : woundParentTransform);

        GemColor gemColor;
        if (attackContext.Attacker is MonoBehaviour mb
            && mb.TryGetComponent<AllyTag>(out _))
        {
            gemColor = attackContext.Attacker.Colors[0];
        }
        else
        {
            gemColor = GemColor.None;

            // Magnify hit effect of enemy attacks
            hitBurst.transform.localScale *= 2f;
        }

        hitBurst.GetComponent<HitBurst>().SetColor(GemColorUtility.ConvertGemColor(gemColor));
    }

    protected void SpawnImpactAttachment(AttackContext attackContext)
    {
        if (attackContext.ImpactAttachPrefab == null)
        {
            return;
        }

        Transform parent = (woundParentTransform == null ? transform : woundParentTransform);

        Vector3 spawnPos = attackContext.GetHitPosition();

        var go = Instantiate(attackContext.ImpactAttachPrefab, spawnPos, Quaternion.identity, parent);

        // Apply local offset if you want it to sink into the body a bit
        go.transform.localPosition += attackContext.ImpactAttachLocalOffset;
    }
}
