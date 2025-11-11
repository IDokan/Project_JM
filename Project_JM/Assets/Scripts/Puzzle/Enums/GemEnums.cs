// SPDX-License-Identifier: MIT
// Copyright (c) 11/04/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemEnums.cs
// Summary: A script that has enums for gem.

using System;
using UnityEngine;

namespace GemEnums
{
    public enum GemColor
    {
        None,
        Red,
        Green,
        Blue,
        Yellow,
    }

    public static class GemColorUtility
    {

        public static readonly GemColor[] PlayableGemColor =
        {
        GemColor.Red,
        GemColor.Green,
        GemColor.Blue,
        GemColor.Yellow,
        };

        public static Color ConvertGemColor(this GemColor color)
        {
            switch (color)
            {
                case GemColor.Red: return Color.red;
                case GemColor.Green: return Color.green;
                case GemColor.Blue: return Color.blue;
                case GemColor.Yellow: return Color.yellow;
                default: return Color.black;
            }
        }

        public static GemColor GetRandomGemColor()
        {
            return PlayableGemColor[GlobalRNG.Instance.NextInt(PlayableGemColor.Length)];
        }

        public static GemColor GetRandomGemColorExcept(GemColor excludeColor)
        {
            GemColor color;
            do
            {
                color = PlayableGemColor[GlobalRNG.Instance.NextInt(PlayableGemColor.Length)];
            }
            while (color == excludeColor);

            return color;
        }
    }

}