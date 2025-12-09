// SPDX-License-Identifier: MIT
// Copyright (c) 11/20/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SystemBehaviour.cs
// Summary: A script for manager of system. It does lots of helper things for system and manager classes.

using MatchEnums;
using UnityEngine;

public class SystemBehaviour : MonoBehaviour
{
    [SerializeField] protected MatchEventChannel _matchEvents;
    [SerializeField] protected CombatManager _combatManager;
    [SerializeField] protected ComboManager _comboManager;

    private void OnEnable()
    {
        _matchEvents.OnRaised += OnMatch;
    }

    private void OnDisable()
    {
        _matchEvents.OnRaised -= OnMatch;
    }

    private void OnMatch(MatchEvent matchEvent)
    {
        _combatManager.OnMatch(matchEvent);
        _comboManager.OnMatch(matchEvent);
    }
}
