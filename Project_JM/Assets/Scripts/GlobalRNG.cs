// SPDX-License-Identifier: MIT
// Copyright (c) 11/10/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GlobalRNG.cs
// Summary: A global system for RNG.

using UnityEngine;
using System;

[CreateAssetMenu(fileName = "GlobalRNG", menuName = "JM/Development/GlobalRNG")]
public class GlobalRNG : ScriptableObject
{
    protected static GlobalRNG _instance;
    public static GlobalRNG Instance
    {
        get
        {
            if(_instance == null )
            {
                _instance = Resources.Load<GlobalRNG>("GlobalRNG");
            }
            return _instance;
        }
    }

    [SerializeField] protected int _seed = 12345;
    protected System.Random _rng;

    protected void OnEnable()
    {
        _rng = new System.Random(_seed);
    }

    public int NextInt(int max) => NextInt(0, max);
    public int NextInt(int min, int max) => _rng.Next(min, max);
    // Returns [0.0, 1.0)
    public float NextFloat() => (float)_rng.NextDouble();
    public float NextFloat(float min, float max)
    {
        return ((float)_rng.NextDouble() * (max - min)) - min;
    }
    public void Reseed(int newSeed)
    {
        _seed = newSeed;
        _rng = new System.Random(_seed);
    }
}
