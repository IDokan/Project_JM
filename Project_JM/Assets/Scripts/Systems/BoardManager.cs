// SPDX-License-Identifier: MIT
// Copyright (c) 11/03/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardManager.cs
// Summary: A script for gem board.

using GemEnums;
using MatchEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoardInfo
{
    public int Rows { get; }
    public int Cols { get; }

    // Returns false if 
    public IReadOnlyList<Vector2Int> DisableGems(IReadOnlyList<Vector2Int> disableIndices);
}

[RequireComponent(typeof(BoardCoverController))]
public class BoardManager : MonoBehaviour, IBoardInfo
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

    [SerializeField] protected MatchEventChannel _matchEvents;
    [SerializeField] protected EnemySpawnedEventChannel _enemySpawnedEventChannel;
    [SerializeField] protected CharacterDeathEventChannel _characterDeathEventChannel;
    [SerializeField] protected BoardDisableEventChannel _boardDisableEvents;

    protected BoardCoverController boardCoverController;

    protected void OnEnable()
    {
        _characterDeathEventChannel.OnRaised += OnAnyoneDied;
        _boardDisableEvents.OnRaised += OnBoardDisabled;
        _enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
    }
    protected void OnDisable()
    {
        _characterDeathEventChannel.OnRaised -= OnAnyoneDied;
        _boardDisableEvents.OnRaised -= OnBoardDisabled;
        _enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
    }

    protected Gem[,] _gems;
    public Gem GemAt(int r, int c) => _gems[r, c];

    protected const int MaxResolveIterations = 100;

    private int _numMovingGems = 0;

    public bool InputEnabled => !_busy;
    protected bool _busy;

    public bool InBounds(int r, int c) => r >= 0 && r < _rows && c >= 0 && c < _cols;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateBoard();
        _busy = false;

        boardCoverController = GetComponent<BoardCoverController>();
        boardCoverController.SetBoardSizeData(_rows, _cols, _cellSize, _spacing);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
    }

    // A function that resolve matches only when board initially generated.
    protected void GenerateBoard()
    {
        _gems = new Gem[_rows, _cols];

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                _gems[r, c] = GetRandomGemAboveContainer(r, c);
                if (HasMatchAtBeginning(r, c))
                {
                    _gems[r, c].Init(GemColorUtility.GetRandomGemColorExcept(_gems[r, c].Color));
                }
                MoveGem(_gems[r, c], r, c);
            }
        }
    }

    // It takes row & col for only gem location.
    protected Gem GetRandomGem(int row, int col)
    {
        GameObject gemObj = Instantiate(_gemPrefab, transform);
        Vector2 gemLocalPos = GetGemLocation(row, col);
        gemObj.transform.localPosition = gemLocalPos;
        Gem gem = gemObj.GetComponent<Gem>();
        GemColor color = GemColorUtility.GetRandomGemColor();
        gem.Init(color);

        return gem;
    }

    // It takes row & col for only gem location.
    protected Gem GetRandomGemAboveContainer(int row, int col)
    {
        return GetRandomGem(row + _rows, col);
    }

    protected bool ResolveMatches()
    {
        bool hasAnyMatch = false;

        var matches = FindMatchedCells();

        if (matches.Count == 0)
        {
            return hasAnyMatch;
        }

        var matchTypes = FindMatchTypes();
        foreach (var (color, tier) in matchTypes)
        {
            FireMatchEvent(color, tier);
        }

        hasAnyMatch = true;

        foreach (var index in matches)
        {
            // resolve gems
            ResolveGem(index.x, index.y);
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

    protected List<Vector2Int> FindMatchedCells()
    {
        var matches = new List<Vector2Int>();

        var seen = new bool[_rows, _cols];

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                var gem = _gems[row, col];
                if (gem == null)
                {
                    continue;
                }

                var color = gem.Color;

                // Horizontal check (1x3)
                if (col <= _cols - 3 &&
                    color == _gems[row, col + 1].Color &&
                    color == _gems[row, col + 2].Color)
                {
                    for (int offset = 0; offset < 3; offset++)
                    {
                        if (!seen[row, col + offset])
                        {
                            matches.Add(new Vector2Int(row, col + offset));
                            seen[row, col + offset] = true;
                        }
                    }
                }

                // Vertical check (3x1)
                if (row <= _rows - 3 &&
                    color == _gems[row + 1, col].Color &&
                    color == _gems[row + 2, col].Color)
                {
                    for (int offset = 0; offset < 3; offset++)
                    {
                        if (!seen[row + offset, col])
                        {
                            matches.Add(new Vector2Int(row + offset, col));
                            seen[row + offset, col] = true;
                        }
                    }
                }
            }
        }

        return matches;
    }

    protected List<(GemColor Color, int Tier)> FindMatchTypes()
    {
        List<(GemColor Color, int Tier)> result = new List<(GemColor, int)>();
        var horizontalSeen = new bool[_rows, _cols];
        var verticalSeen = new bool[_rows, _cols];

        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                GemColor targetColor = _gems[row, col].Color;

                // Horizontal checks
                int horizontalCheck = 1;

                // out of bound check && prevent double check
                // && are they same colors?
                while (col + horizontalCheck < _cols && horizontalSeen[row, col] == false 
                    && targetColor == _gems[row, col + horizontalCheck].Color)
                {
                    horizontalCheck++;
                }

                if(horizontalCheck >= 3)
                {
                    for(int i = 0; i < horizontalCheck; ++i)
                    {
                        horizontalSeen[row, col + i] = true;
                    }

                    result.Add((targetColor, horizontalCheck));
                }


                // Vertical checks
                int verticalCheck = 1;

                // out of bound check && prevent double check
                // && are they same colors?
                while (row + verticalCheck < _rows && verticalSeen[row, col] == false
                    && targetColor == _gems[row + verticalCheck, col].Color)
                {
                    verticalCheck++;
                }

                if (verticalCheck >= 3)
                {
                    for (int i = 0; i < verticalCheck; ++i)
                    {
                        verticalSeen[row + i, col] = true;
                    }

                    result.Add((targetColor, verticalCheck));
                }
            }
        }

        return result;
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
            ResolveMatches();
        }
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
        if (_gems[row, col] != null)
        {
            _gems[row, col].Resolve();
            _gems[row, col] = null;
        }
    }

    // A function to test board has match only and if only at the beginning (Start&GenerateBoard stage)
    protected bool HasMatchAtBeginning(int row, int col)
    {
        if (_gems[row, col] == null)
        {
            return false;
        }

        GemColor color = _gems[row, col].Color;

        // Horizontal check
        int count = 1;
        int c = col - 1;
        while (c >= 0)
        {
            if (_gems[row, c].Color == color)
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
            if (_gems[r, col].Color == color)
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

    protected bool HasMatchAt(int row, int col)
    {
        if (_gems[row, col] == null)
        {
            return false;
        }

        GemColor color = _gems[row, col].Color;

        // Horizontal check
        int count = 1;
        int c = col - 1;
        while (c >= 0)
        {
            if (_gems[row, c].Color == color)
            {
                count++;
                c--;
            }
            else
            {
                break;
            }
        }

        c = col + 1;

        while (c < _cols)
        {
            if (_gems[row, c].Color == color)
            {
                count++;
                c++;
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
            if (_gems[r, col].Color == color)
            {
                count++;
                r--;
            }
            else
            {
                break;
            }
        }

        r = row + 1;
        while (r < _rows)
        {
            if (_gems[r, col].Color == color)
            {
                count++;
                r++;
            }
            else
            {
                break;
            }
        }

        return count >= 3;
    }

    // Return false if player tried pass invalid direction (out of bounds).
                        // and if board is busy
    public bool TrySwapFrom(Vector2Int index, Vector2Int dir)
    {
        if (_busy)
        {
            return false;
        }

        int targetRow = index.x + dir.y;
        int targetCol = index.y + dir.x;

        if (InBounds(index.x, index.y) && InBounds(targetRow, targetCol))
        {
            MoveGem(_gems[index.x, index.y], targetRow, targetCol);
            MoveGem(_gems[targetRow, targetCol], index.x, index.y);

            (_gems[index.x, index.y], _gems[targetRow, targetCol]) = (_gems[targetRow, targetCol], _gems[index.x, index.y]);

            // Restore status before swap if no match found
            if (HasMatchAt(index.x, index.y) == false && HasMatchAt(targetRow, targetCol) == false)
            {
                MoveGem(_gems[index.x, index.y], targetRow, targetCol);
                MoveGem(_gems[targetRow, targetCol], index.x, index.y);

                (_gems[index.x, index.y], _gems[targetRow, targetCol]) = (_gems[targetRow, targetCol], _gems[index.x, index.y]);
            }

            return true;
        }

        return false;
    }

    protected void FireMatchEvent(GemColor color, int count)
    {
        var tier = (MatchTier)Mathf.Clamp(count, 3, 5);
        _matchEvents.Raise(new MatchEvent
        {
            Color = color,
            Tier = tier
        });
    }

    protected void OnBoardDisabled(BoardDisableLogic logic)
    {
        var context = new BoardDisableContext
        {
            BoardInfo = this
        };

        StartCoroutine(RunBoardDisableAttack(logic, context));
    }

    protected IEnumerator RunBoardDisableAttack(BoardDisableLogic logic, BoardDisableContext context)
    {
        _numMovingGems++;
        yield return StartCoroutine(logic.Execute(context));
        ResolveGemMovement();
    }

    public IReadOnlyList<Vector2Int> DisableGems(IReadOnlyList<Vector2Int> disableIndices)
    {
        var failed = new List<Vector2Int>();
        foreach (var index in disableIndices)
        {
            var Gem = _gems[index.x, index.y];
            if (Gem.Color != GemColor.None)
            {
                _gems[index.x, index.y].Init(GemColor.None);
            }
            else
            {
                failed.Add(index);
            }
        }

        return failed;
    }

    protected void OnAnyoneDied(CharacterStatus stat)
    {
        _busy = true;

        StartCoroutine(ClearAndRefillGemsAfterDelay(1f));

        boardCoverController.ShowCover();
    }

    protected void OnEnemySpawned(GameObject gameObject)
    {
        _busy = false;

        boardCoverController.HideCover();
    }

    protected IEnumerator ClearAndRefillGemsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearAndRefillGems();
    }

    protected void ClearAndRefillGems()
    {
        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                ResolveGem(r, c);
            }
        }

        GenerateBoard();
    }
}
