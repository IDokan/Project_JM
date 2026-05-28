// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 19/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BowmanSFXPlayer.cs
// Summary: Plays Bowman-specific SFX on animation events.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class BowmanSFXPlayer : AbstractAnimEventSFXPlayer
{
    public void AnimEvent_PlayArrowShootSFX() => Play();
}
