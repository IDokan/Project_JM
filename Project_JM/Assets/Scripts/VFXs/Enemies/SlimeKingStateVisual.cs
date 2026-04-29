// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 28/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SlimeKingStateVisual.cs
// Summary: Drives SlimeKing eye sprite changes in response to stun and enrage state transitions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.U2D.Animation;

public class SlimeKingStateVisual : MonoBehaviour
{
    [SerializeField] private EnemyAttackBehaviour enemyAttackBehaviour;
    [SerializeField] private SpriteResolver eyeSpriteResolver;
    [SerializeField] private ParticleSystem enrageParticle;

    [Header("Eye Labels")]
    [SerializeField] private string eyeCategory;
    [SerializeField] private string normalEyeLabel;
    [SerializeField] private string enragedEyeLabel;
    [SerializeField] private string stunnedEyeLabel;

    private void OnEnable()
    {
        enemyAttackBehaviour.OnEnraged += OnEnraged;
        enemyAttackBehaviour.OnStunBegin += OnStunBegin;
        enemyAttackBehaviour.OnStunEnd += OnStunEnd;
        enemyAttackBehaviour.OnDied += OnEnemyDied;
    }

    private void OnDisable()
    {
        enemyAttackBehaviour.OnEnraged -= OnEnraged;
        enemyAttackBehaviour.OnStunBegin -= OnStunBegin;
        enemyAttackBehaviour.OnStunEnd -= OnStunEnd;
        enemyAttackBehaviour.OnDied -= OnEnemyDied;
    }

    private void OnEnraged()
    {
        SetEye(enragedEyeLabel);
        enrageParticle?.Play();
    }

    private void OnStunBegin()
    {
        SetEye(stunnedEyeLabel);
    }

    private void OnStunEnd()
    {
        SetEye(enemyAttackBehaviour.IsEnraged ? enragedEyeLabel : normalEyeLabel);
    }

    private void OnEnemyDied()
    {
        SetEye(normalEyeLabel);
        enrageParticle?.Stop();
    }

    private void SetEye(string label)
    {
        eyeSpriteResolver.SetCategoryAndLabel(eyeCategory, label);
    }
}
