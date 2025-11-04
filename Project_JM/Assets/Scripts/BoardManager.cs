using GemEnums;
using System;
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
        Vector2 gemLocalPos = new Vector2(col * (_cellSize + _spacing), row * (_cellSize + _spacing));
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

    protected bool ResolveMatches()
    {
        bool hasAnyMatch = false;

        // I want expression that gems spawns and resolved gradually in visual. This code may not be the proper way.
        for (int iteration = 0; iteration < MaxResolveIterations; iteration++)
        {
            var matches = FindMatches();
            if (matches.Count == 0)
            {
                break;
            }

            hasAnyMatch = true;

            foreach (var (row, col) in matches)
            {
                // @@ TODO: Improve resolving method.
                // @@ 1) I want to design gravity falling effects

                // resolve gems
                _gems[row, col].Init(GemColor.None);
            }

            ApplyGravity();
            RefillBoard();

            if (iteration == MaxResolveIterations - 1 && FindMatches().Count > 0)
            {
                throw new InvalidOperationException("Exceeded maximum resolve iterations. Board may be stuck in a loop.");
            }
        }

        return hasAnyMatch;
    }

    protected void ApplyGravity()
    {
        for (int col = 0; col < _cols; col++)
        {
            int writeRow = _rows - 1;
            for (int row = _rows - 1; row >= 0; row--)
            {
                if (_gems[row, col].color == GemColor.None)
                {
                    if (_gems[row, col].color == GemColor.None)
                    {
                        continue;
                    }

                    if (row != writeRow)
                    {
                        _gems[writeRow, col] = _gems[row, col];
                        _gems[row, col].color = GemColor.None;
                    }

                    writeRow--;
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
                if (_gems[row, col].color == GemColor.None)
                {
                    // @@ TODO: It instantiate a new game object but ApplyGravity swap only color.
                    // @@@@@@ Thus, GC remove only top gems and instantiate it to top.
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
                if (_gems[row, col].color == GemColor.None)
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
}
