// SPDX-License-Identifier: MIT
// Copyright (c) 11/03/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Gem.cs
// Summary: A script for gem.

using System;
using UnityEngine;
using GemEnums;

[RequireComponent(typeof(GemMover))]
public class Gem : MonoBehaviour
{
    public GemColor Color { get; private set; }

    public void Init(GemColor gemColor)
    {
        Color = gemColor;
        GetComponent<SpriteRenderer>().color = GemColorUtility.ConvertGemColor(gemColor);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
