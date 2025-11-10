// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TestAttack.cs
// Summary: A attack logic only for test purpose.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Test Attack")]
public class TestAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 3;
    [SerializeField] private float tierScale = 1.0f;

    public override IEnumerator Execute(AttackContext ctx)
    {
        Debug.Log($"{ctx.Match.Color} casted {ctx.Match.Tier}tier TestAttack");
        int tier = (int)ctx.Match.Tier; // 3/4/5
        int dmg = Mathf.RoundToInt(baseDamage * (1f + (tier - 3) * tierScale));
        ctx.Target?.ReceiveDamage(dmg);
        yield break;
    }

    /*
     *  // VFX+SFX wrapper (no logic, just juice)
 [CreateAssetMenu(menuName = "JM/Combat/Effects/Play VFX+SFX")]
public class VfxSfxEffect : AttackEffect
{
    [SerializeField] private GameObject vfxPrefab;
    [SerializeField] private string sfxKey;

    public override System.Collections.IEnumerator Execute(AttackContext ctx)
    {
        if (vfxPrefab && ctx.VfxAnchor)
            GameObject.Instantiate(vfxPrefab, ctx.VfxAnchor.position, Quaternion.identity);
        if (!string.IsNullOrEmpty(sfxKey))
            Sfx.Play(sfxKey); // your audio helper
        yield break;
    }
}
     */

    /*
     *      // Composite (chain multiple effects)
[CreateAssetMenu(menuName = "JM/Combat/Effects/Composite")]
public class CompositeEffect : AttackEffect
{
    [SerializeField] private AttackEffect[] effects;
    public override System.Collections.IEnumerator Execute(AttackContext ctx)
    {
        foreach (var e in effects)
            if (e) yield return ctx.Attacker.RunEffect(e, ctx); // helper below
    }
}
     */
}
