// SPDX-License-Identifier: MIT
// Copyright (c) 11/20/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ComboBinder.cs
// Summary: A binder that connects between ComboManager and BarUI to display combo count.


using UnityEngine;

public class ComboBinder : MonoBehaviour
{
    [SerializeField] protected ComboManager _boundComboManager;
    protected BarUI _barUI;
    
    protected void OnEnable() => _boundComboManager.OnComboUpdated += UpdateComboData;
    protected void OnDisable() => _boundComboManager.OnComboUpdated -= UpdateComboData;


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

    void UpdateComboData(int comboCount, float timer)
    {
        _barUI.UpdateValue(timer, _boundComboManager.ComboResetTime, $"{comboCount}");
    }
}
