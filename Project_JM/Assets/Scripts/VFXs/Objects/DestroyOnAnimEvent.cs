// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/22/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DestroyOnAnimEvent.cs
// Summary: A script to destroy object by anim event.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class DestroyOnAnimEvent : MonoBehaviour
{   
    public void AnimEvent_Destroy()
    {
        Destroy(gameObject);
    }
}
