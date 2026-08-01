// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardChestSpawner.cs
// Summary: Spawns the reward chest prefab when the reward transition begins.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class RewardChestSpawner : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected GameObject chestPrefab;
    [SerializeField] protected Button[] rewardButtons;
    [SerializeField] protected CanvasGroup rewardButtonsGroup;

    [SerializeField] protected Vector3 landscapeSpawnPosition;
    [SerializeField] protected Vector3 portraitSpawnPosition;

    protected Vector3 _spawnOffsetToCamera;

    protected void Awake()
    {
        bool isPortrait = Screen.height > Screen.width;
        Vector3 spawnPosition = isPortrait ? portraitSpawnPosition : landscapeSpawnPosition;

        CameraOrientationSetter cameraOrientationSetter = Camera.main.GetComponent<CameraOrientationSetter>();
        Vector3 cameraPosition = cameraOrientationSetter != null ? cameraOrientationSetter.OriginalPosition : Camera.main.transform.position;
        _spawnOffsetToCamera = spawnPosition - cameraPosition;
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.RewardTransitionStarts)
        {
            SpawnChest();
        }
    }

    protected void SpawnChest()
    {
        Vector3 pos = _spawnOffsetToCamera + Camera.main.transform.position;
        GameObject chestInstance = Instantiate(chestPrefab, pos, Quaternion.identity);
        chestInstance.GetComponent<RewardChest>().Initialize(rewardButtons, rewardButtonsGroup);
    }
}
