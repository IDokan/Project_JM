// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/11/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatIntroController.cs
// Summary: A script to manage combat intro logic.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.
//                      Combat intro conducts below tasks:
//                                          1. Move party to an arrival position.
//                                          2. Invoke camera mover and enemy spawner. (Can be done by event channel though)


using System.Collections;
using UnityEngine;

public class CombatIntroController : TransitionController
{

    [SerializeField] protected TransitionEventChannel transitionEventChannel;

    [Header("Party Positions")]
    [SerializeField] protected Transform partyTransform;
    [SerializeField] protected Vector3 partyStartPosition;
    [SerializeField] protected Vector3 partyArrivalPosition;

    [Header("Board Positions")]
    [SerializeField] protected Transform boardTransform;
    [SerializeField] protected Vector3 boardStartPosition;
    [SerializeField] protected Vector3 boardArrivalPosition;

    [Header("UI Positions")]
    [SerializeField] protected RectTransform[] uiTransforms;
    [SerializeField] protected Vector3[] uiStartPositions;
    [SerializeField] protected Vector3[] uiArrivalPositions;

    [Header("Menu position")]
    [SerializeField] protected RectTransform menuTransform;
    [SerializeField] protected Vector3 menuStartPosition;
    [SerializeField] protected Vector3 menuArrivalPosition;

    [Header("Timing")]
    [SerializeField] protected float partyMoveDuration = 5f;
    [SerializeField] protected float boardMoveDelay = 4f;
    [SerializeField] protected float boardMoveDuration = 2f;
    [SerializeField] protected float uiMoveDelay = 5.5f;
    [SerializeField] protected float uiMoveDuration = 1f;
    [SerializeField] protected float menuMoveDuration = 0.5f;

    protected Vector3 _partyStartOffsetToCamera;
    protected Vector3 _partyArrivalOffsetToCamera;

    protected Coroutine _partyRoutine = null;
    protected Coroutine _boardRoutine = null;
    protected Coroutine _uiRoutine = null;
    protected Coroutine _menuRoutine = null;

    protected override void Awake()
    {
        base.Awake();
        Transform cameraTransform = Camera.main.transform;
        _partyStartOffsetToCamera = partyStartPosition - cameraTransform.position;
        _partyArrivalOffsetToCamera = partyArrivalPosition - cameraTransform.position;
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
        while (t < partyMoveDuration)
        {
            t += Time.deltaTime;

            // Simple fixing: Update destination at every frame to prevent bugs when both camer and party started moving. (Both enemy and party died)
            destination = camera.transform.position + _partyArrivalOffsetToCamera;
            partyTransform.position = Vector3.Lerp(startPosition, destination, t / partyMoveDuration);

            yield return null;
        }
        partyTransform.position = destination;

        partyParallaxLayer.SetParallaxMode();

        transitionEventChannel.Raise(TransitionPhase.IntroPartyMoveEnd);

        _partyRoutine = null;
    }

    protected IEnumerator BoardRoutine()
    {
        boardTransform.localPosition = boardStartPosition;

        yield return new WaitForSeconds(boardMoveDelay);

        float t = 0f;
        while (t < boardMoveDuration)
        {
            t += Time.deltaTime;
            boardTransform.localPosition = Vector3.Lerp(boardStartPosition, boardArrivalPosition, t / boardMoveDuration);
            yield return null;
        }

        boardTransform.localPosition = boardArrivalPosition;

        transitionEventChannel.Raise(TransitionPhase.IntroBoardMoveEnd);

        _boardRoutine = null;
    }

    protected IEnumerator UIRoutine()
    {
        for (int i = 0; i < uiTransforms.Length; i++)
        {
            uiTransforms[i].anchoredPosition = uiStartPositions[i];
        }

        yield return new WaitForSeconds(uiMoveDelay);

        float t = 0f;
        while (t < uiMoveDuration)
        {
            t += Time.deltaTime;

            for (int i = 0; i < uiTransforms.Length; i++)
            {
                RectTransform uiTransform = uiTransforms[i];
                uiTransform.anchoredPosition = Vector2.Lerp(uiStartPositions[i], uiArrivalPositions[i], t / uiMoveDuration);
            }
            yield return null;
        }

        for (int i = 0; i < uiTransforms.Length; i++)
        {
            RectTransform uiTransform = uiTransforms[i];
            uiTransform.anchoredPosition = uiArrivalPositions[i];
        }

        CompleteTransition();

        _uiRoutine = null;
    }

    public void StartIntroTransition()
    {
        RequestTransitionStart(BeginIntroTransition);
    }

    private void BeginIntroTransition()
    {
        transitionEventChannel.Raise(TransitionPhase.IntroTransitionBegin);

        GlobalRNG.Instance.ResetSeed();

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

        if (uiTransforms.Length > 0 && uiArrivalPositions.Length == uiTransforms.Length)
        {
            KillOngoingRoutine(_uiRoutine);
            _uiRoutine = StartCoroutine(UIRoutine());
        }

        if (menuTransform != null)
        {
            KillOngoingRoutine(_menuRoutine);
            _menuRoutine = StartCoroutine(MenuRoutine());
        }
    }

    protected IEnumerator MenuRoutine()
    {
        if (menuTransform.gameObject.activeSelf == false)
        {
            _menuRoutine = null;
            yield break;
        }

        menuTransform.anchoredPosition = menuStartPosition;

        float t = 0f;
        while (t < menuMoveDuration)
        {
            t += Time.deltaTime;

            menuTransform.anchoredPosition = Vector2.Lerp(menuStartPosition, menuArrivalPosition, t / menuMoveDuration);
            yield return null;
        }

        menuTransform.anchoredPosition = menuArrivalPosition;
        menuTransform.gameObject.SetActive(false);

        _menuRoutine = null;
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
