// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialSpriteEntry.cs
// Summary: Per-sprite configuration for a tutorial overlay: position, draw order, and animations.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;

[Serializable]
public class TutorialSpriteEntry
{
    public Sprite sprite;
    public Vector2 anchoredPosition;
    public Vector2 scale = Vector2.one;
    public TutorialSpriteAnim showAnim;
    public TutorialSpriteAnim hideAnim;
    public TutorialSpriteAnim idleAnim;
}
