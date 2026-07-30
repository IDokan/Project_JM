// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 29/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardIconSelectNotifier.cs
// Summary: Fires a callback with a bound ID when this GameObject receives UI focus.
//          Add to the icon button prefab in the editor; call Init() after instantiation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class LeaderboardIconSelectNotifier : MonoBehaviour, ISelectHandler
{
    private int _id;
    private Action<int> _onSelect;

    public void Init(int id, Action<int> onSelect)
    {
        _id = id;
        _onSelect = onSelect;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _onSelect?.Invoke(_id);
    }
}
