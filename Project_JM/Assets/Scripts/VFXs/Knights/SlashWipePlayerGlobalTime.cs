// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlashWipePlayerGlobalTime.cs
// Summary: SlashWipePlayer variant that uses GlobalTimeManager.DeltaTime so it pauses with the game.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

public class SlashWipePlayerGlobalTime : SlashWipePlayer
{
    protected override float DeltaTime => GlobalTimeManager.DeltaTime;
}
