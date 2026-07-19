// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 06/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemColorIconData.cs
// Summary: A single lookup of icon sprites by gem color, shared across UI that needs one.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using UnityEngine;

[CreateAssetMenu(fileName = "GemColorIconData", menuName = "JM/Data/GemColorIconData")]
public class GemColorIconData : ScriptableObject
{
    [SerializeField] protected Sprite redIcon;
    [SerializeField] protected Sprite greenIcon;
    [SerializeField] protected Sprite blueIcon;
    [SerializeField] protected Sprite yellowIcon;

    public Sprite GetIconByColor(GemColor color)
    {
        switch (color)
        {
            case GemColor.Red: return redIcon;
            case GemColor.Green: return greenIcon;
            case GemColor.Blue: return blueIcon;
            case GemColor.Yellow: return yellowIcon;
            default: return null;
        }
    }
}
