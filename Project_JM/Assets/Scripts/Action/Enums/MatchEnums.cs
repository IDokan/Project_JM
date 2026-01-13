// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MatchEvent.cs
// Summary: A script for match enums.

using UnityEngine;
using GemEnums;

namespace MatchEnums
{
    public enum MatchTier
    {
        Three = 3,
        Four = 4,
        Five = 5
    }

    public struct MatchEvent
    {
        public GemColor Color;      // which character should act
        public MatchTier Tier;      // 3/4/5
    }
    public static class MatchTierUtil
    {
        public static MatchTier GetMatchTier(int target)
        {
            if (target <= 3) return MatchTier.Three;
            if (target == 4) return MatchTier.Four;
            return MatchTier.Five;
        }
    }
}