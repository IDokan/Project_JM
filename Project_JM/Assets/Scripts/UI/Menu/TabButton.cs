// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TabButton.cs
// Summary: Shows this tab's panel through TabGroup whenever its Toggle is switched on.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private CanvasGroup panel;
    [SerializeField] private TabGroup tabGroup;

    protected void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    protected void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    protected void Start()
    {
        if (toggle.isOn)
        {
            tabGroup.ShowOnly(panel);
        }
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (isOn)
        {
            tabGroup.ShowOnly(panel);
        }
    }
}
