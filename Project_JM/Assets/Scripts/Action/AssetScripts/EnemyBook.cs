// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/14/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyBook.cs
// Summary: A scriptable object that holds enemy prefabs grouped by difficulty tier and manages spawn progression.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyBook", menuName = "JM/Data/EnemyBook")]
public class EnemyBook : ScriptableObject
{
    [SerializeField] private GameObject[] easyEnemies;
    [SerializeField] private GameObject[] mediumEnemies;
    [SerializeField] private GameObject[] hardEnemies;

    private GameObject[][] _groups;
    private int _currentGroupIndex;
    private readonly List<int> _remainingInGroup = new List<int>();

    private void OnEnable()
    {
        _groups = new[] { easyEnemies, mediumEnemies, hardEnemies };
        ResetProgression();
    }

    public void ResetProgression()
    {
        _currentGroupIndex = 0;
        FillRemainingFromCurrentGroup();
        SkipEmptyGroups();
    }

    public GameObject GetNextEnemy()
    {
        if (_remainingInGroup.Count == 0)
        {
            AdvanceToNextGroup();
        }

        int pick = Random.Range(0, _remainingInGroup.Count);
        int enemyIndex = _remainingInGroup[pick];
        _remainingInGroup.RemoveAt(pick);

        return _groups[_currentGroupIndex][enemyIndex];
    }

    private void FillRemainingFromCurrentGroup()
    {
        _remainingInGroup.Clear();
        GameObject[] group = _groups[_currentGroupIndex];
        for (int i = 0; i < group.Length; i++)
        {
            _remainingInGroup.Add(i);
        }
    }

    private void AdvanceToNextGroup()
    {
        _currentGroupIndex = Mathf.Min(_currentGroupIndex + 1, _groups.Length - 1);
        FillRemainingFromCurrentGroup();
        SkipEmptyGroups();
    }

    private void SkipEmptyGroups()
    {
        while (_remainingInGroup.Count == 0 && _currentGroupIndex < _groups.Length - 1)
        {
            _currentGroupIndex++;
            FillRemainingFromCurrentGroup();
        }
    }
}
