// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: JewelRecordManager.cs
// Summary: Accumulates gem match counts by color and tier, then flushes the totals
//          into SaveDataManager whenever any character dies.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using GemEnums;
using MatchEnums;
using UnityEngine;

public class JewelRecordManager : MonoBehaviour
{
    [SerializeField] protected MatchEventChannel matchEvents;
    [SerializeField] protected CharacterDeathEventChannel deathChannel;

    protected readonly Dictionary<(GemColor, MatchTier), int> _matchCountDelta = new Dictionary<(GemColor, MatchTier), int>();
    protected int _noneMatchCountDelta;

    protected void OnEnable()
    {
        matchEvents.OnRaised += OnMatch;
        deathChannel.OnRaised += OnCharacterDied;
    }

    protected void OnDisable()
    {
        matchEvents.OnRaised -= OnMatch;
        deathChannel.OnRaised -= OnCharacterDied;
    }

    protected void OnMatch(MatchEvent matchEvent)
    {
        if (matchEvent.Color == GemColor.None)
        {
            _noneMatchCountDelta++;
            return;
        }

        (GemColor, MatchTier) key = (matchEvent.Color, matchEvent.Tier);
        _matchCountDelta.TryGetValue(key, out int current);
        _matchCountDelta[key] = current + 1;
    }

    protected void OnCharacterDied(CharacterStatus stat)
    {
        foreach (KeyValuePair<(GemColor, MatchTier), int> entry in _matchCountDelta)
        {
            if (entry.Value <= 0)
            {
                continue;
            }

            SaveDataManager.Instance.AddJewelMatchCount(entry.Key.Item1, entry.Key.Item2, entry.Value);
        }
        _matchCountDelta.Clear();

        if (_noneMatchCountDelta > 0)
        {
            SaveDataManager.Instance.AddJewelMatchCount(GemColor.None, default, _noneMatchCountDelta);
            _noneMatchCountDelta = 0;
        }
    }
}
