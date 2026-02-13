// SPDX-License-Identifier: MIT
// Copyright (c) 02/12/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BeamParamController.cs
// Summary: A script to control parameter value of beam material.

using UnityEngine;

public class BeamParamController : MonoBehaviour
{
    [Header("Shader properly name")]
    [SerializeField] protected string powerProperty = "_Power";

    [Header("Curve time in seconds")]
    [SerializeField] protected float duration = 1f;
    [SerializeField] protected bool loop = true;

    [SerializeField] protected AnimationCurve powerCurve = AnimationCurve.EaseInOut(0f, 10f, 1f, 0f);

    protected Renderer _renderer;
    protected MaterialPropertyBlock _materialPropertyBlock;
    protected int _powerID;
    protected float _t;


    protected void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
        _powerID = Shader.PropertyToID(powerProperty);
    }

    // Update is called once per frame
    void Update()
    {
        if (duration <= 0f)
        {
            return;
        }

        _t += Time.deltaTime;
        float normalized = _t / duration;

        if (loop)
        {
            normalized = Mathf.Repeat(normalized, 1f);
        }
        else
        {
            normalized = Mathf.Clamp01(normalized);
        }

        float power = powerCurve.Evaluate(normalized);

        _renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(_powerID, power);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
