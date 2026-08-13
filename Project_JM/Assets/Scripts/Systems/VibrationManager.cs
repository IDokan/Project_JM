// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 13/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: VibrationManager.cs
// Summary: Pulses gamepad rumble on gem matches, scaled by match tier. Stays
//          silent unless a gamepad is the most recently active input device.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using MatchEnums;
using UnityEngine;
using UnityEngine.InputSystem;

public class VibrationManager : MonoBehaviour
{
    [SerializeField] protected MatchEventChannel matchEventChannel;

    [SerializeField] protected float tierThreeIntensity = 0.3f;
    [SerializeField] protected float tierFourIntensity = 0.6f;
    [SerializeField] protected float tierFiveIntensity = 1f;
    [SerializeField] protected float pulseDuration = 0.15f;

    // ComboManager plays gemMatchSfx the instant it sees the frame's first
    // match (ComboManager.cs:109-117), but the audible hit inside that clip
    // sits slightly after its start. Delaying the motors by the same amount
    // lines the felt pulse up with the heard hit instead of firing early.
    [SerializeField] protected float sfxSyncDelay = 0.1f;

    private Coroutine _pulseRoutine;
    private Coroutine _batchRoutine;
    private int _pendingFrame = -1;
    private MatchTier _pendingHighestTier;

    protected void OnEnable() => matchEventChannel.OnRaised += OnMatched;

    protected void OnDisable()
    {
        matchEventChannel.OnRaised -= OnMatched;
        StopVibration();
    }

    // BoardManager fires one MatchEventChannel event per match group, and
    // simultaneous groups (e.g. one swap completing two lines at once) are
    // all raised synchronously within the same frame — see
    // BoardManager.ResolveMatches(). Collecting them here and pulsing once at
    // end of frame, using the highest tier seen, keeps a strong match from
    // being downgraded by a weaker one raised right after it.
    private void OnMatched(MatchEvent matchEvent)
    {
        if (!IsGamepadActive())
        {
            return;
        }

        if (Time.frameCount != _pendingFrame)
        {
            _pendingFrame = Time.frameCount;
            _pendingHighestTier = matchEvent.Tier;

            if (_batchRoutine != null)
            {
                StopCoroutine(_batchRoutine);
            }
            _batchRoutine = StartCoroutine(PulseHighestTierAtEndOfFrame());
        }
        else if (matchEvent.Tier > _pendingHighestTier)
        {
            _pendingHighestTier = matchEvent.Tier;
        }
    }

    private IEnumerator PulseHighestTierAtEndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        float intensity = GetIntensity(_pendingHighestTier);
        _batchRoutine = null;

        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
        }
        _pulseRoutine = StartCoroutine(PulseRoutine(intensity));
    }

    private float GetIntensity(MatchTier tier)
    {
        switch (tier)
        {
            case MatchTier.Five:
                return tierFiveIntensity;
            case MatchTier.Four:
                return tierFourIntensity;
            default:
                return tierThreeIntensity;
        }
    }

    private IEnumerator PulseRoutine(float intensity)
    {
        yield return new WaitForSecondsRealtime(sfxSyncDelay);
        Gamepad.current?.SetMotorSpeeds(intensity, intensity);
        yield return new WaitForSecondsRealtime(pulseDuration);
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
        _pulseRoutine = null;
    }

    private void StopVibration()
    {
        if (_pulseRoutine != null)
        {
            StopCoroutine(_pulseRoutine);
            _pulseRoutine = null;
        }
        if (_batchRoutine != null)
        {
            StopCoroutine(_batchRoutine);
            _batchRoutine = null;
        }
        _pendingFrame = -1;
        Gamepad.current?.SetMotorSpeeds(0f, 0f);
    }

    // A gamepad connecting doesn't mean it's in use — the player may still be
    // on mouse/keyboard/touch. Compare each device's lastUpdateTime (the same
    // signal Unity's own control-scheme icons use) so only the device that
    // most recently produced input counts as "active".
    private bool IsGamepadActive()
    {
        Gamepad gamepad = Gamepad.current;
        if (gamepad == null)
        {
            return false;
        }

        double gamepadTime = gamepad.lastUpdateTime;
        double keyboardTime = Keyboard.current?.lastUpdateTime ?? -1d;
        double mouseTime = Mouse.current?.lastUpdateTime ?? -1d;
        double touchTime = Touchscreen.current?.lastUpdateTime ?? -1d;

        return gamepadTime > keyboardTime && gamepadTime > mouseTime && gamepadTime > touchTime;
    }
}
