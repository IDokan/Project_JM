// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardOfferUI.cs
// Summary: Rolls a reward offer from RewardManager and reveals it as
//          selectable buttons at the start of the reward transition. Applies
//          whichever reward the player picks through RewardManager and
//          raises TransitionPhase.RewardChosen. Owns the reward buttons so
//          RewardChest can stay a purely visual prop.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardOfferUI : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected RewardManager rewardManager;

    [SerializeField] protected Button[] rewardButtons;
    [SerializeField] protected CanvasGroup rewardButtonsGroup;
    [SerializeField] protected float revealDelay = 1f;

    protected UnityAction[] _buttonListeners;
    protected RewardDefinition[] _currentOffer;
    protected Coroutine _revealRoutine;

    protected void Awake()
    {
        SetButtonsVisible(false);
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;

        // Bound once per enable, by index, against rewardButtons.Length
        // rather than the offer size rolled later — offerCount is expected
        // to match the button count. Looking the reward up from
        // _currentOffer at click time (instead of capturing it) means
        // BindOffer doesn't need to touch onClick at all when a new offer
        // is rolled.
        _buttonListeners = new UnityAction[rewardButtons.Length];
        for (int i = 0; i < rewardButtons.Length; i++)
        {
            int index = i;
            UnityAction listener = () => OnRewardButtonPressed(index);
            _buttonListeners[i] = listener;
            rewardButtons[i].onClick.AddListener(listener);
        }
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;

        for (int i = 0; i < _buttonListeners.Length; i++)
        {
            rewardButtons[i].onClick.RemoveListener(_buttonListeners[i]);
        }
        _buttonListeners = null;

        if (_revealRoutine != null)
        {
            StopCoroutine(_revealRoutine);
            _revealRoutine = null;
        }
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.RewardTransitionStarts)
        {
            BindOffer();
            _revealRoutine = StartCoroutine(RevealAfterDelay());
        }
    }

    protected void BindOffer()
    {
        _currentOffer = rewardManager.RollOffer();

        int count = Mathf.Min(rewardButtons.Length, _currentOffer.Length);
        for (int i = 0; i < count; i++)
        {
            GemColor color = _currentOffer[i].AssociatedColor;
            rewardButtons[i].targetGraphic.color = color != GemColor.None ? color.ConvertGemColor() : Color.white;
        }
    }

    protected IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSeconds(revealDelay);

        SetButtonsVisible(true);
        _revealRoutine = null;
    }

    protected void OnRewardButtonPressed(int index)
    {
        SetButtonsVisible(false);

        rewardManager.ChooseReward(_currentOffer[index]);
        transitionEventChannel.Raise(TransitionPhase.RewardChosen);
    }

    protected void SetButtonsVisible(bool visible)
    {
        rewardButtonsGroup.alpha = visible ? 1f : 0f;
        rewardButtonsGroup.interactable = visible;
        rewardButtonsGroup.blocksRaycasts = visible;
    }
}
