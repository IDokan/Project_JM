// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 19/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AbstractAnimEventSFXPlayer.cs
// Summary: An abstract base script to play a SFX on animation events.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public abstract class AbstractAnimEventSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioCueSO sfx;

    protected void Play() => AudioManager.Instance.PlayActionSFX(sfx);
}
