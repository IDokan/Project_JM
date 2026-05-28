// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/03/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardManager.cs
// Summary: A script for gem board.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using GemEnums;
using MatchEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] protected int rows = 8;
    public int Rows => rows;
    [SerializeField] protected int cols = 8;
    public int Cols => cols;
    [SerializeField] protected float cellSize = 1f;
    public float CellSize => cellSize;
    [SerializeField] protected float spacing = 0.05f;
    public float Spacing => spacing;
    [SerializeField] protected GameObject gemPrefab;
    [SerializeField] protected float fallingSpeed = 3f;

    [SerializeField] protected PartyRoster partyRoster;
    [SerializeField] protected MatchEventChannel matchEvents;
    [SerializeField] protected GemPowerArrivedEventChannel powerArrivedEvents;
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected BoardDisableEventChannel boardDisableEvents;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected FadeOnSpawnAndDeath gemDisableFXPrefab;

    // Hint delay starts from user's input
    [Header("Hint")]
    [SerializeField] protected float hintDelay = 4f;
    [SerializeField] protected BoardAudioPlayer boardAudioPlayer;

    protected BoardCoverController _boardCoverController;



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

    protected struct HintResult
    {
        public bool Found;
        public List<Vector2Int> Indices;
    }

    protected void OnEnable()
    {
        boardDisableEvents.OnRaised += OnBoardDisabled;
        enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }
    protected void OnDisable()
    {
        boardDisableEvents.OnRaised -= OnBoardDisabled;
        enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected Gem[,] _gems;
    public Gem GemAt(int r, int c) => _gems[r, c];

    protected const int MaxResolveIterations = 100;

    private int _numMovingGems = 0;

    public bool InputEnabled => !_busy && _gems != null;
    protected bool _busy;

    protected readonly List<FadeOnSpawnAndDeath> _disableFXs = new();

    protected struct ShakingData
    {
        public Vector2Int index;
        public GemColor gemColor;
        public GemShake shaker;
    }

    protected readonly List<List<ShakingData>> _shakingDataContainer = new();

    protected Coroutine _hintRoutine = null;

    public bool InBounds(int r, int c) => r >= 0 && r < rows && c >= 0 && c < cols;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _boardCoverController = GetComponent<BoardCoverController>();
        _boardCoverController.SetBoardSizeData(rows, cols, cellSize, spacing);
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
        _gems = new Gem[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
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
    }

    // It takes row & col for only gem location.
    protected Gem GetRandomGem(int row, int col)
    {
        GameObject gemObj = Instantiate(gemPrefab, transform);
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
        return GetRandomGem(row + rows, col);
    }

    protected void ResolveMatches()
    {
        var groups = FindMatchGroups();
        if (groups.Count == 0 || _busy)
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

        // If there was a gem that marked as hint, stop them and its group.
        StopShaking(toResolve);
        StartHintRoutine();

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
        for (int col = 0; col < cols; col++)
        {
            int writeRow = 0;
            for (int row = 0; row < rows; row++)
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
        for (int col = 0; col < cols; col++)
        {
            int numRefilledGem = 0;
            for (int row = 0; row < rows; row++)
            {
                if (_gems[row, col] == null)
                {
                    _gems[row, col] = GetRandomGem(rows + (numRefilledGem++), col);
                    MoveGem(_gems[row, col], row, col);
                }
            }
        }
    }

    protected List<MatchGroup> FindMatchGroups()
    {
        var groups = new List<MatchGroup>();

        // Horizontal runs
        for (int row = 0; row < rows; ++row)
        {
            int col = 0;
            while (col < cols)
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

                while (start + len < cols)
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
        for (int col = 0; col < cols; ++col)
        {
            int row = 0;

            while (row < rows)
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

                while (start + len < rows)
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
        return new Vector2(col * (cellSize + spacing), row * (cellSize + spacing));
    }

    // This function takes only local position according to the board.
    public Vector2Int GetGemIndex(Vector2 localPosition)
    {
        float cellUnit = 1f / (cellSize + spacing);
        int col = Mathf.FloorToInt((localPosition.x + (cellUnit * 0.5f)) * cellUnit);
        int row = Mathf.FloorToInt((localPosition.y + (cellUnit * 0.5f)) * cellUnit);

        Vector2Int index = new Vector2Int(row, col);
        return InBounds(index.x, index.y) ? index : new Vector2Int(-1, -1);
    }

    public GemColor GetGemColor(Vector2Int index)
    {
        if (_gems == null || !InBounds(index.x, index.y))
        {
            return GemColor.None;
        }

        return _gems[index.x, index.y].Color;
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
        if (_gems == null)
        {
            return;
        }

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
                powerArrivedEvents.Raise(new MatchEvent
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

        while (c < cols)
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
        while (r < rows)
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
            boardAudioPlayer?.PlayGemSwap();

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

        boardAudioPlayer?.PlayInvalidSwap();
        return false;
    }

    protected void FireMatchEvent(GemColor color, int count)
    {
        if (color == GemColor.None)
            boardAudioPlayer.PlayNoneMatch();

        var tier = MatchTierUtil.GetMatchTier(count);
        matchEvents.Raise(new MatchEvent
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

            FadeOnSpawnAndDeath fx = Instantiate(gemDisableFXPrefab, transform);
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

        boardDisableEvents.Raise(new BoardDisableEventContext
        {
            boardDisablePhase = BoardDisablePhase.Preview,
            boardDisableLogic = logic
        });
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

        StopShaking(disableIndices);

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

    protected void EnableCover(bool refillGems = true)
    {
        _busy = true;

        if (refillGems)
        {
            StartCoroutine(ClearAndRefillGemsAfterDelay(1f));
        }

        StopHintRoutine();

        _boardCoverController.ShowCover();
    }

    protected void OnEnemySpawned(GameObject gameObject)
    {
        _busy = false;

        StartHintRoutine();

        _boardCoverController.HideCover();
    }

    protected IEnumerator ClearAndRefillGemsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ClearAndRefillGems();
    }

    protected void ClearAndRefillGems()
    {
        ClearDisableEffects();
        ClearShakingEffects();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                ResolveGemNoTarget(r, c);
            }
        }

        _numMovingGems = 0;

        GenerateBoard();
    }

    protected void StartShaking(List<Vector2Int> hintIndices)
    {
        if (_gems == null)
        {
            return;
        }

        List<ShakingData> shakingData = new();
        foreach (Vector2Int index in hintIndices)
        {
            Gem gem = _gems[index.x, index.y];
            if (gem == null)
            {
                continue;
            }

            GemShake gemShake = gem.GetComponentInChildren<GemShake>();

            if (gemShake == null)
            {
                return;
            }

            gemShake.StartShake();

            shakingData.Add(new ShakingData
            {
                index = index,
                gemColor = gem.Color,
                shaker = gemShake
            });
        }

        if (shakingData.Count > 0)
        {
            boardAudioPlayer?.PlayGemHint();
        }

        _shakingDataContainer.Add(shakingData);

    }

    protected void StopShaking(Vector2Int index)
    {
        StopShaking(new[] { index });
    }

    protected void StopShaking(IEnumerable<Vector2Int> indexEnumerable)
    {
        HashSet<Vector2Int> target = new HashSet<Vector2Int>(indexEnumerable);

        bool ShouldRemove(List<ShakingData> inner)
        {
            return inner.Any(item => target.Contains(item.index));
        }

        List<List<ShakingData>> removedData = _shakingDataContainer.Where(ShouldRemove).ToList();

        foreach (List<ShakingData> data in removedData)
        {
            StopShaking(data);
        }

        VerifyShakings();
    }

    protected void StopShaking(List<ShakingData> removedData)
    {
        if (_gems == null)
        {
            return;
        }

        foreach (ShakingData data in removedData)
        {
            data.shaker?.StopShake();
        }

        _shakingDataContainer.Remove(removedData);
    }

    protected void ClearShakingEffects()
    {
        foreach (List<ShakingData> shakingData in _shakingDataContainer)
        {
            foreach (ShakingData data in shakingData)
            {
                data.shaker?.StopShake();
            }
        }
        _shakingDataContainer.Clear();
    }

    protected void ClearDisableEffects()
    {
        foreach (FadeOnSpawnAndDeath fx in _disableFXs)
        {
            if (fx) fx.FadeOutAndDestroy();
        }
        _disableFXs.Clear();
    }

    protected void VerifyShakings()
    {
        HashSet<List<ShakingData>> invalidData = new();

        foreach (List<ShakingData> shakingData in _shakingDataContainer)
        {
            bool isInvalid = false;

            for (int i = 0; i < shakingData.Count; i++)
            {
                ShakingData data = shakingData[i];

                Gem gem = _gems[data.index.x, data.index.y];
                // If gem became different when marked as hint (started to shake)
                if (gem == null || data.gemColor != gem.Color)
                {
                    isInvalid = true;
                    break;
                }
            }

            if (isInvalid)
            {
                foreach (ShakingData data in shakingData)
                {
                    data.shaker?.StopShake();
                }
                invalidData.Add(shakingData);
            }
        }

        if (invalidData.Count > 0)
        {
            _shakingDataContainer.RemoveAll(item => invalidData.Contains(item));
        }
    }

    protected void SwapGems(int row1, int col1, int row2, int col2)
    {
        (_gems[row1, col1], _gems[row2, col2]) = (_gems[row2, col2], _gems[row1, col1]);
    }

    protected List<Vector2Int> FindHintIndices()
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                // Try swap and find if it has match

                if (i + 1 < rows)
                {
                    HintResult rowHintResult = FindHintFromSwap(i, j, i + 1, j);
                    if (rowHintResult.Found)
                    {
                        return rowHintResult.Indices;
                    }
                }

                if (j + 1 < cols)
                {
                    HintResult colHintResult = FindHintFromSwap(i, j, i, j + 1);
                    if (colHintResult.Found)
                    {
                        return colHintResult.Indices;
                    }
                }
            }
        }

        return new List<Vector2Int>();
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
        List<Vector2Int> horizontalMatchIndices = new List<Vector2Int>();
        int c = col - 1;
        while (c >= 0)
        {
            if (_gems[row, c].Color == color)
            {
                result.HorizontalCount++;
                horizontalMatchIndices.Add(new Vector2Int(row, c));
                c--;
            }
            else
            {
                break;
            }
        }

        c = col + 1;

        while (c < cols)
        {
            if (_gems[row, c].Color == color)
            {
                result.HorizontalCount++;
                horizontalMatchIndices.Add(new Vector2Int(row, c));
                c++;
            }
            else
            {
                break;
            }
        }

        if (result.HorizontalCount >= 3)
        {
            result.HorizontalIndices.AddRange(horizontalMatchIndices);
        }

        // Vertical check
        result.VerticalCount = 1;
        List<Vector2Int> verticalMatchIndices = new List<Vector2Int>();
        int r = row - 1;
        while (r >= 0)
        {
            if (_gems[r, col].Color == color)
            {
                result.VerticalCount++;
                verticalMatchIndices.Add(new Vector2Int(r, col));
                r--;
            }
            else
            {
                break;
            }
        }

        r = row + 1;
        while (r < rows)
        {
            if (_gems[r, col].Color == color)
            {
                result.VerticalCount++;
                verticalMatchIndices.Add(new Vector2Int(r, col));
                r++;
            }
            else
            {
                break;
            }
        }

        if (result.VerticalCount >= 3)
        {
            result.VerticalIndices.AddRange(verticalMatchIndices);
        }

        return result;
    }

    protected HintResult FindHintFromSwap(int row1, int col1, int row2, int col2)
    {
        SwapGems(row1, col1, row2, col2);

        try
        {
            if (IsShaking(row1, col1) == false)
            {

                MatchCheckResult result1 = GetMatchAt(row1, col1);
                if (result1.HasMatch)
                {
                    return new HintResult
                    {
                        Found = true,
                        Indices = BuildHintIndices(row2, col2, result1)
                    };
                }
            }


            if (IsShaking(row2, col2) == false)
            {
                MatchCheckResult result2 = GetMatchAt(row2, col2);
                if (result2.HasMatch)
                {
                    return new HintResult
                    {
                        Found = true,
                        Indices = BuildHintIndices(row1, col1, result2)
                    };
                }
            }
        }
        finally
        {
            SwapGems(row1, col1, row2, col2);
        }

        return new HintResult
        {
            Found = false,
            Indices = null
        };
    }

    protected List<Vector2Int> BuildHintIndices(int movedGemRow, int movedGemCol, MatchCheckResult matchCheck)
    {
        List<Vector2Int> result = new();
        Vector2Int movedIndex = new Vector2Int(movedGemRow, movedGemCol);

        result.Add(movedIndex);
        result.AddRange(matchCheck.HorizontalIndices);
        result.AddRange(matchCheck.VerticalIndices);

        return result;
    }

    protected void StartHintRoutine()
    {
        if (_hintRoutine != null)
        {
            StopHintRoutine();
        }

        _hintRoutine = StartCoroutine(HintRoutine(hintDelay));
    }

    protected void StopHintRoutine()
    {
        if (_hintRoutine != null)
        {
            StopCoroutine(_hintRoutine);
        }
        _hintRoutine = null;
    }

    protected IEnumerator HintRoutine(float hintDelay)
    {
        yield return new WaitForSeconds(hintDelay);

        _hintRoutine = null;

        List<Vector2Int> hintIndices = FindHintIndices();
        if (hintIndices.Count > 0)
        {
            StartShaking(hintIndices);

            StartHintRoutine();
        }
    }

    protected bool IsShaking(int r, int c)
    {
        if (_gems == null)
        {
            return false;
        }

        Gem gem = _gems[r, c];
        if (gem == null)
        {
            return false;
        }

        GemShake gemShake = gem.GetComponentInChildren<GemShake>();

        if (gemShake == null)
        {
            return false;
        }

        return gemShake.IsShaking;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin ||
            phase == TransitionPhase.MiddleTransitionStarts ||
            phase == TransitionPhase.EndTransitionBegin)
        {
            EnableCover(phase != TransitionPhase.EndTransitionBegin);
        }
    }
}
