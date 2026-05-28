// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 21/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ClericSFXPlayer.cs
// Summary: Plays Cleric-specific SFX on animation events.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class ClericSFXPlayer : AbstractAnimEventSFXPlayer
{
    public void AnimEvent_ClericHolyJudge() => Play();
}
