// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TabGroup.cs
// Summary: Tracks which tab panel is currently visible and switches it on request.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class TabGroup : MonoBehaviour
{
    private CanvasGroup _activePanel;

    public void ShowOnly(CanvasGroup panel)
    {
        if (panel == _activePanel)
        {
            return;
        }

        if (_activePanel != null)
        {
            SetPanelVisible(_activePanel, false);
        }

        SetPanelVisible(panel, true);
        _activePanel = panel;
    }

    private void SetPanelVisible(CanvasGroup panel, bool visible)
    {
        panel.alpha = visible ? 1f : 0f;
        panel.interactable = visible;
        panel.blocksRaycasts = visible;
    }
}
