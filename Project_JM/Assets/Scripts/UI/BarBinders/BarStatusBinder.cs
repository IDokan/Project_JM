// SPDX-License-Identifier: MIT
// Copyright (c) 11/13/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BarUI.cs
// Summary: A binder that connects between CharacterStatus and BarUI to display UI.

using UnityEngine;

[RequireComponent(typeof(BarUI))]
public class BarStatusBinder : MonoBehaviour
{
    [SerializeField] protected CharacterStatus _boundStatus;
    protected BarUI _barUI;

    protected void OnEnable() => _boundStatus.OnHPChanged += UpdateHP;
    protected void OnDisable() => _boundStatus.OnHPChanged -= UpdateHP;

    protected void Awake()
    {
        _barUI = GetComponent<BarUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
        }

        _boundStatus = newStatus;
        _boundStatus.OnHPChanged += UpdateHP;

        UpdateHP(_boundStatus.CurrentHP, _boundStatus.maxHP);
    }

    protected void UpdateHP(float current, float max)
    {
        _barUI.UpdateValue(current, max);
    }
}
