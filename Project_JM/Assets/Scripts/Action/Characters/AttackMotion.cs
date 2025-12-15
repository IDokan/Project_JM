// SPDX-License-Identifier: MIT
// Copyright (c) 11/12/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: AttackMotion.cs
// Summary: A script for enemy combat behaviour.

using DG.Tweening;
using System;
using MatchEnums;
using UnityEngine;

public class AttackMotion : MonoBehaviour
{
    protected Animator animator;
    [SerializeField] protected string mat3StateName = "match 3";
    [SerializeField] protected string mat4StateName = "match 4";
    [SerializeField] protected string mat5StateName = "match 5";

    protected Vector3 originalPosition;

    protected void Awake()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void PlayAttackMotion(MatchTier matchTier)
    {
        if (animator == null)
        {
            return;
        }

        switch (matchTier)
        {
            case MatchTier.Three:
                animator.Play(mat3StateName, 0, 0f);
                break;
            case MatchTier.Four:
                animator.Play(mat4StateName, 0, 0f);
                break;
            case MatchTier.Five:
                animator.Play(mat5StateName, 0, 0f);
                break;
            default:
                break;
        }
    }
}
