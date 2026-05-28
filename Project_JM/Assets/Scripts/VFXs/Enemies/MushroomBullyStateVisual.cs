// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 09/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MushroomBullyStateVisual.cs
// Summary: MushroomBully-specific visual responses to AI state changes driven by EnemyAttackMotion.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.U2D.Animation;

public class MushroomBullyStateVisual : EnemyStateVisual
{
    [SerializeField] private SpriteResolver faceSpriteResolver;
    [SerializeField] private SpriteResolver leftFingersSpriteResolver;
    [SerializeField] private SpriteResolver rightFingersSpriteResolver;

    [Header("Face Labels")]
    [SerializeField] private string faceCategory;
    [SerializeField] private string normalFaceLabel;
    [SerializeField] private string enragedFaceLabel;
    [SerializeField] private string deadFaceLabel;

    [Header("Fingers Labels")]
    [SerializeField] private string leftFingersCategory;
    [SerializeField] private string rightFingersCategory;
    [SerializeField] private string openFingersLabel;

    [Header("Attack Motion")]
    [SerializeField] private float readyDuration = 0.1f;
    [SerializeField] private float moveDuration = 0.1f;
    [SerializeField] private float pauseDuration = 0.2f;
    [SerializeField, Range(0f, 1f)] private float moveOffsetMultiplier = 1f;
    [SerializeField, Range(0f, 1f)] private float readyOffsetMultiplier = 0.5f;

    private bool _isEnraged;
    private Vector3 _originalPosition;

    private void Awake()
    {
        _originalPosition = transform.localPosition;
    }

    public override void OnEnraged()
    {
        _isEnraged = true;
        SetFace(enragedFaceLabel);
    }

    public override void OnStunBegin()
    {
        SetFace(deadFaceLabel);
    }

    public override void OnStunEnd()
    {
        SetFace(_isEnraged ? enragedFaceLabel : normalFaceLabel);
    }

    public override void OnDied()
    {
        SetFace(deadFaceLabel);
        leftFingersSpriteResolver.SetCategoryAndLabel(leftFingersCategory, openFingersLabel);
        rightFingersSpriteResolver.SetCategoryAndLabel(rightFingersCategory, openFingersLabel);
    }

    public override Sequence BuildAttackSequence(Vector3 moveOffset)
    {
        Vector3 offset = moveOffset * moveOffsetMultiplier;
        Vector3 readyPosition = _originalPosition - offset * readyOffsetMultiplier;
        Vector3 target = _originalPosition + offset;

        return DOTween.Sequence()
            .Append(transform.DOLocalMove(readyPosition, readyDuration).SetEase(Ease.OutQuad))
            .Append(transform.DOLocalMove(target, moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(pauseDuration)
            .Append(transform.DOLocalMove(_originalPosition, moveDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }

    private void SetFace(string label)
    {
        faceSpriteResolver.SetCategoryAndLabel(faceCategory, label);
    }
}
