// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 06/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAlertUI.cs
// Summary: Shows the incoming enemy's icon and the party icons whose gem color has an
//          advantage against it, sliding into view only for the duration of the middle transition.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using GemEnums;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAlertUI : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected EnemyAlertEventChannel enemyAlertEventChannel;

    [SerializeField] protected Image enemyIconImage;
    [SerializeField] protected Image advantageIconA;
    [SerializeField] protected Image advantageIconB;

    [SerializeField] protected GemColorIconData gemColorIconData;

    [SerializeField] protected float hiddenX = 420f;
    [SerializeField] protected float shownX = 20f;
    [SerializeField] protected float slideDuration = 0.5f;

    protected Tweener _tween;

    protected void Awake()
    {
        Vector3 localPosition = transform.localPosition;
        localPosition.x = hiddenX;
        transform.localPosition = localPosition;
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
        enemyAlertEventChannel.OnRaised += OnEnemyAlert;
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
        enemyAlertEventChannel.OnRaised -= OnEnemyAlert;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.MiddleTransitionStarts)
        {
            ShowAlert();
        }
        else if (phase == TransitionPhase.MiddleTransitionEnd)
        {
            HideAlert();
        }
    }

    protected void ShowAlert()
    {
        _tween?.Kill();
        _tween = transform.DOLocalMoveX(shownX, slideDuration).SetEase(Ease.OutCubic);
    }

    protected void HideAlert()
    {
        _tween?.Kill();
        _tween = transform.DOLocalMoveX(hiddenX, slideDuration).SetEase(Ease.InCubic);
    }

    protected void OnEnemyAlert(GameObject enemyPrefab)
    {
        if (enemyPrefab.TryGetComponent<CharacterStatus>(out CharacterStatus status))
        {
            enemyIconImage.sprite = status.Icon;
            enemyIconImage.enabled = enemyIconImage.sprite != null;
            enemyIconImage.SetNativeSize();
        }

        GemColor[] colors = enemyPrefab.TryGetComponent<CharacterCombatant>(out CharacterCombatant combatant)
            ? combatant.Colors
            : null;

        SetAdvantageIcon(advantageIconA, colors, 0);
        SetAdvantageIcon(advantageIconB, colors, 1);
    }

    protected void SetAdvantageIcon(Image image, GemColor[] colors, int index)
    {
        Sprite sprite = colors != null && index < colors.Length ? gemColorIconData.GetIconByColor(colors[index]) : null;
        image.sprite = sprite;
        image.enabled = sprite != null;
        image.SetNativeSize();
    }
}
