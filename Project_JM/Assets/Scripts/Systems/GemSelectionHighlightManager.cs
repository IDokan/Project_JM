// SPDX-License-Identifier: MIT
// Copyright (c) 03/20/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemSelectionHighlightManager.cs
// Summary: A script to manage GemSelectionHighlight object.

using UnityEngine;

public class GemSelectionHighlightManager : MonoBehaviour
{
    [SerializeField] protected GameObject selectionHighlightPrefab;

    [SerializeField] protected BoardManager boardManager;

    protected GameObject highlightObject = null;

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
        if (highlightObject == null)
        {
            highlightObject = Instantiate(selectionHighlightPrefab, transform);
        }
        highlightObject.transform.localPosition = boardManager.GetGemLocation(row, col);
    }
}
