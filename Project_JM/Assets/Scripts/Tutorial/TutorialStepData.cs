// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TutorialStepData.cs
// Summary: Abstract base ScriptableObject for a single step in a tutorial sequence.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class TutorialStepData : ScriptableObject
{
    [SerializeField] private List<TutorialSpriteEntry> sprites = new();
    [SerializeField] private List<TutorialSpriteEntry> dialogueSprites = new();
    [FormerlySerializedAs("brightZoneIndex")]
    [SerializeField] private int landscapeBrightZoneIndex = -1;
    [SerializeField] private int portraitBrightZoneIndex = -1;

    public IReadOnlyList<TutorialSpriteEntry> Sprites => sprites;
    public IReadOnlyList<TutorialSpriteEntry> DialogueSprites => dialogueSprites;
    // -1 means no bright zone (full dark backdrop); >= 0 indexes TutorialOverlayUI.brightZones
    public int BrightZoneIndex => Screen.height > Screen.width ? portraitBrightZoneIndex : landscapeBrightZoneIndex;
}
