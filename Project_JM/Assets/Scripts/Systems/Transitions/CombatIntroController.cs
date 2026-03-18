// SPDX-License-Identifier: MIT
// Copyright (c) 03/11/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatIntroController.cs
// Summary: A script to manage combat intro logic.
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

    protected Vector3 partyStartOffsetToCamera;
    protected Vector3 partyArrivalOffsetToCamera;

    bool _isInTransition = false;

    protected void Awake()
    {
        Transform cameraTransform = Camera.main.transform;
        partyStartOffsetToCamera = partyStartPosition - cameraTransform.position;
        partyArrivalOffsetToCamera = partyArrivalPosition - cameraTransform.position;
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
        partyTransform.position = camera.transform.position + partyStartOffsetToCamera;
        Vector3 destination = camera.transform.position + partyArrivalOffsetToCamera;

        float t = 0f;
        while (t < partyMoveDuration)
        {
            t += Time.deltaTime;

            partyTransform.position = Vector3.Lerp(partyStartPosition, destination, t / partyMoveDuration);

            yield return null;
        }
        partyTransform.position = destination;

        partyParallaxLayer.SetParallaxMode();

        transitionEventChannel.Raise(TransitionPhase.IntroPartyMoveEnd);
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

        _isInTransition = false;
        RaiseCompleted();
    }

    public void StartIntroTransition()
    {
        if (_isInTransition)
        {
            return;
        }

        _isInTransition = true;
        RaiseStarted();
        transitionEventChannel.Raise(TransitionPhase.IntroTransitionBegin);

        if (partyTransform != null)
        {
            StartCoroutine(IntroRoutine());
        }

        if (boardTransform != null)
        {
            StartCoroutine(BoardRoutine());
        }

        if (uiTransforms.Length > 0 && uiArrivalPositions.Length == uiTransforms.Length)
        {
            StartCoroutine(UIRoutine());
        }

        if (menuTransform != null)
        {
            StartCoroutine(MenuRoutine());
        }
    }

    protected IEnumerator MenuRoutine()
    {
        if (menuTransform.gameObject.activeSelf == false)
        {
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
    }
}
