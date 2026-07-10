// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialSpriteEntry.cs
// Summary: Per-sprite configuration for a tutorial overlay: per-orientation anchor/position/scale, draw order, and animations.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class TutorialSpriteEntry
{
    private static readonly Vector2 CenterAnchor = new Vector2(0.5f, 0.5f);

    public Sprite sprite;
    [FormerlySerializedAs("anchoredPosition")]
    public Vector2 landscapeAnchoredPosition;
    [FormerlySerializedAs("scale")]
    public Vector2 landscapeScale = Vector2.one;
    public Vector2 portraitAnchoredPosition;
    public Vector2 portraitScale = Vector2.one;

    // Normalized (0-1) anchor point; defaults keep every existing entry center-anchored.
    // Set away from center only when a sprite must stay glued to a screen edge/corner
    // across devices of varying aspect ratio, since anchoredPosition alone cannot do that.
    public Vector2 landscapeAnchorMin = CenterAnchor;
    public Vector2 landscapeAnchorMax = CenterAnchor;
    public Vector2 portraitAnchorMin = CenterAnchor;
    public Vector2 portraitAnchorMax = CenterAnchor;

    public TutorialSpriteAnim showAnim;
    public TutorialSpriteAnim hideAnim;
    public TutorialSpriteAnim idleAnim;

    public Vector2 AnchoredPosition => Screen.height > Screen.width ? portraitAnchoredPosition : landscapeAnchoredPosition;
    public Vector2 Scale => Screen.height > Screen.width ? portraitScale : landscapeScale;
    public Vector2 AnchorMin => Screen.height > Screen.width ? portraitAnchorMin : landscapeAnchorMin;
    public Vector2 AnchorMax => Screen.height > Screen.width ? portraitAnchorMax : landscapeAnchorMax;
}
