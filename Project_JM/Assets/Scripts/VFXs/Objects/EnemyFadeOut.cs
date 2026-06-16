// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 16/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyFadeOut.cs
// Summary: FadeOut variant that advances using GlobalTimeManager so it respects game pause and time scale.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

public class EnemyFadeOut : FadeOut
{
    protected override float DeltaTime => GlobalTimeManager.DeltaTime;
}
