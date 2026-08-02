// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardChestManager.cs
// Summary: Owns all transition-event handling for the reward chest prop:
//          shows it at the reward transition's spawn position on
//          RewardTransitionStarts, and hides it on MiddleTransitionStarts.
//          Stays active for the whole scene (unlike the chest itself) so it
//          can always hear RewardTransitionStarts and bring the chest back,
//          even between uses when the chest is inactive.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class RewardChestManager : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [SerializeField] protected RewardChest chest;

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
            Vector3 pos = _spawnOffsetToCamera + Camera.main.transform.position;
            chest.Show(pos);
        }
        else if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            chest.Hide();
        }
    }
}
