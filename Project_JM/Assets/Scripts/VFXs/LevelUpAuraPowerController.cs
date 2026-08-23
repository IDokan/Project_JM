// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LevelUpAuraPowerController.cs
// Summary: Animates the _Power property of a level-up aura's material
//          directly on the shared material asset: starts high on Awake, dips
//          low shortly after, then eases back up to the starting value.
//          Shared across all classes' level-up VFX (Knight/Mage/Cleric/
//          Bowman); assumes only one instance of a given class's aura plays
//          at a time, since it mutates the shared material rather than using
//          a per-renderer MaterialPropertyBlock.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class LevelUpAuraPowerController : MonoBehaviour
{
    [Header("Shader property name")]
    [SerializeField] protected string powerProperty = "_Power";

    [Header("Power values")]
    [SerializeField] protected float startPower = 10f;
    [SerializeField] protected float dipPower = 3.2f;

    [Header("Timing in seconds")]
    [SerializeField] protected float dipTime = 0.5f;
    [SerializeField] protected float totalDuration = 2f;

    protected Material _material;
    protected int _powerID;
    protected float _t;

    protected void Awake()
    {
        _material = GetComponent<Renderer>().sharedMaterial;
        _powerID = Shader.PropertyToID(powerProperty);
        ApplyPower(startPower);
    }

    protected void Update()
    {
        if (_t >= totalDuration)
        {
            return;
        }

        _t += Time.deltaTime;
        ApplyPower(EvaluatePower(_t));
    }

    protected float EvaluatePower(float elapsed)
    {
        if (elapsed <= dipTime)
        {
            float normalized = dipTime > 0f ? elapsed / dipTime : 1f;
            return Mathf.Lerp(startPower, dipPower, normalized);
        }

        float riseNormalized = Mathf.Clamp01((elapsed - dipTime) / (totalDuration - dipTime));
        return Mathf.Lerp(dipPower, startPower, Mathf.SmoothStep(0f, 1f, riseNormalized));
    }

    protected void ApplyPower(float power)
    {
        _material.SetFloat(_powerID, power);
    }
}
