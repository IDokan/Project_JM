// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardOfferUI.cs
// Summary: Rolls a reward offer from RewardManager and reveals it as
//          selectable buttons, tinted and iconed per reward, at the start of
//          the reward transition. Applies whichever reward the player picks
//          through RewardManager and raises TransitionPhase.RewardChosen.
//          Owns the reward buttons so RewardChest can stay a purely visual
//          prop.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardOfferUI : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected TransitionManager transitionManager;
    [SerializeField] protected RewardManager rewardManager;
    [SerializeField] protected RewardChest chest;

    [SerializeField] protected Button[] rewardButtons;
    [SerializeField] protected RewardIconGroup[] rewardIconGroups;
    [SerializeField] protected CanvasGroup rewardButtonsGroup;
    [SerializeField] protected float revealDelay = 1f;

    [Header("Particles")]
    // One pre-placed mover per button slot, reused for every reward offer —
    // the chest itself is a single persistent scene object (see
    // RewardChest), so these are scene-authored the same way rather than
    // instantiated/destroyed at runtime. Position doesn't matter where you
    // place them; SpawnParticlesAfterDelay repositions each to the chest
    // before every use.
    [SerializeField] protected RewardParticleMover[] particleMovers;
    [SerializeField] protected float particleLaunchDelay = 0.6f;

    protected UnityAction[] _buttonListeners;
    protected RewardDefinition[] _currentOffer;
    protected Coroutine _revealRoutine;
    protected Coroutine _particleSpawnRoutine;

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

        if (_particleSpawnRoutine != null)
        {
            StopCoroutine(_particleSpawnRoutine);
            _particleSpawnRoutine = null;
        }
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.RewardTransitionStarts)
        {
            BindOffer();
            _revealRoutine = StartCoroutine(RevealAfterDelay());
            _particleSpawnRoutine = StartCoroutine(SpawnParticlesAfterDelay());
        }
    }

    protected void BindOffer()
    {
        _currentOffer = rewardManager.RollOffer();

        int count = Mathf.Min(rewardButtons.Length, _currentOffer.Length);
        for (int i = 0; i < count; i++)
        {
            GemColor color = _currentOffer[i].AssociatedColor;
            rewardButtons[i].targetGraphic.color = color.GetSoftenedGemColor();
            rewardIconGroups[i].SetIcons(_currentOffer[i].Icons);
        }
    }

    protected IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSeconds(revealDelay);

        transitionManager.SetSkipHoldBlocked(true);
        SetButtonsVisible(true);
        _revealRoutine = null;
    }

    protected IEnumerator SpawnParticlesAfterDelay()
    {
        yield return new WaitForSeconds(particleLaunchDelay);

        int count = Mathf.Min(Mathf.Min(rewardButtons.Length, particleMovers.Length), _currentOffer.Length);
        for (int i = 0; i < count; i++)
        {
            particleMovers[i].transform.position = chest.transform.position;
            particleMovers[i].Init(rewardButtons[i].GetComponent<RectTransform>(), chest.transform.position.z);
        }

        _particleSpawnRoutine = null;
    }

    protected void OnRewardButtonPressed(int index)
    {
        transitionManager.SetSkipHoldBlocked(false);
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
