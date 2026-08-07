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
using RewardEnums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RewardOfferUI : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected TransitionManager transitionManager;
    [SerializeField] protected RewardManager rewardManager;
    [SerializeField] protected RewardChest chest;
    [SerializeField] protected PartyRoster partyRoster;

    [SerializeField] protected Button[] rewardButtons;
    [SerializeField] protected RewardIconGroup[] rewardIconGroups;
    [SerializeField] protected CanvasGroup rewardButtonsGroup;

    // How long the group's alpha takes to reach 0 once a reward is picked.
    [SerializeField] protected float pickedFadeOutDuration = 0.3f;

    [Header("Particles")]
    // One pre-placed mover per button slot, reused for every reward offer —
    // the chest itself is a single persistent scene object (see
    // RewardChest), so these are scene-authored the same way rather than
    // instantiated/destroyed at runtime. Position doesn't matter where you
    // place them; SpawnParticlesAfterDelay repositions each to the chest
    // before every use.
    [SerializeField] protected RewardParticleMover[] particleMovers;
    [SerializeField] protected float particleLaunchDelay = 0.6f;

    // One pre-placed mover per button slot, mirroring particleMovers above —
    // reused across every reward offer rather than instantiated per pick.
    // Position doesn't matter where placed; OnRewardButtonPressed
    // repositions the picked slot's mover onto that button before use. The
    // target(s) aren't fixed — see ResolveApplyTargets: colored rewards
    // resolve their character via partyRoster (same as Gem.Resolve does for
    // GemResolver), colorless ones route by RewardId instead.
    [SerializeField] protected RewardApplyParticleMover[] applyParticleMovers;

    // One pre-placed mover per button slot, index-aligned with
    // applyParticleMovers — each lives on the same GameObject as its
    // applyParticleMovers counterpart, sharing its ParticleSystem, since the
    // two are mutually exclusive per offer (a button either gets picked and
    // applies, or doesn't and dismisses).
    [SerializeField] protected RewardDismissParticleMover[] dismissParticleMovers;

    // Convergence points for colorless rewards, whose AssociatedColor can't
    // resolve a character via partyRoster. See ResolveApplyTargets.
    [SerializeField] protected RectTransform hpIconTarget;
    [SerializeField] protected RectTransform comboIconTarget;

    protected UnityAction[] _buttonListeners;
    protected RewardDefinition[] _currentOffer;
    protected Coroutine _revealRoutine;
    protected Coroutine _particleSpawnRoutine;
    protected Coroutine _fadeOutRoutine;

    // Which targets the pick currently in flight should flash once
    // RaiseRewardGiven fires, from mover.Completed.
    protected Transform[] _pendingTargets;
    protected bool _rewardGivenFired;

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

        // applyParticleMovers are scene-persistent (reused every offer, not
        // instantiated/destroyed per pick like GemResolver), so it's safe to
        // subscribe every slot up front rather than per pick — at most one
        // is ever actually traveling (Init'd) at a time, so RaiseRewardGiven
        // only ever fires for the pick _pendingTargets currently belongs to.
        for (int i = 0; i < applyParticleMovers.Length; i++)
        {
            applyParticleMovers[i].Completed += RaiseRewardGiven;
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

        for (int i = 0; i < applyParticleMovers.Length; i++)
        {
            applyParticleMovers[i].Completed -= RaiseRewardGiven;
        }

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

        if (_fadeOutRoutine != null)
        {
            StopCoroutine(_fadeOutRoutine);
            _fadeOutRoutine = null;
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
            Color softened = color.GetSoftenedGemColor();

            rewardButtons[i].targetGraphic.color = softened;
            rewardIconGroups[i].SetIcons(_currentOffer[i].Icons);

            ColorBlock colors = rewardButtons[i].colors;
            colors.normalColor = softened;
            colors.disabledColor = softened;
            rewardButtons[i].colors = colors;
        }
    }

    // Fades rewardButtonsGroup.alpha in from 0, timed against the chest→
    // button swarm's own arrival window (particleMovers[0].MinTravelDuration
    // to .MaxTravelDuration, both measured from the same particleLaunchDelay
    // SpawnParticlesAfterDelay waits on) rather than a flat reveal delay —
    // held at 0 until the earliest particle could land, then ramping to 1 by
    // the time the slowest one does, so the buttons visibly materialize
    // alongside the swarm converging on them. Only the first slot's mover is
    // sampled since every slot shares the same authored travel timing; the
    // group fades as one.
    protected IEnumerator RevealAfterDelay()
    {
        yield return new WaitForSeconds(particleLaunchDelay);

        float minDuration = particleMovers.Length > 0 ? particleMovers[0].MinTravelDuration : 0f;
        float maxDuration = particleMovers.Length > 0 ? particleMovers[0].MaxTravelDuration : 0f;

        rewardButtonsGroup.alpha = 0f;

        if (minDuration > 0f)
        {
            yield return new WaitForSeconds(minDuration);
        }

        float rampDuration = maxDuration - minDuration;
        if (rampDuration > 0f)
        {
            float elapsed = 0f;
            while (elapsed < rampDuration)
            {
                elapsed += Time.deltaTime;
                rewardButtonsGroup.alpha = Mathf.Clamp01(elapsed / rampDuration);
                yield return null;
            }
        }

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
            particleMovers[i].Init(rewardButtons[i].GetComponent<RectTransform>(), _currentOffer[i].AssociatedColor, chest.transform.position.z);
        }

        _particleSpawnRoutine = null;
    }

    protected void OnRewardButtonPressed(int index)
    {
        transitionManager.SetSkipHoldBlocked(false);

        rewardButtonsGroup.interactable = false;
        rewardButtonsGroup.blocksRaycasts = false;

        if (_fadeOutRoutine != null)
        {
            StopCoroutine(_fadeOutRoutine);
        }
        _fadeOutRoutine = StartCoroutine(FadeOutButtons());

        (Transform[] targets, GemColor[] colors) = index < applyParticleMovers.Length ? ResolveApplyTargets(_currentOffer[index]) : (null, null);
        RewardApplyParticleMover mover = null;
        if (targets != null && targets.Length > 0)
        {
            RectTransform buttonRect = rewardButtons[index].GetComponent<RectTransform>();
            mover = applyParticleMovers[index];
            mover.Init(buttonRect, targets, colors, chest.transform.position.z);
        }

        DismissUnpickedButtons(index);

        rewardManager.ChooseReward(_currentOffer[index]);
        transitionEventChannel.Raise(TransitionPhase.RewardChosen);

        _rewardGivenFired = false;
        _pendingTargets = targets;

        // mover == null means no particle swarm will ever finish for this
        // pick (no mover slot, or no resolvable target) — RewardTransitionController
        // only advances on RewardGiven, so this pick would otherwise softlock
        // the reward screen forever. Nothing to wait for in that case, so
        // fire immediately; otherwise mover.Completed (subscribed in
        // OnEnable) fires RaiseRewardGiven once the swarm actually finishes.
        if (mover == null)
        {
            RaiseRewardGiven();
        }
    }

    // Fades rewardButtonsGroup.alpha from 1 to 0 over pickedFadeOutDuration,
    // starting the instant a reward is picked.
    protected IEnumerator FadeOutButtons()
    {
        rewardButtonsGroup.alpha = 1f;

        float elapsed = 0f;
        while (elapsed < pickedFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            rewardButtonsGroup.alpha = 1f - Mathf.Clamp01(elapsed / pickedFadeOutDuration);
            yield return null;
        }

        SetButtonsVisible(false);
        _fadeOutRoutine = null;
    }

    // Bursts every other slot's dismiss mover in place on its own button,
    // skipping pickedIndex — the picked button's swarm instead travels via
    // applyParticleMovers, handled separately in OnRewardButtonPressed.
    protected void DismissUnpickedButtons(int pickedIndex)
    {
        int count = Mathf.Min(Mathf.Min(rewardButtons.Length, dismissParticleMovers.Length), _currentOffer.Length);
        for (int i = 0; i < count; i++)
        {
            if (i == pickedIndex)
            {
                continue;
            }

            RectTransform buttonRect = rewardButtons[i].GetComponent<RectTransform>();
            dismissParticleMovers[i].Init(buttonRect, _currentOffer[i].AssociatedColor, chest.transform.position.z);
        }
    }

    // Fires once per pick, from mover.Completed. _rewardGivenFired guards
    // against a mover somehow firing Completed twice for the same pick.
    protected void RaiseRewardGiven()
    {
        if (_rewardGivenFired)
        {
            return;
        }
        _rewardGivenFired = true;

        FlashAbsorbedCharacters(_pendingTargets);
        transitionEventChannel.Raise(TransitionPhase.RewardGiven);
    }

    // Flashes whichever targets turn out to be characters, through the same
    // GemPowerGlowEffect.PlayGlow() gem-power absorption already uses — HUD
    // icon targets (HP/combo, for Blessings/Fortify/Focus) simply have no
    // GemPowerGlowEffect to find there and are skipped.
    protected void FlashAbsorbedCharacters(Transform[] targets)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            GemPowerGlowEffect glow = targets[i] ? targets[i].GetComponentInParent<GemPowerGlowEffect>() : null;
            glow?.PlayGlow();
        }
    }

    // Colored rewards (PowerUp/SharpAttack) fly to their matching character
    // via partyRoster, same lookup Gem.Resolve uses for GemResolver, tinted
    // with that same color. Colorless rewards (AssociatedColor ==
    // GemColor.None) instead route by RewardId: Berserked splits the swarm
    // evenly across every party member, each quarter tinted with that
    // character's own color (partyRoster.GetAllCharacterColors is
    // index-aligned with GetAllCharacterTransforms); Blessings/Fortify
    // converge on the HP icon and Focus on the combo icon, both untinted
    // (GemColor.None → white).
    protected (Transform[] targets, GemColor[] colors) ResolveApplyTargets(RewardDefinition reward)
    {
        switch (reward.Id)
        {
            case RewardId.Berserked:
                return (partyRoster.GetAllCharacterTransforms(), partyRoster.GetAllCharacterColors());

            case RewardId.Blessings:
            case RewardId.Fortify:
                return (new Transform[] { hpIconTarget }, new[] { GemColor.None });

            case RewardId.Focus:
                return (new Transform[] { comboIconTarget }, new[] { GemColor.None });

            default:
                Transform target = partyRoster.GetCharacterTransform(reward.AssociatedColor);
                return target ? (new[] { target }, new[] { reward.AssociatedColor }) : (null, null);
        }
    }

    protected void SetButtonsVisible(bool visible)
    {
        rewardButtonsGroup.alpha = visible ? 1f : 0f;
        rewardButtonsGroup.interactable = visible;
        rewardButtonsGroup.blocksRaycasts = visible;
    }
}
