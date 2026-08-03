// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardIconGroup.cs
// Summary: Displays a reward's icons (2 to 4) over a reward button, showing
//          only as many slots as the bound reward provides; unused slots
//          are hidden and the row re-centers via the container's own
//          HorizontalLayoutGroup + ContentSizeFitter.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardIconGroup : MonoBehaviour
{
    [SerializeField] protected Image[] iconSlots;

    public void SetIcons(IReadOnlyList<Sprite> sprites)
    {
        for (int i = 0; i < iconSlots.Length; i++)
        {
            bool hasSprite = sprites != null && i < sprites.Count;
            iconSlots[i].gameObject.SetActive(hasSprite);

            if (hasSprite)
            {
                iconSlots[i].sprite = sprites[i];
                iconSlots[i].SetNativeSize();
            }
        }
    }
}
