using GemEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] protected int _rows = 8;
    [SerializeField] protected int _cols = 8;
    [SerializeField] protected float _cellSize = 1f;
    [SerializeField] protected float _spacing = 0.05f;
    [SerializeField] protected GameObject _gemPrefab;
    protected Gem[,] _gems;

    protected static readonly GemColor[] PlayableGemColor =
    {
        GemColor.Red,
        GemColor.Green,
        GemColor.Blue,
        GemColor.Yellow,
    };

    [SerializeField] public int initialSeed = 12345;
    protected const int MaxResolveIterations = 100;
    protected System.Random _random;

    private bool _isResolving = false;
    private int _numMovingGems = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateBoard();
        ResolveMatches();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        SetRandomSeed(initialSeed);
    }

    protected void GenerateBoard()
    {
        _gems = new Gem[_rows, _cols];

        for (int r = 0; r < _rows; r++)
        {
            for (int c = 0; c < _cols; c++)
            {
                _gems[r, c] = GetRandomGem(r, c);
            }
        }
    }

    protected Gem GetRandomGem(int row, int col)
    {
        GameObject gemObj = Instantiate(_gemPrefab, transform);
        Vector2 gemLocalPos = GetGemLocation(row, col);
        gemObj.transform.localPosition = gemLocalPos;
        Gem gem = gemObj.GetComponent<Gem>();
        GemColor color = PlayableGemColor[_random.Next(PlayableGemColor.Length)];
        gem.Init(color);

        return gem;
    }

    public void SetRandomSeed(int seed)
    {
        _random = new System.Random(seed);
    }

    // A function that resolve matches only when board initially generated.
    protected bool ResolveMatches   ()
    {
        bool hasAnyMatch = false;

        var matches = FindMatches();

        if(matches.Count == 0)
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
                        StartCoroutine(MoveGem(_gems[writeRow, col], writeRow, col));
                        _numMovingGems++;
                    }

                    writeRow++;
                }
            }
        }
    }

    protected void RefillBoard()
    {
        for (int row = 0; row < _rows; row++)
        {
            for (int col = 0; col < _cols; col++)
            {
                if (_gems[row, col] == null)
                {
                    // @@ TODO: It instantiate a new game object but ApplyGravity swap only color.
                    // @@@@@@ Thus, GC remove only top gems and instantiate it to top.

                    // @@ TODO: Add additional logic to refill gravity logic.
                    _gems[row, col] = GetRandomGem(row, col);
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
                            seen[row + offset, col] = true; ;
                        }
                    }
                }
            }
        }

        return matches;
    }

    protected IEnumerator MoveGem(Gem gem, int newRow, int newCol)
    {
        Vector2 targetLocation = GetGemLocation(newRow, newCol);
        while (Vector2.Distance(gem.transform.localPosition, targetLocation) > 0.01f)
        {
            gem.transform.localPosition = Vector2.Lerp(gem.transform.localPosition, targetLocation, Time.deltaTime);
            yield return null;
        }

        gem.transform.localPosition = targetLocation;

        // @@ TODO: Is this okay? Need to check..
        if(--_numMovingGems >= 0)
        {
            _isResolving = false;
        }

        RequestResolve();
    }

    protected Vector2 GetGemLocation(int row, int col)
    {
        return new Vector2(col * (_cellSize + _spacing), row * (_cellSize + _spacing));
    }

    protected void ResolveGem(int row, int col)
    {
        // @@ TODO: Improve resolving method to look much fancier.
        // @@ TODO: Implement object pool for gems.

        Destroy(_gems[row, col].gameObject);
        _gems[row, col] = null;
    }

    protected bool IsMatchAt(int row, int col)
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

        c = col + 1;
        while (c < _cols)
        {
            if (_gems[row, c].color == color)
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

        r = row + 1;
        while (r < _rows)
        {
            if(_gems[r, col].color == color)
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

    protected void RequestResolve()
    {
        if(!_isResolving)
        {
            ResolveMatches();
        }
    }
}
