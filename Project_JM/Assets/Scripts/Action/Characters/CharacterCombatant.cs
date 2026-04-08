// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterCombatant.cs
// Summary: A combatant script for action characters.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
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
        float damage = rawDamage * attackContext.DamageMultiplierManager.GetMultiplier;

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
        if (calculatedDamage > 0 &&
            attackContext.Attacker is MonoBehaviour attackerMB &&
            attackerMB.TryGetComponent<EnemyTag>(out _))
        {
            colorDamageMultiplier *= 2f;
        }
        // Spawn Damageui slightly above the origin of attacked target
        DamageUIManager.Instance.SpawnDamage(calculatedDamage, attackContext, isCritical, shieldReduced, colorDamageMultiplier);

        status.TakeDamage(damage);

        if (flashSprite != null)
        {
            flashSprite.Flash();
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

    protected void SpawnHitBurstParticle(AttackContext attackContext)
    {
        if (hitBurstPrefab == null || attackContext.HitTransform == null)
        {
            return;
        }

        var hitBurst = Instantiate(hitBurstPrefab, attackContext.HitTransform.position, attackContext.HitTransform.rotation,
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
        if (attackContext.ImpactAttachPrefab == null || attackContext.HitTransform == null)
        {
            return;
        }

        Transform parent = (woundParentTransform == null ? transform : woundParentTransform);

        Vector3 spawnPos = attackContext.HitTransform.position;

        var go = Instantiate(attackContext.ImpactAttachPrefab, spawnPos, Quaternion.identity, parent);

        // Apply local offset if you want it to sink into the body a bit
        go.transform.localPosition += attackContext.ImpactAttachLocalOffset;
    }
}
