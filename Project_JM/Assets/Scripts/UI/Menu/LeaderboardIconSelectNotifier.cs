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

public class LeaderboardIconSelectNotifier : MonoBehaviour, ISelectHandler, IPointerUpHandler
{
    private int _id;
    private Action<int> _onSelect;
    private Action<int> _onPointerUp;

    public void Init(int id, Action<int> onSelect, Action<int> onPointerUp)
    {
        _id = id;
        _onSelect = onSelect;
        _onPointerUp = onPointerUp;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _onSelect?.Invoke(_id);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!eventData.dragging)
        {
            _onPointerUp?.Invoke(_id);
        }
    }
}
