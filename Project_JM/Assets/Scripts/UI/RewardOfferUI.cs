// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardOfferUI.cs
// Summary: Rolls a reward offer from RewardManager and reveals it as
//          selectable buttons, tinted and iconed per reward, once
//          RewardChest.OnLanded fires. Raises TransitionPhase.RewardChosen on
//          pick, then applies the reward through RewardManager once its
//          apply particle swarm arrives, raising TransitionPhase.RewardGiven.
//          Owns the reward buttons so RewardChest can stay a purely visual
//          prop.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using System.Collections;
using GemEnums;
using RewardEnums;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RewardOfferUI : MonoBehaviour, ICancelHandler
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected EnemyAlertEventChannel enemyAlertEventChannel;
    [SerializeField] protected TransitionManager transitionManager;
    [SerializeField] protected RewardManager rewardManager;
    [SerializeField] protected RewardChest chest;
    [SerializeField] protected PartyRoster partyRoster;
    [SerializeField] protected PlayerInput playerInput;
    [SerializeField] protected PauseButton pauseButton;

    [SerializeField] protected Button[] rewardButtons;
    [SerializeField] protected RewardIconGroup[] rewardIconGroups;
    [SerializeField] protected CanvasGroup rewardButtonsGroup;

    // One pre-placed particle system per button slot, index-aligned with
    // rewardButtons — enabled only for the slot(s) whose reward color
    // matches the incoming enemy's color (see _peekedEnemyColors). Applied
    // in RevealAfterDelay once the buttons actually become visible, not at
    // BindOffer — see UpdateColorMatchParticles.
    [SerializeField] protected ParticleSystem[] colorMatchParticles;

    // One pre-placed overlay Image per button slot, inactive until any
    // button is picked — see BindOffer (tinted, reset inactive) and
    // OnRewardButtonPressed (all slots activated together on pick) below.
    [SerializeField] protected Image[] flashImages;

    // How long every button's flash Image stays up before
    // rewardButtonsGroup.alpha is cut to 0.
    [SerializeField] protected float flashDuration = 0.1f;

    [SerializeField] protected AudioCueSO rewardGivenSfx;

    [Header("Particles")]
    // One pre-placed mover per button slot, reused for every reward offer —
    // the chest itself is a single persistent scene object (see
    // RewardChest), so these are scene-authored the same way rather than
    // instantiated/destroyed at runtime. Position doesn't matter where you
    // place them; SpawnParticles repositions each to the chest before every
    // use, once RewardChest.OnLanded fires — see OnChestLanded.
    [SerializeField] protected RewardParticleMover[] particleMovers;

    // Held after RewardChest.OnLanded fires before reacting to it — lets the
    // chest's landing impact read on its own beat before the reward swarm
    // bursts out of it, rather than the two happening in the same frame.
    [SerializeField] protected float postLandDelay = 0.6f;

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
    protected Coroutine _chestLandedRoutine;
    protected Coroutine _fadeOutRoutine;

    protected bool _rewardGivenFired;

    // The reward picked, held until RaiseRewardGiven actually applies it —
    // application is deferred to particle arrival rather than click, so the
    // effect lands in sync with the swarm reaching its target.
    protected RewardDefinition _pendingReward;

    // Cached from enemyAlertEventChannel (see OnEnemyAlert) — an enemy can
    // carry more than one color (dual-type, same as CharacterCombatant.Colors
    // consumed by EnemyAlertUI), so matching is "reward color is one of
    // these", not equality against a single color. Consumed once, in
    // RevealAfterDelay (see UpdateColorMatchParticles), which always runs
    // well after both BindOffer and OnEnemyAlert have fired for the round.
    protected GemColor[] _peekedEnemyColors;

    protected void Awake()
    {
        SetButtonsVisible(false);
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
        enemyAlertEventChannel.OnRaised += OnEnemyAlert;
        chest.OnLanded += OnChestLanded;

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
        // only ever fires for the pick _pendingReward currently belongs to.
        for (int i = 0; i < applyParticleMovers.Length; i++)
        {
            applyParticleMovers[i].Completed += RaiseRewardGiven;
        }
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
        enemyAlertEventChannel.OnRaised -= OnEnemyAlert;
        chest.OnLanded -= OnChestLanded;

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

        if (_chestLandedRoutine != null)
        {
            StopCoroutine(_chestLandedRoutine);
            _chestLandedRoutine = null;
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
        }
        else if (phase == TransitionPhase.RewardChosen)
        {
            playerInput.SwitchToGameplayMap();
        }
    }

    // RewardChest.OnLanded fires the instant the chest reaches its target
    // position (RewardChest.cs), which is the earliest moment
    // chest.transform.position is actually correct — replaces a
    // separately-tuned launch delay here that had to be kept in sync with
    // RewardChest's own entry timing by hand. postLandDelay is a separate,
    // deliberate hold on top of that (see its own field comment).
    protected void OnChestLanded()
    {
        if (_chestLandedRoutine != null)
        {
            StopCoroutine(_chestLandedRoutine);
        }
        _chestLandedRoutine = StartCoroutine(ReactToChestLandedAfterDelay());
    }

    protected IEnumerator ReactToChestLandedAfterDelay()
    {
        yield return new WaitForSeconds(postLandDelay);

        SpawnParticles();
        _revealRoutine = StartCoroutine(RevealAfterDelay());

        _chestLandedRoutine = null;
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

            flashImages[i].color = color.ConvertGemColorOrWhite();
            flashImages[i].gameObject.SetActive(false);
        }

        // Particles are applied later, once the buttons actually become
        // visible — see RevealAfterDelay — not here, so they don't need to
        // race OnEnemyAlert for _peekedEnemyColors.
    }

    protected void UpdateColorMatchParticles()
    {
        int count = Mathf.Min(rewardButtons.Length, _currentOffer.Length);
        for (int i = 0; i < count; i++)
        {
            SetColorMatchParticle(i, _currentOffer[i]);
        }
    }

    // Plays a slot's HintParticle when either its reward's color matches the
    // peeked enemy's color, or RewardManager.IsGameStateRecommended calls it
    // out regardless of color (e.g. Blessings while HP is low, Berserked
    // while still early) — enemies can carry more than one color (dual-type,
    // see _peekedEnemyColors), and GemColor.None (colorless rewards) never
    // matches on color alone, so the state-based hint colors those white.
    protected void SetColorMatchParticle(int index, RewardDefinition reward)
    {
        if (index >= colorMatchParticles.Length || colorMatchParticles[index] == null)
        {
            return;
        }

        GemColor color = reward.AssociatedColor;
        bool matchesEnemyColor = color != GemColor.None
            && _peekedEnemyColors != null
            && Array.IndexOf(_peekedEnemyColors, color) >= 0;

        if (matchesEnemyColor || rewardManager.IsGameStateRecommended(reward))
        {
            ParticleSystem.MainModule main = colorMatchParticles[index].main;
            main.startColor = new ParticleSystem.MinMaxGradient(color.ConvertGemColorOrWhite());
            colorMatchParticles[index].Play();
        }
        else
        {
            colorMatchParticles[index].Stop();
        }
    }

    protected void OnEnemyAlert(GameObject enemyPrefab)
    {
        _peekedEnemyColors = enemyPrefab.TryGetComponent<CharacterCombatant>(out CharacterCombatant combatant)
            ? combatant.Colors
            : null;
    }

    // Fades rewardButtonsGroup.alpha in from 0, timed against the chest→
    // button swarm's own arrival window (particleMovers[0].MinTravelDuration
    // to .MaxTravelDuration, both measured from the same chest-landing
    // instant SpawnParticles is called on, via OnChestLanded) rather than a
    // flat reveal delay — held at 0 until the earliest particle could land,
    // then ramping to 1 by the time the slowest one does, so the buttons
    // visibly materialize alongside the swarm converging on them. Only the
    // first slot's mover is sampled since every slot shares the same
    // authored travel timing; the group fades as one.
    protected IEnumerator RevealAfterDelay()
    {
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
        playerInput.SwitchToUIMap();
        SetButtonsVisible(true);
        UpdateColorMatchParticles();

        if (rewardButtons.Length > 0)
        {
            EventSystem.current.SetSelectedGameObject(rewardButtons[0].gameObject);
        }

        _revealRoutine = null;
    }

    protected void SpawnParticles()
    {
        int count = Mathf.Min(Mathf.Min(rewardButtons.Length, particleMovers.Length), _currentOffer.Length);
        for (int i = 0; i < count; i++)
        {
            particleMovers[i].transform.position = chest.transform.position;
            particleMovers[i].Init(rewardButtons[i].GetComponent<RectTransform>(), _currentOffer[i].AssociatedColor, chest.transform.position.z);
        }
    }

    protected void OnRewardButtonPressed(int index)
    {
        transitionManager.SetSkipHoldBlocked(false);

        for (int i = 0; i < flashImages.Length; i++)
        {
            flashImages[i].gameObject.SetActive(true);
        }

        for (int i = 0; i < colorMatchParticles.Length; i++)
        {
            if (colorMatchParticles[i] != null)
            {
                colorMatchParticles[i].Stop();
            }
        }

        rewardButtonsGroup.interactable = false;
        rewardButtonsGroup.blocksRaycasts = false;

        if (_fadeOutRoutine != null)
        {
            StopCoroutine(_fadeOutRoutine);
        }
        _fadeOutRoutine = StartCoroutine(HideButtonsAfterFlash());

        (Transform[] targets, GemColor[] colors) = index < applyParticleMovers.Length ? ResolveApplyTargets(_currentOffer[index]) : (null, null);
        RewardApplyParticleMover mover = null;
        if (targets != null && targets.Length > 0)
        {
            RectTransform buttonRect = rewardButtons[index].GetComponent<RectTransform>();
            mover = applyParticleMovers[index];
            mover.Init(buttonRect, targets, colors, chest.transform.position.z);
        }

        DismissUnpickedButtons(index);

        transitionEventChannel.Raise(TransitionPhase.RewardChosen);

        _rewardGivenFired = false;
        _pendingReward = _currentOffer[index];

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

    // Holds every button's flash Image up for flashDuration, then cuts
    // rewardButtonsGroup.alpha straight to 0 — a snap, not a gradual fade.
    protected IEnumerator HideButtonsAfterFlash()
    {
        yield return new WaitForSeconds(flashDuration);

        SetButtonsVisible(false);
        EventSystem.current.SetSelectedGameObject(null);
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

        // The reward's own Apply() now owns its target-facing feedback (see
        // RewardDefinition.FlashGlow) — RewardOfferUI no longer reaches into
        // character VFX directly.
        rewardManager.ChooseReward(_pendingReward);
        AudioManager.Instance.PlayActionSFX(rewardGivenSfx);

        transitionEventChannel.Raise(TransitionPhase.RewardGiven);
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

    // A reward pick can't be cancelled once offered, so unlike Menu.OnCancel
    // (which Hides), Cancel here opens the pause menu instead. Only reachable
    // while a reward button holds selection - see RevealAfterDelay/
    // HideButtonsAfterFlash, which set/clear it - so this can't fire outside
    // the offer window.
    public void OnCancel(BaseEventData eventData)
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        Selectable returnTo = selected != null ? selected.GetComponent<Selectable>() : null;
        pauseButton.Open(returnTo);
    }

    protected void SetButtonsVisible(bool visible)
    {
        rewardButtonsGroup.alpha = visible ? 1f : 0f;
        rewardButtonsGroup.interactable = visible;
        rewardButtonsGroup.blocksRaycasts = visible;
    }
}
