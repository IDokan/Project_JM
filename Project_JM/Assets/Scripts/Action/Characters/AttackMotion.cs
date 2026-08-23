// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/12/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackMotion.cs
// Summary: A script for ally attack motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using MatchEnums;
using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Tiered attack scheduler with:
/// - Tier-specific Approach / Attack / Return (per character via Animator Controller)
/// - Priority switching: 5 > 4 > 3
/// - Tier switching rule: Return(oldTier) -> Approach(newTier)
/// - "At least one attack after approach" rule (no Approach->Return without a strike)
/// - Hurt interrupt: stop immediately, knockback + DamagedTrig, then Return(interruptedTier), then resume
///
/// REQUIRED Animator setup (names configurable in inspector):
///   int     Tier
///   trigger ApproachTrig
///   trigger AttackTrig
///   trigger ReturnTrig
///   trigger DamagedTrig
///
/// REQUIRED Animation Events (called on the clips):
///   Approach_* clip end -> AnimEvent_ApproachEnd()
///   Attack_*   hit frame -> AnimEvent_Hit()
///   Attack_*   clip end -> AnimEvent_AttackEnd()
///   Return_*   clip end -> AnimEvent_ReturnEnd()
///   Hurt       clip end -> AnimEvent_HurtEnd()
/// </summary>

public class AttackMotion : MonoBehaviour
{
    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;
    [SerializeField] protected EnemySpawnedEventChannel enemySpawnedEventChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterStateVisual stateVisual;

    protected Animator _animator;
    protected Transform _kockbackRoot;

    // The durations can be changed in near future
    //      to display different pause for different attack logics.
    protected const float _moveDuration = 0.1f;
    protected const float _pauseDuration = 0.2f;

    protected static readonly int DamagedTrig = Animator.StringToHash("DamagedTrig");
    protected static readonly int ApproachTrig = Animator.StringToHash("ApproachTrig");
    protected static readonly int AttackTrig = Animator.StringToHash("AttackTrig");
    protected static readonly int ReturnTrig = Animator.StringToHash("ReturnTrig");
    protected static readonly int WalkBool = Animator.StringToHash("WalkBool");
    protected static readonly int DiedBool = Animator.StringToHash("DiedBool");
    protected static readonly int VictoryBool = Animator.StringToHash("VictoryBool");

    protected static readonly int TierInt = Animator.StringToHash("TierInt");

    public event Action<MatchTier> OnHit;

    protected int _pendingThreeTier, _pendingFourTier, _pendingFiveTier;
    protected Coroutine _runner;

    protected MatchTier? _currentTier;                    // tier we're currently "in"

    protected bool _hurtRequested;
    protected bool _walkRequested;
    protected bool _died = false;
    protected bool Inturrupted => _hurtRequested || _died;
    protected int _version;                           // increments to break waits on interrupts

    protected bool _attackedSinceApproach;

    protected enum State
    {
        Idle,
        Approaching,
        Attacking,
        Returning,
        Hurting,
        Walking,
        Died,
    }

    protected State _state = State.Idle;

    // Flags set by animation events.
    protected bool _approachDone, _attackDone, _returnDone, _hurtDone, _walkDone;
    protected MatchTier _executingTierForHit;

    protected Sequence _knockbackSequence;
    protected Vector3 _knockbackOffset;
    protected Vector3 _originalPosition;

    protected void Awake()
    {
        if (!_animator)
        {
            _animator = GetComponent<Animator>();
        }
        if (!_kockbackRoot)
        {
            _kockbackRoot = transform;
            _originalPosition = _kockbackRoot.localPosition;
        }
    }

    protected void OnEnable()
    {
        if (characterDeathEventChannel != null)
        {
            characterDeathEventChannel.OnRaised += OnCharacterDied;
        }
        else
        {
            Debug.LogWarning("CharacterDeathEventChannel is null", this);
        }
        if (enemySpawnedEventChannel != null)
        {
            enemySpawnedEventChannel.OnRaised += OnEnemySpawned;
        }
        else
        {
            Debug.LogWarning("EnemySpawnedEventChannel is null", this);
        }
        if (transitionEventChannel)
        {
            transitionEventChannel.OnRaised += OnTransitionEvent;
        }
        else
        {
            Debug.LogWarning("TransitionEventChannel is null", this);
        }
    }

    protected void OnDisable()
    {
        if (characterDeathEventChannel != null)
        {
            characterDeathEventChannel.OnRaised -= OnCharacterDied;
        }
        if (enemySpawnedEventChannel != null)
        {
            enemySpawnedEventChannel.OnRaised -= OnEnemySpawned;
        }
        if (transitionEventChannel)
        {
            transitionEventChannel.OnRaised -= OnTransitionEvent;
        }
    }

    public void EnqueueAttack(int tier)
    {
        switch (tier)
        {
            case 3:
                _pendingThreeTier++;
                break;
            case 4:
                _pendingFourTier++;
                break;
            case 5:
                _pendingFiveTier++;
                break;
            default:
                break;
        }

        EnsureRunner();
    }

    public void EnqueueAttack(MatchTier tier)
    {
        EnqueueAttack((int)tier);
    }

    public void RequestHurt(Vector3 offset)
    {
        _hurtRequested = true;

        // Break any waits immediately.
        _version++;

        _knockbackOffset = offset;

        EnsureRunner();
    }

    protected IEnumerator Run()
    {
        while (true)
        {
            if (_died)
            {
                HandleDie();
                break;
            }

            // 0) In idle, play walking animation when requested
            if (_walkRequested && _state == State.Idle)
            {
                yield return HandleWalk();
                continue;
            }

            // 1) Hurt has absolute priority
            if (_hurtRequested)
            {
                yield return HandleHurt();
                continue;
            }

            // 2) If nothing pending and we're idle -> stop
            if (!HasPending() && _state == State.Idle)
            {
                break;
            }

            // 3) If we are idle but have pending -> pick highest tier and approach it
            if (!_currentTier.HasValue)
            {
                if (!HasPending())
                {
                    // Nothing to do; ensure we are idle
                    _state = State.Idle;
                    yield return null;
                    continue;
                }

                _currentTier = PeekHighestPending();
                yield return PlayApproach(_currentTier.Value);
                if (_hurtRequested)
                {
                    continue;
                }

                _state = State.Attacking;
                continue;
            }

            // 4) If we are attacking, decide what to do next
            if (_state == State.Attacking && _currentTier.HasValue)
            {
                MatchTier current = _currentTier.Value;
                if (!_attackedSinceApproach)
                {
                    ConsumeOne(current);
                    yield return PlayAttack(current);
                    continue;
                }

                MatchTier desired = PeekHighestPendingOrCurrent(current);

                if (desired != current && IsHigher(desired, current))
                {
                    yield return PlayReturn(current);
                    _state = State.Idle;
                    _currentTier = null;
                    continue;
                }


                // Keep working on the current tier until it's running out.
                if (PendingCount(current) > 0)
                {
                    ConsumeOne(current);
                    yield return PlayAttack(current);
                    // Remain in State.Attacking
                    continue;
                }

                // Nothing pending: return and finish
                yield return PlayReturn(_currentTier.Value);
                _state = State.Idle;
                _currentTier = null;
                continue;
            }

            // 5) If we are in some other state but not hurt, hust yield (events will advance)
            yield return null;
        }

        _runner = null;
    }

    protected IEnumerator HandleHurt()
    {
        _state = State.Hurting;

        _hurtRequested = false;

        stateVisual?.OnDamagedBegin();

        PlayKnockback();

        // Wait hurt end event (or another interrupt version bump)
        _hurtDone = false;
        yield return WaitFlagOrInterrupt(() => _hurtDone);

        stateVisual?.OnDamagedEnd();

        _state = State.Idle;
        _currentTier = null;
    }

    protected void PlayKnockback()
    {
        if (_animator == null)
        {
            return;
        }

        Move(0.1f, 0.2f);

        // Force hurt animation (AnyState -> Hurt)
        ResetCombatTriggers();
        _animator.SetTrigger(DamagedTrig);
    }

    protected IEnumerator PlayApproach(MatchTier tier)
    {
        _state = State.Approaching;
        _approachDone = false;
        _attackedSinceApproach = false;

        _animator.SetInteger(TierInt, (int)tier);
        ResetCombatTriggers();
        _animator.SetTrigger(ApproachTrig);


        yield return WaitFlagOrInterrupt(() => _approachDone);
        if (Inturrupted) yield break;
    }

    protected IEnumerator PlayAttack(MatchTier tier)
    {
        _state = State.Attacking;
        _attackDone = false;

        _executingTierForHit = tier;
        _animator.SetInteger(TierInt, (int)tier);
        ResetCombatTriggers();
        _animator.SetTrigger(AttackTrig);

        yield return WaitFlagOrInterrupt(() => _attackDone);
        _attackedSinceApproach = true;
        if (Inturrupted) yield break;
    }

    protected IEnumerator PlayReturn(MatchTier tier)
    {
        _state = State.Returning;
        _returnDone = false;

        _animator.SetInteger(TierInt, (int)tier);
        ResetCombatTriggers();
        _animator.SetTrigger(ReturnTrig);

        yield return WaitFlagOrInterrupt(() => _returnDone);
        if (Inturrupted) yield break;

        _state = State.Idle;
    }

    protected IEnumerator WaitFlagOrInterrupt(Func<bool> flag)
    {
        int v = _version;
        while (!flag() && v == _version && !Inturrupted)
        {
            yield return null;
        }
    }

    protected void ResetCombatTriggers()
    {
        _animator.ResetTrigger(ApproachTrig);
        _animator.ResetTrigger(AttackTrig);
        _animator.ResetTrigger(ReturnTrig);
        _animator.ResetTrigger(DamagedTrig);
        _animator.ResetTrigger(WalkBool);
        _animator.SetBool(WalkBool, false);
    }

    protected bool HasPending() => _pendingThreeTier > 0 || _pendingFourTier > 0 || _pendingFiveTier > 0;

    // This function requires to call HasPending before to call it.
    // If have not called HasPending, prefer to call PeekHighestPendingOrCurrent
    protected MatchTier PeekHighestPending()
    {
        if (_pendingFiveTier > 0)
        {
            return MatchTier.Five;
        }
        if (_pendingFourTier > 0)
        {
            return MatchTier.Four;
        }
        return MatchTier.Three;
    }

    protected MatchTier PeekHighestPendingOrCurrent(MatchTier current)
    {
        // If nothing pending, keep current (so we can return gracefully);
        if (!HasPending())
        {
            return current;
        }
        return PeekHighestPending();
    }

    protected int PendingCount(MatchTier t)
    {
        return t switch
        {
            MatchTier.Three => _pendingThreeTier,
            MatchTier.Four => _pendingFourTier,
            MatchTier.Five => _pendingFiveTier,
            _ => 0
        };
    }

    protected void ConsumeOne(MatchTier t)
    {
        switch (t)
        {
            case MatchTier.Three: _pendingThreeTier = Mathf.Max(0, _pendingThreeTier - 1); break;

            case MatchTier.Four: _pendingFourTier = Mathf.Max(0, _pendingFourTier - 1); break;

            case MatchTier.Five: _pendingFiveTier = Mathf.Max(0, _pendingFiveTier - 1); break;
        }
    }

    protected void ClearPendingAttacks()
    {
        _pendingThreeTier = 0;
        _pendingFourTier = 0;
        _pendingFiveTier = 0;
    }

    protected static bool IsHigher(MatchTier a, MatchTier b) => (int)a > (int)b;

    protected void EnsureRunner()
    {
        if (_runner == null)
        {
            _runner = StartCoroutine(Run());
        }
    }

    protected void StopRunner()
    {
        if (_runner != null)
        {
            StopCoroutine(_runner);
            _runner = null;
        }
    }

    // Animation Events (called from clips) ------------------------
    public void AnimEvent_ApproachEnd() => _approachDone = true;

    public void AnimEvent_Hit()
    {
        // Damage should correspond to the attack currently being executed.
        OnHit?.Invoke(_executingTierForHit);
    }

    public void AnimEvent_AttackEnd() => _attackDone = true;
    public void AnimEvent_ReturnEnd() => _returnDone = true;
    public void AnimEvent_HurtEnd() => _hurtDone = true;

    protected void Move(float moveDuration, float pauseDuration)
    {
        // If previous sequence has not finished yet, kill it.
        if (_knockbackSequence != null && _knockbackSequence.IsActive())
        {
            _knockbackSequence.Kill();
        }

        Vector3 target = _kockbackRoot.localPosition + _knockbackOffset;

        _knockbackSequence = DOTween.Sequence();
        _knockbackSequence.Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    protected void OnCharacterDied(CharacterStatus characterStat)
    {
        if (characterStat.TryGetComponent<EnemyTag>(out _))
        {
            ClearPendingAttacks();

            _animator.SetBool(VictoryBool, true);
            stateVisual?.OnVictory();
        }

        if (characterStat.TryGetComponent<AllyTag>(out _))
        {
            _died = true;
            EnsureRunner();
        }
    }

    protected void StartWalk()
    {
        _walkRequested = true;
        EnsureRunner();
    }

    protected IEnumerator HandleWalk()
    {
        _state = State.Walking;

        _walkRequested = false;


        ResetCombatTriggers();
        _animator.SetBool(WalkBool, true);

        _walkDone = false;
        yield return WaitFlagOrInterrupt(() => _walkDone);

        _animator.SetBool(WalkBool, false);

        _state = State.Idle;
        _currentTier = null;
    }

    protected void OnEnemySpawned(GameObject enemy)
    {
        _walkDone = true;
    }

    protected void HandleDie()
    {
        _state = State.Died;

        ResetCombatTriggers();
        _animator.SetBool(DiedBool, true);
        stateVisual?.OnDied();

        _currentTier = null;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            Clear();
            StartWalk();
        }

        if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            _animator.SetBool(VictoryBool, false);
            stateVisual?.OnVictoryEnd();
            StartWalk();
        }

        // Insurance: MiddleTransitionStarts already clears this, but a
        // character that wasn't around yet to hear it (e.g. respawned/
        // re-enabled mid-transition) could otherwise carry VictoryBool into
        // the next fight still true.
        if (phase == TransitionPhase.MiddleTransitionEnd)
        {
            _animator.SetBool(VictoryBool, false);
            stateVisual?.OnVictoryEnd();
        }
    }

    protected void Clear()
    {
        ClearPendingAttacks();
        _hurtRequested = false;
        _walkRequested = false;
        _died = false;
        _version = 0;
        _attackedSinceApproach = false;

        _state = State.Idle;

        _approachDone = false;
        _attackDone = false;
        _returnDone = false;
        _hurtDone = false;
        _walkDone = false;

        ResetCombatTriggers();

        _animator.SetBool(WalkBool, false);
        _animator.SetBool(DiedBool, false);
        _animator.SetBool(VictoryBool, false);
        stateVisual?.OnVictoryEnd();
        stateVisual?.OnDamagedEnd();

        StopRunner();
    }
}
