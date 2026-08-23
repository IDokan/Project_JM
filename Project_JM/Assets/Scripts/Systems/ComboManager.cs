// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/20/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SystemBehaviour.cs
// Summary: A manager for combo.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using MatchEnums;
using GemEnums;
using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] protected MatchEventChannel matchEvents;
    [SerializeField] protected CharacterDeathEventChannel deathChannel;
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected CharacterStatus partyStatus;
    [SerializeField] protected float comboResetTime = 3f;

    [Header("Audio")]
    [SerializeField] private AudioCueSO gemMatchSfx;
    [SerializeField] private float audioChainTimeLimit = 0.2f;

    private int _audioChainIndex;
    private float _audioChainTimer;
    private int _lastAudioFrame = -1;

    public event Action<int, float> OnComboUpdated;

    // Permanent bonus granted by rewards (e.g. Focus). 0 means no bonus.
    protected float _rewardComboResetTimeBonus = 0f;
    public float ComboResetTime => comboResetTime + _rewardComboResetTimeBonus;

    protected int _comboCount = 0;
    protected float _timer = 0f;
    protected int _maxComboThisRun = 0;


    private void OnEnable()
    {
        matchEvents.OnRaised += OnMatch;
        deathChannel.OnRaised += OnCharacterDied;
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    private void OnDisable()
    {
        matchEvents.OnRaised -= OnMatch;
        deathChannel.OnRaised -= OnCharacterDied;
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.IntroTransitionBegin)
        {
            _rewardComboResetTimeBonus = 0f;
        }
    }

    public void AddComboResetTimeBonus(float bonus)
    {
        _rewardComboResetTimeBonus += bonus;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_comboCount > 0)
        {
            _timer -= GlobalTimeManager.DeltaTime;
            OnComboUpdated.Invoke(_comboCount, _timer);

            if (_timer <= 0f)
            {
                ResetCombo();
            }
        }

        if (_audioChainTimer > 0f)
        {
            _audioChainTimer -= GlobalTimeManager.DeltaTime;
            if (_audioChainTimer <= 0f)
                _audioChainIndex = 0;
        }
    }
    
    public void OnMatch(MatchEvent matchEvent)
    {
        // Pass if no valid color
        if (matchEvent.Color == GemColor.None)
        {
            return;
        }

        // On Match, increase combo for a duration.
        _comboCount += (int)matchEvent.Tier;
        _maxComboThisRun = Mathf.Max(_maxComboThisRun, _comboCount);
        _timer = ComboResetTime;
        OnComboUpdated.Invoke(_comboCount, _timer);

        if (Time.frameCount != _lastAudioFrame)
        {
            AudioManager.Instance.PlayPuzzleSFX(gemMatchSfx, _audioChainIndex);
            _audioChainIndex = gemMatchSfx != null
                ? Mathf.Min(_audioChainIndex + 1, gemMatchSfx.ClipCount - 1)
                : 0;
            _audioChainTimer = audioChainTimeLimit;
            _lastAudioFrame = Time.frameCount;
        }

        // Increase critical hit chance per combo
        partyStatus.SetComboCritBonus(_comboCount / 100f);
    }

    public void ResetCombo()
    {
        _comboCount = 0;
        partyStatus.SetComboCritBonus(_comboCount);
        _timer = 0f;
        OnComboUpdated.Invoke(_comboCount, _timer);
    }

    protected void OnCharacterDied(CharacterStatus stat)
    {
        SaveDataManager.Instance.TrySetMaxCombo(_maxComboThisRun);
    }
}
