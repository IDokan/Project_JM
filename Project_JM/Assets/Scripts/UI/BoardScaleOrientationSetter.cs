// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 08/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardScaleOrientationSetter.cs
// Summary: Resolves the gem board's scale by screen orientation and compensates this
//          Canvas's own scale so its content keeps a consistent apparent size.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class BoardScaleOrientationSetter : MonoBehaviour
{
    [SerializeField] protected Transform boardTransform;
    [SerializeField] protected RectTransform canvasTransform;

    [SerializeField] protected Vector3 landscapeBoardScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] protected Vector3 portraitBoardScale = new Vector3(1f, 1f, 1f);
    [SerializeField] protected float canvasBaseScale = 1f;

    protected void Awake()
    {
        bool isPortrait = Screen.height > Screen.width;
        Vector3 boardScale = isPortrait ? portraitBoardScale : landscapeBoardScale;
        boardTransform.localScale = boardScale;

        Vector3 canvasScale = canvasTransform.localScale;
        canvasScale.x = canvasScale.y = canvasBaseScale / boardScale.x;
        canvasTransform.localScale = canvasScale;
    }
}
