// SPDX-License-Identifier: MIT
// Copyright (c) 11/12/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackMotion.cs
// Summary: A script for enemy combat behaviour.

using DG.Tweening;
using UnityEngine;

public class AttackMotion : MonoBehaviour
{
    [SerializeField] protected float _moveDuration = 0.15f;
    [SerializeField] protected float _pauseDuration = 0.2f;

    protected Vector3 originalPosition;

    protected void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void PlayAttackMotion(Vector3 offset)
    {
        Vector3 target = originalPosition + offset;
        
        Sequence seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMove(target, _moveDuration).SetEase(Ease.OutQuad))
            .AppendInterval(_pauseDuration)
            .Append(transform.DOLocalMove(originalPosition, _moveDuration).SetEase(Ease.InQuad))
            .SetLink(gameObject);
    }
}
