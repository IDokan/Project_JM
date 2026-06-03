// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialSpriteAnim.cs
// Summary: Animation descriptor for a single tutorial overlay sprite (show, hide, or idle).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using DG.Tweening;
using UnityEngine;

public enum TutorialAnimType
{
    None,
    Fade,
    SlideFrom,
    Scale,
    PulseScale,
    PingPongFade
}

[Serializable]
public class TutorialSpriteAnim
{
    public TutorialAnimType type;
    public float duration = 0.3f;
    public Ease ease = Ease.OutQuad;
    // SlideFrom: starting offset from the sprite's final anchored position
    public Vector2 offset;
    // Scale: from-scale for show/hide; PulseScale: peak scale; PingPongFade: target alpha
    public float targetValue = 1f;
}
