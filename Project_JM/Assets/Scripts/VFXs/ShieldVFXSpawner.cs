// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShieldVFXSpawner.cs
// Summary: A script to spawn shield VFX.

using UnityEngine;

public class ShieldVFXSpawner : MonoBehaviour
{
    [SerializeField] protected ShieldFadeOutPlayer shieldPrefab;
    [SerializeField] protected Transform parentTransform;
    [SerializeField] protected Vector3 localOffset;
    [SerializeField] protected float baseRotatingDeg = 0f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AnimEvent_Shield()
    {
        if (shieldPrefab == null)
        {
            return;
        }

        Transform t = parentTransform ? parentTransform : transform;

        Vector3 pos = t.position + t.TransformVector(localOffset);

        // Rotate slash 
        Quaternion rot = Quaternion.Euler(0f, 0f, baseRotatingDeg);

        var vfx = Instantiate(shieldPrefab, pos, rot);
    }
}
