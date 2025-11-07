// SPDX-License-Identifier: MIT
// Copyright (c) 11/03/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardManager.cs
// Summary: A script for gem board.

using GemEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] protected int _rows = 8;
    public int Rows => _rows;
    [SerializeField] protected int _cols = 8;
    public int Cols => _cols;
    [SerializeField] protected float _cellSize = 1f;
    public float CellSize => _cellSize;
    [SerializeField] protected float _spacing = 0.05f;
    public float Spacing => _spacing;
    [SerializeField] protected GameObject _gemPrefab;
    [SerializeField] protected float _fallingSpeed = 3f;
    protected Gem[,] _gems;
    public Gem GemAt(int r, int c) => _gems[r, c];

    [SerializeField] public int initialSeed = 12345;
    protected const int MaxResolveIterations = 100;
    protected System.Random _random;

    private bool _isResolving = false;
    private int _numMovingGems = 0;

    public bool InputEnabled => !_busy;
    protected bool _busy;

    public bool InBounds(int r, int c) => r >= 0 && r < _rows && c >= 0 && c < _cols;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateBoard();
        _busy = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        SetRandomSeed(initialSeed);
    }

    // A function that resolve matches only when board initially generated.
    protected void GenerateBoard()
    {
        _gems = new Gem[_rows, _cols];

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                _gems[r, c] = GetRandomGem(r, c);
                if (HasMatchAtBeginning(r, c))
                {
                    _gems[r, c].Init(GemColorUtility.GetRandomGemColorExcept(_random, _gems[r, c].color));
                }
            }
        }
    }

    protected Gem GetRandomGem(int row, int col)
    {
        GameObject gemObj = Instantiate(_gemPrefab, transform);
        Vector2 gemLocalPos = GetGemLocation(row, col);
        gemObj.transform.localPosition = gemLocalPos;
        Gem gem = gemObj.GetComponent<Gem>();
        GemColor color = GemColorUtility.GetRandomGemColor(_random);
        gem.Init(color);

        return gem;
    }

    public void SetRandomSeed(int seed)
    {
        _random = new System.Random(seed);
    }

    protected bool ResolveMatches()
    {
        bool hasAnyMatch = false;

        var matches = FindMatches();

        if (matches.Count == 0)
        {
            return hasAnyMatch;
        }

        hasAnyMatch = true;
        _isResolving = true;

        foreach (var (row, col) in matches)
        {
            // resolve gems
            ResolveGem(row, col);
        }

        ApplyGravity();
        RefillBoard();

        return hasAnyMatch;
    }

    protected void ApplyGravity()
    {
        for (int col = 0; col < _cols; col++)
        {
            int writeRow = 0;
            for (int row = 0; row < _rows; row++)
            {
                if (_gems[row, col] != null)
                {
                    if (row != writeRow)
                    {
                        _gems[writeRow, col] = _gems[row, col];
                        _gems[row, col] = null;
                        MoveGem(_gems[writeRow, col], writeRow, col);
                    }

                    writeRow++;
                }
            }
        }
    }

    protected void RefillBoard()
    {
        for (int col = 0; col < _cols; col++)
        {
            int numRefilledGem = 0;
            for (int row = 0; row < _rows; row++)
            {
                if (_gems[row, col] == null)
                {
                    _gems[row, col] = GetRandomGem(_rows + (numRefilledGem++), col);
                    MoveGem(_gems[row, col], row, col);
                }
            }
        }
    }

    protected List<(int Row, int Column)> FindMatches()
    {
        var matches = new List<(int, int)>();

        var seen = new bool[_rows, _cols];

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                if (_gems[row, col] == null)
                {
                    continue;
                }

                // Horizontal check (1x3)
                if (col <= _cols - 3 &&
                    _gems[row, col].color == _gems[row, col + 1].color &&
                    _gems[row, col].color == _gems[row, col + 2].color)
                {
                    for (int offset = 0; offset < 3; offset++)
                    {
                        if (!seen[row, col + offset])
                        {
                            matches.Add((row, col + offset));
                            seen[row, col + offset] = true;
                        }
                    }
                }

                // Vertical check (3x1)
                if (row <= _rows - 3 &&
                    _gems[row, col].color == _gems[row + 1, col].color &&
                    _gems[row, col].color == _gems[row + 2, col].color)
                {
                    for (int offset = 0; offset < 3; offset++)
                    {
                        if (!seen[row + offset, col])
                        {
                            matches.Add((row + offset, col));
                            seen[row + offset, col] = true;
                        }
                    }
                }
            }
        }

        return matches;
    }

    protected void MoveGem(Gem gem, int newRow, int newCol)
    {
        _numMovingGems++;

        Vector2 targetLocation = GetGemLocation(newRow, newCol);
        gem.GetComponent<GemMover>().EnqueueMove(targetLocation, onComplete: ResolveGemMovement);
    }

    protected void ResolveGemMovement()
    {
        if (--_numMovingGems <= 0)
        {
            _isResolving = false;
        }

        RequestResolve();
    }

    public Vector2 GetGemLocation(int row, int col)
    {
        return new Vector2(col * (_cellSize + _spacing), row * (_cellSize + _spacing));
    }

    // This function takes only local position according to the board.
    public Vector2Int GetGemIndex(Vector2 localPosition)
    {
        float cellUnit = 1f / (_cellSize + _spacing);
        int col = Mathf.FloorToInt((localPosition.x + (cellUnit * 0.5f)) * cellUnit);
        int row = Mathf.FloorToInt((localPosition.y + (cellUnit * 0.5f)) * cellUnit);

        Vector2Int index = new Vector2Int(row, col);
        return InBounds(index.x, index.y) ? index : new Vector2Int(-1, -1);
    }

    protected void ResolveGem(int row, int col)
    {
        // @@ TODO: Improve resolving method to look much fancier.
        // @@ TODO: Implement object pool for gems.

        Destroy(_gems[row, col].gameObject);
        _gems[row, col] = null;
    }

    protected void RequestResolve()
    {
        if (!_isResolving)
        {
            ResolveMatches();
        }
    }

    // A function to test board has match only and if only at the beginning (Start&GenerateBoard stage)
    protected bool HasMatchAtBeginning(int row, int col)
    {
        if (_gems[row, col] == null)
        {
            return false;
        }

        GemColor color = _gems[row, col].color;

        // Horizontal check
        int count = 1;
        int c = col - 1;
        while (c >= 0)
        {
            if (_gems[row, c].color == color)
            {
                count++;
                c--;
            }
            else
            {
                break;
            }
        }

        if (count >= 3)
        {
            return true;
        }

        // Vertical check
        count = 1;
        int r = row - 1;
        while (r >= 0)
        {
            if (_gems[r, col].color == color)
            {
                count++;
                r--;
            }
            else
            {
                break;
            }
        }

        return count >= 3;
    }

    public bool TrySwapFrom(Vector2Int index, Vector2Int dir)
    {
        int targetRow = index.x + dir.y;
        int targetCol = index.y + dir.x;

        if (InBounds(index.x, index.y) && InBounds(targetRow, targetCol))
        {
            MoveGem(_gems[index.x, index.y], targetRow, targetCol);
            MoveGem(_gems[targetRow, targetCol], index.x, index.y);

            (_gems[index.x, index.y], _gems[targetRow, targetCol]) = (_gems[targetRow, targetCol], _gems[index.x, index.y]);

            return true;
        }

        return false;
    }
}
