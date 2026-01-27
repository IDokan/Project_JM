// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterCombatant.cs
// Summary: A combatant script for action characters.

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
    [SerializeField] protected CharacterStatus _status;
    [SerializeField] protected GemColor[] _colors;

    [SerializeField] protected GameObject hitBurstPrefab;
    [SerializeField] protected Transform woundParentTransform;

    public CharacterStatus Status => _status;
    public GemColor[] Colors => _colors;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Heal(float healPercentage)
    {
        _status.Heal(healPercentage);
    }

    public void AddShield(float shieldPercentage)
    {
        _status.AddShield(shieldPercentage);
    }

    public void TakeDamage(float rawDamage, AttackContext attackContext)
    {
        float damage = rawDamage * attackContext.DamageMultiplierManager.GetMultiplier;

        // Critical hit calculation
        if (attackContext.Attacker is CharacterCombatant attackerObject)
        {
            if (attackerObject.Status.IsCriticalHit())
            {
                damage *= attackerObject.Status.CriticalDamage;
            }
        }

        damage *= GemColorUtility.GetGemColorDamageMultiplier(attackContext.Attacker.Colors, attackContext.Target.Colors);

        _status.TakeDamage(damage);

        SpawnHitBurstParticle(attackContext);
        SpawnImpactAttachment(attackContext);
    }

    public void AddBuffCritBonus(float value)
    {
        _status.AddBuffCritBonus(value);
    }

    public void AddBuffCritBonus(float value, float duration)
    {
        _status.AddBuffCritBonus(value, duration);
    }

    public void AddBuffCritDamage(float value)
    {
        _status.AddBuffCritDamage(value);
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
        if (attackContext.Attacker.Colors.Length <= 1)
        {
            gemColor = attackContext.Attacker.Colors[0];
        }
        else
        {
            gemColor = attackContext.Attacker.Colors[GlobalRNG.Instance.NextInt(attackContext.Attacker.Colors.Length)];
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
