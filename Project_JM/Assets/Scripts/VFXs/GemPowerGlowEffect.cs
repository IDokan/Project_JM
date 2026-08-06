// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 06/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemPowerGlowEffect.cs
// Summary: Pulses a material flash effect on a character sprite. Fires on
//          two independent triggers that share this one component so
//          neither stomps the other's material properties: gem power
//          matching myColor arriving (via GemPowerArrivedEventChannel), and
//          reward absorption (RewardOfferUI calling PlayGlow() directly).
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using GemEnums;
using MatchEnums;
using UnityEngine;

public class GemPowerGlowEffect : MonoBehaviour
{
    [SerializeField] private GemPowerArrivedEventChannel channel;
    [SerializeField] private GemColor myColor;
    [SerializeField] private Color glowColor = Color.white;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float maxAmount = 0.6f;
    [SerializeField] private AnimationCurve glowCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.5f, 1f),
        new Keyframe(1f, 0f)
    );
    [SerializeField] private Material targetMaterial;

    private Material _material;
    private Coroutine _routine;

    private void Awake()
    {
        _material = targetMaterial != null ? targetMaterial : GetComponentInChildren<SpriteRenderer>()?.material;
    }

    private void OnEnable()
    {
        channel.OnRaised += OnPowerArrived;
        if (_material != null)
        {
            _material.SetFloat("_FlashAmount", 0f);
        }
    }

    private void OnDisable()
    {
        channel.OnRaised -= OnPowerArrived;
        if (_material != null)
        {
            _material.SetFloat("_FlashAmount", 0f);
        }
    }

    private void OnPowerArrived(MatchEvent e)
    {
        if (e.Color != myColor)
        {
            return;
        }

        PlayGlow();
    }

    // Starts (or restarts) the glow pulse on this character's sprite
    // material. Called both from gem-power arrival above (filtered by
    // myColor) and from reward absorption (see RewardOfferUI), so every
    // source of this character's "absorbed something" flash funnels through
    // this one coroutine instead of separate components independently
    // writing the same _FlashAmount/_FlashColor properties and fighting
    // each other.
    public void PlayGlow()
    {
        if (_material == null)
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(GlowRoutine());
    }

    private IEnumerator GlowRoutine()
    {
        _material.SetColor("_FlashColor", glowColor);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _material.SetFloat("_FlashAmount", glowCurve.Evaluate(t) * maxAmount);
            yield return null;
        }
        _material.SetFloat("_FlashAmount", 0f);
        _routine = null;
    }
}
