// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardChest.cs
// Summary: Opens after a short delay to reveal reward buttons, raises
//          RewardChosenEventChannel once the player picks one, and moves
//          itself off-screen once the middle transition begins.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RewardChest : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected RewardChosenEventChannel rewardChosenEventChannel;

    [SerializeField] protected float openDelay = 1f;
    [SerializeField] protected float exitDistance = 10f;
    [SerializeField] protected float exitDuration = 1f;

    protected Button[] _rewardButtons;
    protected CanvasGroup _rewardButtonsGroup;

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
        StartCoroutine(OpenAfterDelay());
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;

        if (_rewardButtons != null)
        {
            for (int i = 0; i < _rewardButtons.Length; i++)
            {
                _rewardButtons[i].onClick.RemoveListener(OnRewardButtonPressed);
            }
        }
    }

    // Called by RewardChestSpawner right after Instantiate, since the reward
    // buttons live in the scene's UI canvas rather than under this prefab.
    public void Initialize(Button[] rewardButtons, CanvasGroup rewardButtonsGroup)
    {
        _rewardButtons = rewardButtons;
        _rewardButtonsGroup = rewardButtonsGroup;

        for (int i = 0; i < _rewardButtons.Length; i++)
        {
            _rewardButtons[i].onClick.AddListener(OnRewardButtonPressed);
        }

        SetButtonsVisible(false);
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            StartCoroutine(ExitRoutine());
        }
    }

    protected IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(openDelay);

        SetButtonsVisible(true);
    }

    protected void OnRewardButtonPressed()
    {
        SetButtonsVisible(false);

        rewardChosenEventChannel.Raise();
    }

    protected void SetButtonsVisible(bool visible)
    {
        _rewardButtonsGroup.alpha = visible ? 1f : 0f;
        _rewardButtonsGroup.interactable = visible;
        _rewardButtonsGroup.blocksRaycasts = visible;
    }

    protected IEnumerator ExitRoutine()
    {
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * exitDistance;

        float t = 0f;
        while (t < exitDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t / exitDuration);
            yield return null;
        }

        transform.position = end;
    }
}
