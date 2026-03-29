// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/13/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BarUI.cs
// Summary: A binder that connects between CharacterStatus and BarUI to display UI.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class BarStatusBinder : MonoBehaviour
{
    [SerializeField] protected CharacterStatus boundStatus;
    [SerializeField] protected BarUIBase hPbarUI;
    [SerializeField] protected BarUIBase shieldbarUI;

    protected void OnEnable()
    {
        if (boundStatus != null)
        {
            boundStatus.OnHPChanged += UpdateHP;
            boundStatus.OnShieldChanged += UpdateShield;
        }
    }

    protected void OnDisable()
    {
        if (boundStatus != null)
        {
            boundStatus.OnHPChanged -= UpdateHP;
            boundStatus.OnShieldChanged -= UpdateShield;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (boundStatus != null)
        {
            UpdateHP(boundStatus.CurrentHP, boundStatus.maxHP);
            UpdateShield(0, boundStatus.maxHP);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void BindNewStatus(CharacterStatus newStatus)
    {
        if (newStatus == boundStatus) return;

        if (boundStatus != null)
        {
            boundStatus.OnHPChanged -= UpdateHP;
            boundStatus.OnShieldChanged -= UpdateShield;
        }

        boundStatus = newStatus;
        boundStatus.OnHPChanged += UpdateHP;
        boundStatus.OnShieldChanged += UpdateShield;

        UpdateHP(boundStatus.CurrentHP, boundStatus.maxHP);
        UpdateShield(0, boundStatus.maxHP);
    }

    protected void UpdateHP(float current, float max)
    {
        hPbarUI.UpdateValue(current, max);
    }

    protected void UpdateShield(float shieldAmount, float max)
    {
        if (shieldAmount > boundStatus.maxHP)
        {
            shieldbarUI.UpdateValue(shieldAmount, boundStatus.maxHP, false);
        }
        else
        {
            shieldbarUI.UpdateValue(shieldAmount, max, false);
        }
    }
}
