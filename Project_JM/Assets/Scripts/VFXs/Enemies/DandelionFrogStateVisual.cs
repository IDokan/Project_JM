// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DandelionFrogStateVisual.cs
// Summary: DandelionFrog enemy visual — eye blink sequence on enrage/stun-end/win/attack, enrage particle on enrage (stopped on death or win), static eye on stun/death, mouth change during attack, no lunge motion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class DandelionFrogStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver leftEyeSpriteResolver;
    [SerializeField] private SpriteResolver rightEyeSpriteResolver;
    [SerializeField] private SpriteResolver mouthSpriteResolver;
    [SerializeField] private ParticleSystem enrageParticle;

    [Header("Eye Labels")]
    [SerializeField] private string leftEyeCategory;
    [SerializeField] private string rightEyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string halfEyeLabel;
    [SerializeField] private string closedEyeLabel;
    [SerializeField] private string stunnedEyeLabel;
    [SerializeField] private string deadEyeLabel;

    [Header("Eye Blink")]
    [SerializeField] private float eyeBlinkInterval = 0.5f;

    [Header("Mouth Labels")]
    [SerializeField] private string mouthCategory;
    [SerializeField] private string normalMouthLabel;
    [SerializeField] private string attackMouthLabel;

    private Coroutine _eyeBlinkRoutine;

    public override void OnEnraged()
    {
        BlinkToNormal();
        enrageParticle?.Play();
    }

    public override void OnStunBegin()
    {
        StopBlink();
        SetEye(stunnedEyeLabel);
    }

    public override void OnStunEnd()
    {
        BlinkToNormal();
    }

    public override void OnDied()
    {
        StopBlink();
        SetEye(deadEyeLabel);
        enrageParticle?.Stop();
    }

    public override void OnWin()
    {
        BlinkToNormal();
        enrageParticle?.Stop();
    }

    public override Sequence BuildAttackSequence(Vector3 moveOffset)
    {
        SetMouth(attackMouthLabel);
        BlinkToNormal();
        return null;
    }

    public override void OnAttackEnd()
    {
        SetMouth(normalMouthLabel);
    }

    private void BlinkToNormal()
    {
        if (_eyeBlinkRoutine != null)
        {
            StopCoroutine(_eyeBlinkRoutine);
        }
        _eyeBlinkRoutine = StartCoroutine(EyeBlinkSequence());
    }

    private void StopBlink()
    {
        if (_eyeBlinkRoutine != null)
        {
            StopCoroutine(_eyeBlinkRoutine);
            _eyeBlinkRoutine = null;
        }
    }

    private IEnumerator EyeBlinkSequence()
    {
        SetEye(halfEyeLabel);
        yield return GlobalTimeManager.WaitForGlobalSeconds(eyeBlinkInterval);
        SetEye(closedEyeLabel);
        yield return GlobalTimeManager.WaitForGlobalSeconds(eyeBlinkInterval);
        SetEye(halfEyeLabel);
        yield return GlobalTimeManager.WaitForGlobalSeconds(eyeBlinkInterval);
        SetEye(normalEyeLabel);
        _eyeBlinkRoutine = null;
    }

    private void SetEye(string label)
    {
        leftEyeSpriteResolver.SetCategoryAndLabel(leftEyeCategory, label);
        rightEyeSpriteResolver.SetCategoryAndLabel(rightEyeCategory, label);
    }

    private void SetMouth(string label)
    {
        mouthSpriteResolver.SetCategoryAndLabel(mouthCategory, label);
    }
}
