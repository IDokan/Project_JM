// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardPanelButton.cs
// Summary: Notifies LeaderboardNavigation when a right-panel button (scope, page-up, page-down)
//          receives focus so rows can update their right navigation target.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LeaderboardPanelButton : MonoBehaviour, ISelectHandler
{
    [SerializeField] private LeaderboardNavigation navigation;

    public void OnSelect(BaseEventData eventData)
    {
        navigation.OnRightPanelButtonSelected(GetComponent<Button>());
    }
}
