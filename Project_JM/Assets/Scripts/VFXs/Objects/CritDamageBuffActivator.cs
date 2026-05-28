// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CritDamageBuffActivator.cs
// Summary: Activates a target object when crit damage buff starts and
//          deactivates it when the buff is cleared.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class CritDamageBuffActivator : MonoBehaviour
{
    [SerializeField] private CharacterStatus characterStatus;
    [SerializeField] private GameObject target;

    private void Start()
    {
        target.SetActive(false);
    }

    protected void OnEnable()
    {
        characterStatus.OnCritDamageBuffStarted += OnBuffStarted;
        characterStatus.OnCritDamageBuffStopped += OnBuffStopped;
    }

    protected void OnDisable()
    {
        characterStatus.OnCritDamageBuffStarted -= OnBuffStarted;
        characterStatus.OnCritDamageBuffStopped -= OnBuffStopped;
    }

    private void OnBuffStarted() => target.SetActive(true);
    private void OnBuffStopped() => target.SetActive(false);
}
