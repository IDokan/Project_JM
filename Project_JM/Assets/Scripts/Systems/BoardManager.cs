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

    [SerializeField] protected PartyRoster partyRoster;
    [SerializeField] protected MatchEventChannel _matchEvents;
    [SerializeField] protected GemPowerArrivedEventChannel _powerArrivedEvents;
    [SerializeField] protected EnemySpawnedEventChannel _enemySpawnedEventChannel;
    [SerializeField] protected CharacterDeathEventChannel _characterDeathEventChannel;
    [SerializeField] protected BoardDisableEventChannel _boardDisableEvents;

    protected BoardCoverController boardCoverController;



    // Tracks which pending match-groups each gem belongs to.
    // Key: Gem instance id
    private readonly Dictionary<int, List<PendingMatchGroup>> _pendingByGemID = new();


    protected sealed class PendingMatchGroup
    {
        public GemColor Color { get; }
        public int Required { get; }
        public bool Completed { get; set; }

        private readonly int[] _allIDs;
        private readonly HashSet<int> _remainingIDs;

        public PendingMatchGroup(GemColor color, List<int> gemIDs)
        {
            Color = color;
            _allIDs = gemIDs.ToArray();
            _remainingIDs = new HashSet<int>(_allIDs);
            Required = _remainingIDs.Count;
        }

        public bool TryConsume(int gemID) => _remainingIDs.Remove(gemID);
        public bool IsComplete => _remainingIDs.Count <= 0;
        public IReadOnlyList<int> AllIDs => _allIDs;
    }

    protected readonly struct MatchGroup
    {
        public readonly GemColor Color;
        public readonly List<Vector2Int> Cells;

        public MatchGroup(GemColor color, List<Vector2Int> cells)
        {
            Color = color;
            Cells = cells;
        }
    }

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

    protected void ResolveMatches()
    {
        var groups = FindMatchGroups();
        if (groups.Count == 0)
        {
            return;
        }

        // Register first
        RegisterPendingGroups(groups);

        // Immediate match event
        foreach (var group in groups)
        {
            FireMatchEvent(group.Color, group.Cells.Count);
        }

        // Resolve each cell once (union),
        // but let each gem completion advance multiple groups
        var toResolve = new HashSet<Vector2Int>();
        foreach (var group in groups)
        {
            foreach (var cell in group.Cells)
            {
                toResolve.Add(cell);
            }
        }

        foreach (var cell in toResolve)
        {
            // resolve gems
            ResolveGem(cell.x, cell.y);
        }

        ApplyGravity();
        RefillBoard();
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

    protected List<MatchGroup> FindMatchGroups()
    {
        var groups = new List<MatchGroup>();

        // Horizontal runs
        for (int row = 0; row < _rows; ++row)
        {
            int col = 0;
            while (col < _cols)
            {
                var gem = _gems[row, col];

                if (gem == null || gem.Color == GemColor.None)
                {
                    col++;
                    continue;
                }

                var color = gem.Color;
                int start = col;
                int len = 1;

                while (start + len < _cols)
                {
                    var g2 = _gems[row, start + len];
                    if (g2 == null || g2.Color != color)
                    {
                        break;
                    }

                    len++;
                }

                if (len >= 3)
                {
                    var cells = new List<Vector2Int>(len);

                    for (int i = 0; i < len; i++)
                    {
                        cells.Add(new Vector2Int(row, start + i));
                    }

                    groups.Add(new MatchGroup(color, cells));
                }

                // Skip run as much as recorded to groups
                col = start + len;
            }
        }


        // Vertical runs
        for (int col = 0; col < _cols; ++col)
        {
            int row = 0;

            while (row < _rows)
            {
                var gem = _gems[row, col];

                if (gem == null || gem.Color == GemColor.None)
                {
                    row++;
                    continue;
                }

                var color = gem.Color;
                int start = row;
                int len = 1;

                while (start + len < _rows)
                {
                    var g2 = _gems[start + len, col];
                    if (g2 == null || g2.Color != color)
                    {
                        break;
                    }
                    len++;
                }

                if (len >= 3)
                {
                    var cells = new List<Vector2Int>(len);
                    for (int i = 0; i < len; ++i)
                    {
                        cells.Add(new Vector2Int(start + i, col));
                    }

                    groups.Add(new MatchGroup(color, cells));
                }

                row = start + len;
            }
        }

        return groups;
    }

    protected void RegisterPendingGroups(List<MatchGroup> groups)
    {
        foreach (var group in groups)
        {
            // Capture gem ids NOW (before ResolveGem sets board slot to NULL)
            var ids = new List<int>(group.Cells.Count);

            foreach (var cell in group.Cells)
            {
                var gem = _gems[cell.x, cell.y];
                if (gem == null || gem.Color == GemColor.None)
                {
                    continue;
                }

                ids.Add(gem.GetInstanceID());
            }

            if (ids.Count == 0)
            {
                continue;
            }

            var pending = new PendingMatchGroup(group.Color, ids);

            foreach (var id in ids)
            {
                if (!_pendingByGemID.TryGetValue(id, out var list))
                {
                    list = new List<PendingMatchGroup>(2);
                    _pendingByGemID.Add(id, list);
                }

                list.Add(pending);
            }
        }
    }

    protected void UnregisterGroup(PendingMatchGroup group)
    {
        foreach (var id in group.AllIDs)
        {
            if (_pendingByGemID.TryGetValue(id, out var list))
            {
                list.Remove(group);

                if (list.Count == 0)
                {
                    _pendingByGemID.Remove(id);
                }
            }
        }
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
        // @@ TODO: Implement object pool for gems.
        var gem = _gems[row, col];

        if (gem != null)
        {
            int id = gem.GetInstanceID();
            gem.Resolve(partyRoster, color => NotifyAbsorbed(color, id));
            _gems[row, col] = null;
        }
    }

    // @@ TODO: Resolve bug in this code
    // Bug: It cannot detect gem absorbed correctly on the overlapped matches such as (5vertical X 3 horizontal).
    public void NotifyAbsorbed(GemColor color, int gemID)
    {
        if (!_pendingByGemID.TryGetValue(gemID, out var groups))
        {
            // late / unexpected; ignore it
            return;
        }

        // One gem can belong to multiple match groups (overlap).
        for (int i = groups.Count - 1; i >= 0; --i)
        {
            var group = groups[i];
            if (group.Completed)
            {
                continue;
            }

            if (!group.TryConsume(gemID))
            {
                continue;
            }

            if (group.IsComplete)
            {
                group.Completed = true;

                var tier = MatchTierUtil.GetMatchTier(group.Required);
                _powerArrivedEvents.Raise(new MatchEvent
                {
                    Color = group.Color,
                    Tier = tier
                });

                UnregisterGroup(group);
            }
        }

        // This gem id should NEVER be needed again after it "arrived"
        _pendingByGemID.Remove(gemID);
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
        var tier = MatchTierUtil.GetMatchTier(count);
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
