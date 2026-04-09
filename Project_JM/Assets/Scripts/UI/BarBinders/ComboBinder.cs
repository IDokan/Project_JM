// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/20/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ComboBinder.cs
// Summary: A binder that connects between ComboManager and BarUI to display combo count.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using UnityEngine;

public class ComboBinder : MonoBehaviour
{
    [SerializeField] protected ComboManager boundComboManager;
    protected BarUI _barUI;
    
    protected void OnEnable() => boundComboManager.OnComboUpdated += UpdateComboData;
    protected void OnDisable() => boundComboManager.OnComboUpdated -= UpdateComboData;


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
        _barUI.UpdateValue(timer, boundComboManager.ComboResetTime, $"{comboCount}", Mathf.InverseLerp(50f, 90f, comboCount));
    }
}
