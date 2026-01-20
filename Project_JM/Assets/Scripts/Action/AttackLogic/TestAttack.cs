// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TestAttack.cs
// Summary: An attack logic only for test purpose.

using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "JM/Combat/AttackLogic/Ally/Test Attack")]
public class TestAttack : AttackLogic
{
    [SerializeField] protected int baseDamage = 3;


    public override IEnumerator Execute(AttackContext ctx)
    {
        ctx.Target?.TakeDamage(baseDamage, ctx);
        yield break;
    }

    public override Vector3 GetTargetMotionOffset()
    {
        return new Vector3(1.5f, 0f, 0f);
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
