// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 25/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardNavigation.cs
// Summary: Manages explicit gamepad/keyboard navigation between the title button,
//          scope button, and leaderboard rows. Call SetupNavigation after rows are built.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardNavigation : MonoBehaviour
{
    [SerializeField] private Button titleButton;
    [SerializeField] private Button scopeButton;
    [SerializeField] private RectTransform content;

    // TEMP: remove once the leaderboard data script drives SetupNavigation at runtime
    private void Start()
    {
        List<LeaderboardRow> rows = new List<LeaderboardRow>(content.GetComponentsInChildren<LeaderboardRow>());
        SetupNavigation(rows);
    }

    public void SetupNavigation(List<LeaderboardRow> rows)
    {
        Navigation titleNav = new Navigation();
        titleNav.mode = Navigation.Mode.Explicit;
        titleNav.selectOnLeft = scopeButton;
        titleNav.selectOnRight = scopeButton;
        titleButton.navigation = titleNav;

        Navigation scopeNav = new Navigation();
        scopeNav.mode = Navigation.Mode.Explicit;
        scopeNav.selectOnUp = titleButton;
        scopeNav.selectOnDown = titleButton;
        scopeButton.navigation = scopeNav;

        for (int i = 0; i < rows.Count; i++)
        {
            Button button = rows[i].GetComponent<Button>();
            Navigation nav = new Navigation();
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnUp = i > 0 ? rows[i - 1].GetComponent<Button>() : null;
            nav.selectOnDown = i < rows.Count - 1 ? rows[i + 1].GetComponent<Button>() : null;
            nav.selectOnLeft = scopeButton;
            nav.selectOnRight = scopeButton;
            button.navigation = nav;
        }

        if (rows.Count > 0)
        {
            OnRowSelected(rows[0].GetComponent<Button>());
        }
    }

    public void OnRowSelected(Button rowButton)
    {
        Navigation titleNav = titleButton.navigation;
        titleNav.selectOnUp = rowButton;
        titleNav.selectOnDown = rowButton;
        titleButton.navigation = titleNav;

        Navigation scopeNav = scopeButton.navigation;
        scopeNav.selectOnLeft = rowButton;
        scopeNav.selectOnRight = rowButton;
        scopeButton.navigation = scopeNav;
    }
}
