// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: FrameRateManager.cs
// Summary: Configures target frame rate and vSync per platform at startup. Mobile gets an
//          explicit frame rate target since Android/iOS otherwise default to a lower cap and
//          ignore Application.targetFrameRate while vSyncCount is nonzero; PC keeps native
//          monitor-refresh vSync since its vSyncCount directly controls tearing.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class FrameRateManager : MonoBehaviour
{
    [SerializeField] protected int mobileTargetFrameRate = 60;

    protected void Awake()
    {
#if UNITY_ANDROID || UNITY_IOS
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = mobileTargetFrameRate;
#endif
    }
}
