// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StatsMenu.cs
// Summary: Stats overlay menu opened from the main menu; its internal tabs are switched by
//          TabButton/TabGroup within the panel.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine.UI;

public class StatsMenu : Menu
{
    public override void Show(Selectable returnTo)
    {
        Selectable firstSelectable = GetFirstSelectable();
        if (firstSelectable is Toggle firstToggle)
        {
            firstToggle.isOn = true;
        }

        base.Show(returnTo);
    }
}
