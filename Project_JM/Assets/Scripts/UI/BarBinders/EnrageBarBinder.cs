// SPDX-License-Identifier: MIT
// Copyright (c) 03/09/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM 
// File: EnrageBarBinder.cs
// Summary: A script to bind enrage time to a slider bar.

using UnityEngine;
using UnityEngine.UI;

public class EnrageBarBinder : MonoBehaviour
{
    [SerializeField] protected EnemyAttackBehaviour boundEnemyAI;

    [SerializeField] protected Slider slider;

    [SerializeField] protected UIHideShow uiHideShow;

    [SerializeField] protected CharacterDeathEventChannel characterDeathEventChannel;

    protected void OnEnable()
    {
        if (boundEnemyAI != null)
        {
            boundEnemyAI.OnEnrageTimeChanged += UpdateEnrage;
            characterDeathEventChannel.OnRaised += OnAnyoneDied;
        }
    }

    protected void OnDisable()
    {
        if (boundEnemyAI != null)
        {
            boundEnemyAI.OnEnrageTimeChanged -= UpdateEnrage;
            characterDeathEventChannel.OnRaised -= OnAnyoneDied;
        }
    }

    protected void Start()
    {
        UpdateEnrage(1f, 1f);
        if (uiHideShow != null)
        {
            uiHideShow.HideObjects();
        }
    }

    protected void UpdateEnrage(float current, float max)
    {
        if (current <= 0f)
        {
            if (uiHideShow != null)
            {
                uiHideShow.ShowObjects();
            }
        }

        slider.maxValue = max;
        slider.value = current;
    }

    public void BindNewAI(EnemyAttackBehaviour newAI)
    {
        if (newAI == boundEnemyAI) return;

        if (boundEnemyAI != null)
        {
            boundEnemyAI.OnEnrageTimeChanged -= UpdateEnrage;
        }

        boundEnemyAI = newAI;
        boundEnemyAI.OnEnrageTimeChanged += UpdateEnrage;

        UpdateEnrage(boundEnemyAI.EnrageDelay, boundEnemyAI.EnrageDelay);
    }

    public void OnAnyoneDied(CharacterStatus characterStatus)
    {
        if (uiHideShow != null)
        {
            uiHideShow.HideObjects();
        }
    }
}
