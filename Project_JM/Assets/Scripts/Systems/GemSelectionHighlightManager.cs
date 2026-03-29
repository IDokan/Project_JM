// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/20/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemSelectionHighlightManager.cs
// Summary: A script to manage GemSelectionHighlight object.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class GemSelectionHighlightManager : MonoBehaviour
{
    [SerializeField] protected GemSelectionHighlight selectionHighlightPrefab;

    [SerializeField] protected BoardManager boardManager;

    protected GemSelectionHighlight _highlightObject = null;

    protected void Awake()
    {
        if (selectionHighlightPrefab == null)
        {
            Debug.LogWarning("A prefab to highlight where is selected is NULL", this);
        }

        if (boardManager == null)
        {
            boardManager = GetComponent<BoardManager>();
        }
    }

    public void HighlightCell(int row, int col)
    {
        if (_highlightObject == null)
        {
            _highlightObject = Instantiate(selectionHighlightPrefab, transform);
        }
        _highlightObject.EnableArrows(false);
        _highlightObject.transform.localPosition = boardManager.GetGemLocation(row, col);
    }

    public void EnableArrows(int row, int col)
    {
        if (_highlightObject == null)
        {
            return;
        }

        _highlightObject.EnableArrows(row < boardManager.Rows - 1,    // Top
            col > 0,    // Left
            row > 0,    // Bottom
            col < boardManager.Cols - 1      // Right
            );
    }

    public void DisableArrows()
    {
        if (_highlightObject == null)
        {
            return;
        }

        _highlightObject.EnableArrows(false);
    }
}
