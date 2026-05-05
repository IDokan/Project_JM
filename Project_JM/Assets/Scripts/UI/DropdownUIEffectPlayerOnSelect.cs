// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DropdownUIEffectPlayerOnSelect.cs
// Summary: Extends UIEffectPlayerOnSelect to play press SFX when the dropdown value changes.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TMPro;
using UnityEngine;

public class DropdownUIEffectPlayerOnSelect : UIEffectPlayerOnSelect
{
    private TMP_Dropdown _dropdown;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        if (_dropdown != null)
        {
            _dropdown.onValueChanged.AddListener(OnValueChanged);
        }
    }

    private void OnDisable()
    {
        if (_dropdown != null)
        {
            _dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

    private void OnValueChanged(int value)
    {
        AudioManager.Instance.PlayUISFX(pressSFX);
    }
}
