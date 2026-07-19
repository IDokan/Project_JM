// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardScaleOrientationSetter.cs
// Summary: Resolves the gem board's scale by screen orientation.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class BoardScaleOrientationSetter : MonoBehaviour
{
    [SerializeField] protected Transform boardTransform;

    [SerializeField] protected Vector3 landscapeBoardScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] protected Vector3 portraitBoardScale = new Vector3(1f, 1f, 1f);

    protected void Awake()
    {
        bool isPortrait = Screen.height > Screen.width;
        boardTransform.localScale = isPortrait ? portraitBoardScale : landscapeBoardScale;
    }
}
