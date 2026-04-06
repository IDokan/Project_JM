// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 20/11/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageUIManager.cs
// Summary: Manages spawning and pooling of floating damage number UI elements.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class DamageUIManager : MonoBehaviour
{
    public static DamageUIManager Instance;

    [SerializeField] protected DamageUI damagePrefab;

    protected void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public DamageUI SpawnDamage(int amount, AttackContext context, bool isCritical, float sizeMultiplier = 1f)
    {
        var dmg = Instantiate(damagePrefab, transform);
        dmg.Show(amount, context, isCritical, sizeMultiplier);

        return dmg;
    }
}
