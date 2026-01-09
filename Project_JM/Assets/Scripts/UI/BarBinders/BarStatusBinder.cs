// SPDX-License-Identifier: MIT
// Copyright (c) 11/13/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BarUI.cs
// Summary: A binder that connects between CharacterStatus and BarUI to display UI.

using UnityEngine;

public class BarStatusBinder : MonoBehaviour
{
    [SerializeField] protected CharacterStatus _boundStatus;
    [SerializeField] protected BarUIBase _HPbarUI;
    [SerializeField] protected BarUIBase _ShieldbarUI;

    protected void OnEnable()
    {
        _boundStatus.OnHPChanged += UpdateHP;
        _boundStatus.OnShieldChanged += UpdateShield;
    }

    protected void OnDisable()
    {
        _boundStatus.OnHPChanged -= UpdateHP;
        _boundStatus.OnShieldChanged -= UpdateShield;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateHP(_boundStatus.CurrentHP, _boundStatus.maxHP);
        UpdateShield(0, _boundStatus.maxHP);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void BindNewStatus(CharacterStatus newStatus)
    {
        if (newStatus == _boundStatus) return;

        if (_boundStatus != null)
        {
            _boundStatus.OnHPChanged -= UpdateHP;
            _boundStatus.OnShieldChanged -= UpdateShield;
        }

        _boundStatus = newStatus;
        _boundStatus.OnHPChanged += UpdateHP;
        _boundStatus.OnShieldChanged += UpdateShield;

        UpdateHP(_boundStatus.CurrentHP, _boundStatus.maxHP);
        UpdateShield(0, _boundStatus.maxHP);
    }

    protected void UpdateHP(float current, float max)
    {
        _HPbarUI.UpdateValue(current, max);
    }

    protected void UpdateShield(float shieldAmount, float max)
    {
        if (shieldAmount > _boundStatus.maxHP)
        {
            _ShieldbarUI.UpdateValue(shieldAmount, _boundStatus.maxHP, false);
        }
        else
        {
            _ShieldbarUI.UpdateValue(shieldAmount, max, false);
        }
    }
}
