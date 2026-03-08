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


    public bool CanBeDisable(Vector2Int index);

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

    [SerializeField] protected GameObject _gemDisableFXPrefab;

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

    protected struct MatchCheckResult
    {
        public int HorizontalCount;
        public int VerticalCount;

        public List<Vector2Int> HorizontalIndices;
        public List<Vector2Int> VerticalIndices;

        public bool HasMatch => HorizontalCount >= 3 || VerticalCount >= 3;

        public MatchCheckResult(int horizontalCount, int verticalCount, List<Vector2Int> horizontalIndices, List<Vector2Int> verticalIndices)
        {
            HorizontalCount = horizontalCount;
            VerticalCount = verticalCount;
            HorizontalIndices = horizontalIndices;
            VerticalIndices = verticalIndices;
        }

        public static MatchCheckResult Empty()
        {
            return new MatchCheckResult(0, 0, new List<Vector2Int>(), new List<Vector2Int>());
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

    protected readonly HashSet<GameObject> _disableFXs = new ();
    protected readonly HashSet<Vector2Int> _disableIndices = new();
    protected readonly HashSet<GemShake> _activeShakes = new();

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

                List<GemColor> excludeColors = new List<GemColor>();
                while (HasMatchAtBeginning(r, c))
                {
                    GemColor currentColor = _gems[r, c].Color;
                    if (excludeColors.Contains(currentColor) == false)
                    {
                        excludeColors.Add(currentColor);
                    }

                    _gems[r, c].Init(GemColorUtility.GetRandomGemColorExcept(excludeColors.ToArray()));
                }
                MoveGem(_gems[r, c], r, c);
            }
        }

        // @@ TODO: Remove the below code and apply shakings when there are no movements after 4 seconds from the last movement.
        ClearShakingEffects();
        List<Vector2Int> hintIndices = FindHintIndices();
        foreach (Vector2Int index in  hintIndices)
        {
            ApplyShaking(index);
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

                if (gem == null)
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

                if (gem == null)
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

        if (gem == null)
        {
            return;
        }

        if (gem.Color == GemColor.None)
        {
            ResolveGemNoTarget(row, col);
            return;
        }


        int id = gem.GetInstanceID();
        gem.Resolve(partyRoster, color => NotifyAbsorbed(color, id));
        _gems[row, col] = null;
    }

    protected void ResolveGemNoTarget(int row, int col)
    {
        // @@ TODO: Implement object pool for gems.
        var gem = _gems[row, col];

        if (gem != null)
        {
            gem.ResolveNoTarget();
            _gems[row, col] = null;
        }
    }

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

    // A function to test board has match only and if only at the beginning (Start&GenerateBoard stage)\
    // Instead of only beginning, it is for total inspection.
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

            // Swap
            SwapGems(index.x, index.y, targetRow, targetCol);

            // Restore status before swap if no match found
            if (HasMatchAt(index.x, index.y) == false && HasMatchAt(targetRow, targetCol) == false)
            {
                MoveGem(_gems[index.x, index.y], targetRow, targetCol);
                MoveGem(_gems[targetRow, targetCol], index.x, index.y);

                SwapGems(index.x, index.y, targetRow, targetCol);
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

    protected void OnBoardDisabled(BoardDisableEventContext context)
    {

        switch (context.boardDisablePhase)
        {
            case BoardDisablePhase.Preview:
                SpawnTileDisableEffect(context.boardDisableLogic);
                break;
            case BoardDisablePhase.Commit:
                StartCoroutine(RunBoardDisableAttack(context.boardDisableLogic));
                break;


            default:
                break;
        }
    }

    protected void SpawnTileDisableEffect(BoardDisableLogic logic)
    {
        ClearDisableEffects();

        // Spawn disable effect to the location queried by logic.
        var context = new BoardDisableContext
        {
            BoardInfo = this
        };
        IReadOnlyList<Vector2Int> indices = logic.PreviewGemWillDisabled(context);
        for (int i = 0; i < indices.Count; i++)
        {
            Vector2Int index = indices[i];
            _disableIndices.Add(index);

            GameObject fx = Instantiate(_gemDisableFXPrefab, transform);
            fx.transform.localPosition = GetGemLocation(index.x, index.y);
            _disableFXs.Add(fx);
        }

    }

    protected IEnumerator RunBoardDisableAttack(BoardDisableLogic logic)
    {
        var context = new BoardDisableContext
        {
            BoardInfo = this
        };

        _numMovingGems++;
        yield return StartCoroutine(logic.Execute(context));
        ResolveGemMovement();
    }


    public bool CanBeDisable(Vector2Int index)
    {
        // If gem board is not initialized, there are no disabled gems => all cells can be disable.
        if (_gems == null)
        {
            return true;
        }
        var gem = _gems[index.x, index.y];
        return gem != null ? (gem.Color != GemColor.None) : false;
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
        ClearDisableEffects();

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                ResolveGemNoTarget(r, c);
            }
        }

        _numMovingGems = 0;

        GenerateBoard();
    }

    protected void ApplyShaking(int row, int col, Gem gem = null)
    {
        ApplyShaking(new Vector2Int(row, col), gem);
    }

    protected void ApplyShaking(Vector2Int index, Gem gem = null)
    {
        if (_gems == null)
        {
            return;
        }

        if (gem == null)
        {
            gem = _gems[index.x, index.y];
        }

        GemShake gemShake = gem.GetComponentInChildren<GemShake>();

        if (gemShake == null)
        {
            return;
        }

        // @@ TODO: Remove shaking effects when they moved.
        // @@ TODO: Remove commented out lines.
        //if (_disableIndices.Contains(index))
        {
            gemShake.StartShake();
            _activeShakes.Add(gemShake);
        }
        //else
        //{
        //    gemShake.StopShake();
        //    _activeShakes.Remove(gemShake);
        //}
    }

    protected void DisableShaking(int row, int col)
    {
        GemShake gemShake = _gems[row, col]?.GetComponentInChildren<GemShake>();
        gemShake.StopShake();
        if (_activeShakes.Contains(gemShake))
        {
            _activeShakes.Remove(gemShake);
        }
    }

    protected void ClearShakingEffects()
    {
        foreach (GemShake gs in _activeShakes)
        {
            if (gs)
            {
                gs.StopShake();
            }
        }
        _activeShakes.Clear();
    }

    protected void ClearDisableEffects()
    {
        foreach (GameObject go in _disableFXs)
        {
            if (go)
            {
                FadeOnSpawnAndDeath fadeScript = go.GetComponent<FadeOnSpawnAndDeath>();
                if (fadeScript != null)
                {
                    fadeScript.FadeOutAndDestroy();
                }
                else
                {
                    Destroy(go);
                }
            }
        }
        _disableFXs.Clear();
        _disableIndices.Clear();
    }

    protected void SwapGems(int row1, int col1, int row2, int col2)
    {
        (_gems[row1, col1], _gems[row2, col2]) = (_gems[row2, col2], _gems[row1, col1]);
    }

    protected List<Vector2Int> FindHintIndices()
    {
        List<Vector2Int> result = new();

        for (int i = 0; i < _rows - 1; i++)
        {
            for (int j = 0; j < _cols - 1; j++)
            {
                // Try swap and find if it has match

                SwapGems(i, j, i, j + 1);
                MatchCheckResult horizontalSwapResult = GetMatchAt(i, j);
                if (horizontalSwapResult.HasMatch)
                {
                    SwapGems(i, j, i, j + 1);
                    result.Add(new Vector2Int(i, j + 1));
                    result.AddRange(horizontalSwapResult.HorizontalIndices);
                    result.AddRange(horizontalSwapResult.VerticalIndices);
                    return result;
                }
                MatchCheckResult horizontalSwapResult2 = GetMatchAt(i, j + 1);
                if (horizontalSwapResult2.HasMatch)
                {
                    SwapGems(i, j, i, j + 1);
                    result.Add(new Vector2Int(i, j));
                    result.AddRange(horizontalSwapResult2.HorizontalIndices);
                    result.AddRange(horizontalSwapResult2.VerticalIndices);
                    return result;
                }
                SwapGems(i, j, i, j + 1);

                SwapGems(i, j, i + 1, j);
                MatchCheckResult verticalSwapResult = GetMatchAt(i, j);
                if (verticalSwapResult.HasMatch)
                {
                    SwapGems(i, j, i + 1, j);
                    result.Add(new Vector2Int(i + 1, j));
                    result.AddRange(verticalSwapResult.HorizontalIndices);
                    result.AddRange(verticalSwapResult.VerticalIndices);
                    return result;
                }
                MatchCheckResult verticalSwapResult2 = GetMatchAt(i + 1, j);
                if (verticalSwapResult.HasMatch)
                {
                    SwapGems(i, j, i + 1, j);
                    result.Add(new Vector2Int(i, j));
                    result.AddRange(verticalSwapResult2.HorizontalIndices);
                    result.AddRange(verticalSwapResult2.VerticalIndices);
                    return result;
                }
                SwapGems(i, j, i + 1, j);
            }
        }

        return result;
    }

    // Warning: the result indices does not contain (row, col)
    protected MatchCheckResult GetMatchAt(int row, int col)
    {
        MatchCheckResult result = MatchCheckResult.Empty();

        if (!InBounds(row, col) || _gems[row, col] == null)
        {
            return result;
        }

        GemColor color = _gems[row, col].Color;

        // Horizontal check
        result.HorizontalCount = 1;
        int c = col - 1;
        while (c >= 0)
        {
            if (_gems[row, c].Color == color)
            {
                result.HorizontalCount++;
                result.HorizontalIndices.Add(new Vector2Int(row, c));
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
                result.HorizontalCount++;
                result.HorizontalIndices.Add(new Vector2Int(row, c));
                c++;
            }
            else
            {
                break;
            }
        }

        // Vertical check
        result.VerticalCount = 1;
        int r = row - 1;
        while (r >= 0)
        {
            if (_gems[r, col].Color == color)
            {
                result.VerticalCount++;
                result.VerticalIndices.Add(new Vector2Int(r, col));
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
                result.VerticalCount++;
                result.VerticalIndices.Add(new Vector2Int(r, col));
                r++;
            }
            else
            {
                break;
            }
        }

        return result;
    }
}
