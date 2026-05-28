// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 14/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageBonusActivator.cs
// Summary: Activates a target object and fades in its child Graphics when a
//          damage bonus is applied; fades out and deactivates when bonus clears.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageBonusActivator : MonoBehaviour
{
    [SerializeField] private DamageBonusChangedEventChannel damageBonusChangedEventChannel;
    [SerializeField] private GameObject target;
    [SerializeField] private float fadeDuration = 0.3f;

    private Graphic[] _graphics;
    private Coroutine _fadeRoutine;

    protected void Awake()
    {
        _graphics = target.GetComponentsInChildren<Graphic>(true);
    }

    protected void OnEnable()  => damageBonusChangedEventChannel.OnRaised += OnDamageBonusChanged;
    protected void OnDisable() => damageBonusChangedEventChannel.OnRaised -= OnDamageBonusChanged;

    private void OnDamageBonusChanged(float newBonus)
    {
        if (newBonus > 1f)
        {
            target.SetActive(true);
            StartFade(from: 0f, to: 1f, deactivateOnComplete: false);
        }
        else
        {
            StartFade(from: 1f, to: 0f, deactivateOnComplete: true);
        }
    }

    private void StartFade(float from, float to, bool deactivateOnComplete)
    {
        if (_fadeRoutine != null)
        {
            StopCoroutine(_fadeRoutine);
        }
        _fadeRoutine = StartCoroutine(FadeRoutine(from, to, deactivateOnComplete));
    }

    private IEnumerator FadeRoutine(float from, float to, bool deactivateOnComplete)
    {
        SetAlpha(from);

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(time / fadeDuration)));
            yield return null;
        }

        SetAlpha(to);
        _fadeRoutine = null;

        if (deactivateOnComplete)
        {
            target.SetActive(false);
        }
    }

    private void SetAlpha(float alpha)
    {
        foreach (Graphic graphic in _graphics)
        {
            Color c = graphic.color;
            c.a = alpha;
            graphic.color = c;
        }
    }
}
