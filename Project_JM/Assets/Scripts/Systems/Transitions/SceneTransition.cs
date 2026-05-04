// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 06/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SceneTransition.cs
// Summary: Handles between-scene fade transitions. On Start, fades the overlay out
//          (scene enter). FadeAndLoad/FadeAndQuit fade it in then exit (scene exit).
//          Separate from TransitionManager, which owns in-scene choreography only.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private CanvasGroup fadeOverlay;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private float fadeDuration = 0.4f;
    [SerializeField] private bool fadeInOnEnter = true;

    private void Start()
    {
        if (!fadeInOnEnter)
        {
            return;
        }

        playerInput.DeactivateInput();

        fadeOverlay.alpha = 1f;
        fadeOverlay.DOFade(0f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => playerInput.ActivateInput());
    }

    public void FadeAndLoad(string sceneName)
    {
        playerInput.DeactivateInput();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        loadOp.allowSceneActivation = false;

        AudioManager.Instance.FadeOutBGM(fadeDuration);
        fadeOverlay.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() => loadOp.allowSceneActivation = true);
    }

    public void FadeAndQuit()
    {
        playerInput.DeactivateInput();

        AudioManager.Instance.FadeOutBGM(fadeDuration);
        fadeOverlay.DOFade(1f, fadeDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            });
    }
}
