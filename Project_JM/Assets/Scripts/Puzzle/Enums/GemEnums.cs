// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/04/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemEnums.cs
// Summary: A script that has enums for gem.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections.Generic;
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
                default: return new Color(1f, 0f, 236f / 255f);
            }
        }

        public static GemColor GetRandomGemColor(bool isUsingSystemRand = false)
        {
            return PlayableGemColor[RandomInt(PlayableGemColor.Length, isUsingSystemRand)];
        }

        public static GemColor GetRandomGemColorExcept(params GemColor[] excludeColors)
        {
            HashSet<GemColor> exclude = (excludeColors == null || excludeColors.Length == 0)
                ? null
                : new HashSet<GemColor>(excludeColors);

            List<GemColor> allowed = new List<GemColor>(PlayableGemColor.Length);
            for (int i = 0; i < PlayableGemColor.Length; ++i)
            {
                GemColor c = PlayableGemColor[i];
                if (exclude == null || exclude.Contains(c) == false)
                {
                    allowed.Add(c);
                }
            }

            if (allowed.Count <= 0)
            {
                return GemColor.None;
            }
            return allowed[RandomInt(allowed.Count, false)];
        }

        public static GemColor GetRandomGemColorExcept(GemColor colorExclude, bool isUsingSystemRand = false)
        {
            List<GemColor> allowed = new List<GemColor>(PlayableGemColor.Length);
            for (int i = 0; i < PlayableGemColor.Length; ++i)
            {
                GemColor c = PlayableGemColor[i];
                if (c != colorExclude)
                {
                    allowed.Add(c);
                }
            }

            return allowed[RandomInt(allowed.Count, isUsingSystemRand)];
        }

        public static float GetGemColorDamageMultiplier(GemColor[] attackerColor, GemColor[] targetColor)
        {
            if (attackerColor == null || targetColor == null ||
                attackerColor.Length <= 0 || targetColor.Length <= 0)
            {
                Debug.LogError("GetGemColorDamageMultiplier: any of given array is null or empty.");
                return 1f;
            }

            for (int i = 0; i < attackerColor.Length; i++)
            {
                for (int j = 0; j < targetColor.Length; j++)
                {
                    if (attackerColor[i] == targetColor[j])
                    {
                        return 1.2f;
                    }
                }
            }

            return 0.8f;
        }

        public static int RandomInt(int max, bool isUsingSystemRand)
        {
            return isUsingSystemRand ?
                UnityEngine.Random.Range(0, max) : GlobalRNG.Instance.NextInt(max);
        }

        public static GemColor GetNextGemColor(GemColor gemColor)
        {
            for (int i = 0; i < PlayableGemColor.Length; i++)
            {
                if (gemColor == PlayableGemColor[i])
                {
                    return PlayableGemColor[(i + 1) % PlayableGemColor.Length];
                }
            }

            return PlayableGemColor[0];
        }
    }

}