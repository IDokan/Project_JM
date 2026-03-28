// SPDX-License-Identifier: MIT
// Copyright (c) 02/22/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DestroyOnAnimEvent.cs
// Summary: A script to destroy object by anim event.

using UnityEngine;

public class DestroyOnAnimEvent : MonoBehaviour
{   
    public void AnimEvent_Destroy()
    {
        Destroy(gameObject);
    }
}
