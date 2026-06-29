// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 29/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SelectNotifier.cs
// Summary: Fires a callback with a bound ID when this GameObject receives UI focus.
//          Add to the icon button prefab in the editor; call Init() after instantiation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectNotifier : MonoBehaviour, ISelectHandler
{
    private int _id;
    private Action<int> _callback;

    public void Init(int id, Action<int> callback)
    {
        _id = id;
        _callback = callback;
    }

    public void OnSelect(BaseEventData eventData)
    {
        _callback?.Invoke(_id);
    }
}
