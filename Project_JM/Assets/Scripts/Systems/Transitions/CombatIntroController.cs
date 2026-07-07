// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/11/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatIntroController.cs
// Summary: A script to manage combat intro logic.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
//                      Combat intro conducts below tasks:
//                                          1. Move party to an arrival position.
//                                          2. Invoke camera mover and enemy spawner. (Can be done by event channel though)
//                                          3. Play and fade in combat BGM.


using System.Collections;
using TutorialEnums;
using UnityEngine;

public class CombatIntroController : TransitionController
{

    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [Header("Party Positions")]
    [SerializeField] protected Transform partyTransform;

    [Header("Board Positions")]
    [SerializeField] protected Transform boardTransform;

    [Header("UI Positions")]
    [SerializeField] protected RectTransform[] uiTransforms;

    [Header("Layout Profiles")]
    [SerializeField] protected CombatLayoutProfileData landscapeLayoutProfile;
    [SerializeField] protected CombatLayoutProfileData portraitLayoutProfile;

    protected Vector3 _partyStartOffsetToCamera;
    protected Vector3 _partyArrivalOffsetToCamera;

    protected CombatLayoutProfileData _activeLayoutProfile;
    protected Vector3 _boardStartPosition;
    protected Vector3 _boardArrivalPosition;
    protected Vector3[] _uiStartPositions;
    protected Vector3[] _uiArrivalPositions;

    protected float _partyMoveDuration;
    protected float _boardMoveDelay;
    protected float _boardMoveDuration;
    protected float _uiMoveDelay;
    protected float _uiMoveDuration;

    protected Coroutine _partyRoutine = null;
    protected Coroutine _boardRoutine = null;
    protected Coroutine _uiRoutine = null;

    protected override void Awake()
    {
        base.Awake();
        _activeLayoutProfile = ResolveActiveLayoutProfile(landscapeLayoutProfile, portraitLayoutProfile);
        _boardStartPosition = _activeLayoutProfile.IntroBoardStartPosition;
        _boardArrivalPosition = _activeLayoutProfile.IntroBoardArrivalPosition;
        _uiStartPositions = _activeLayoutProfile.IntroUIStartPositions;
        _uiArrivalPositions = _activeLayoutProfile.IntroUIArrivalPositions;

        _partyMoveDuration = _activeLayoutProfile.IntroPartyMoveDuration;
        _boardMoveDelay = _activeLayoutProfile.IntroBoardMoveDelay;
        _boardMoveDuration = _activeLayoutProfile.IntroBoardMoveDuration;
        _uiMoveDelay = _activeLayoutProfile.IntroUIMoveDelay;
        _uiMoveDuration = _activeLayoutProfile.IntroUIMoveDuration;

        CameraOrientationSetter cameraOrientationSetter = Camera.main.GetComponent<CameraOrientationSetter>();
        Vector3 cameraPosition = cameraOrientationSetter != null ? cameraOrientationSetter.OriginalPosition : Camera.main.transform.position;
        _partyStartOffsetToCamera = _activeLayoutProfile.IntroPartyStartPosition - cameraPosition;
        _partyArrivalOffsetToCamera = _activeLayoutProfile.IntroPartyArrivalPosition - cameraPosition;
        // Since Board is under an object that contains ParallaxLayout script, no need to find offset.
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartIntroTransition();
    }

    protected IEnumerator IntroRoutine()
    {
        ParallaxLayer partyParallaxLayer = partyTransform.GetComponent<ParallaxLayer>();

        partyParallaxLayer.SetManualMode();

        Camera camera = Camera.main;
        Vector3 startPosition = camera.transform.position + _partyStartOffsetToCamera;
        partyTransform.position = startPosition;
        Vector3 destination = camera.transform.position + _partyArrivalOffsetToCamera;

        float t = 0f;
        while (t < _partyMoveDuration)
        {
            t += Time.deltaTime;

            // Simple fixing: Update destination at every frame to prevent bugs when both camer and party started moving. (Both enemy and party died)
            destination = camera.transform.position + _partyArrivalOffsetToCamera;
            partyTransform.position = Vector3.Lerp(startPosition, destination, t / _partyMoveDuration);

            yield return null;
        }
        partyTransform.position = destination;

        partyParallaxLayer.SetParallaxMode();

        transitionEventChannel.Raise(TransitionPhase.IntroPartyMoveEnd);

        _partyRoutine = null;
    }

    protected IEnumerator BoardRoutine()
    {
        boardTransform.localPosition = _boardStartPosition;

        yield return new WaitForSeconds(_boardMoveDelay);

        float t = 0f;
        while (t < _boardMoveDuration)
        {
            t += Time.deltaTime;
            boardTransform.localPosition = Vector3.Lerp(_boardStartPosition, _boardArrivalPosition, t / _boardMoveDuration);
            yield return null;
        }

        boardTransform.localPosition = _boardArrivalPosition;

        transitionEventChannel.Raise(TransitionPhase.IntroBoardMoveEnd);

        _boardRoutine = null;
    }

    protected IEnumerator UIRoutine()
    {
        for (int i = 0; i < uiTransforms.Length; i++)
        {
            uiTransforms[i].anchoredPosition = _uiStartPositions[i];
        }

        yield return new WaitForSeconds(_uiMoveDelay);

        float t = 0f;
        while (t < _uiMoveDuration)
        {
            t += Time.deltaTime;

            for (int i = 0; i < uiTransforms.Length; i++)
            {
                RectTransform uiTransform = uiTransforms[i];
                uiTransform.anchoredPosition = Vector2.Lerp(_uiStartPositions[i], _uiArrivalPositions[i], t / _uiMoveDuration);
            }
            yield return null;
        }

        for (int i = 0; i < uiTransforms.Length; i++)
        {
            RectTransform uiTransform = uiTransforms[i];
            uiTransform.anchoredPosition = _uiArrivalPositions[i];
        }

        CompleteTransition();

        _uiRoutine = null;
    }

    public void StartIntroTransition()
    {
        RequestTransitionStart(BeginIntroTransition);
    }

    private const int TutorialDeterministicSeed = 525;

    private void BeginIntroTransition()
    {
        transitionEventChannel.Raise(TransitionPhase.IntroTransitionBegin);

        TutorialProgress progress = SaveDataManager.Instance.Progress;
        bool useRandomSeed = progress >= TutorialProgress.Hard
            || SaveDataManager.Instance.IsTutorialCompleted(progress);
        if (useRandomSeed)
        {
            GlobalRNG.Instance.Reseed(System.Environment.TickCount);
        }
        else
        {
            GlobalRNG.Instance.Reseed(TutorialDeterministicSeed);
        }

        if (partyTransform != null)
        {
            KillOngoingRoutine(_partyRoutine);
            _partyRoutine = StartCoroutine(IntroRoutine());
        }

        if (boardTransform != null)
        {
            KillOngoingRoutine(_boardRoutine);
            _boardRoutine = StartCoroutine(BoardRoutine());
        }

        if (uiTransforms.Length > 0 && _uiArrivalPositions.Length == uiTransforms.Length)
        {
            KillOngoingRoutine(_uiRoutine);
            _uiRoutine = StartCoroutine(UIRoutine());
        }
    }

    protected void KillOngoingRoutine(Coroutine routine)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }
}
