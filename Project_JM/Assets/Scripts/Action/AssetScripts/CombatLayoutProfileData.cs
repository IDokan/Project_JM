// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatLayoutProfileData.cs
// Summary: Per-orientation transform targets for combat intro/defeated transitions, so Portrait and Landscape can share the same scene.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

[CreateAssetMenu(fileName = "CombatLayoutProfileData", menuName = "JM/Data/CombatLayoutProfileData")]
public class CombatLayoutProfileData : ScriptableObject
{
    [Header("Intro - Party")]
    [SerializeField] protected Vector3 introPartyStartPosition;
    [SerializeField] protected Vector3 introPartyArrivalPosition;

    [Header("Intro - Board")]
    [SerializeField] protected Vector3 introBoardStartPosition;
    [SerializeField] protected Vector3 introBoardArrivalPosition;

    [Header("Intro - UI")]
    [SerializeField] protected Vector3[] introUIStartPositions;
    [SerializeField] protected Vector3[] introUIArrivalPositions;

    [Header("Intro - Timing")]
    [SerializeField] protected float introPartyMoveDuration;
    [SerializeField] protected float introBoardMoveDelay;
    [SerializeField] protected float introBoardMoveDuration;
    [SerializeField] protected float introUIMoveDelay;
    [SerializeField] protected float introUIMoveDuration;

    [Header("Defeated - Party")]
    [SerializeField] protected float defeatedPartyYOffset;

    [Header("Defeated - Enemy")]
    [SerializeField] protected Vector3 defeatedEnemyArrivalPosition;

    [Header("Defeated - Board")]
    [SerializeField] protected Vector3 defeatedBoardArrivalPosition;

    public Vector3 IntroPartyStartPosition => introPartyStartPosition;
    public Vector3 IntroPartyArrivalPosition => introPartyArrivalPosition;
    public Vector3 IntroBoardStartPosition => introBoardStartPosition;
    public Vector3 IntroBoardArrivalPosition => introBoardArrivalPosition;
    public Vector3[] IntroUIStartPositions => introUIStartPositions;
    public Vector3[] IntroUIArrivalPositions => introUIArrivalPositions;

    public float IntroPartyMoveDuration => introPartyMoveDuration;
    public float IntroBoardMoveDelay => introBoardMoveDelay;
    public float IntroBoardMoveDuration => introBoardMoveDuration;
    public float IntroUIMoveDelay => introUIMoveDelay;
    public float IntroUIMoveDuration => introUIMoveDuration;

    public float DefeatedPartyYOffset => defeatedPartyYOffset;
    public Vector3 DefeatedEnemyArrivalPosition => defeatedEnemyArrivalPosition;
    public Vector3 DefeatedBoardArrivalPosition => defeatedBoardArrivalPosition;
}
