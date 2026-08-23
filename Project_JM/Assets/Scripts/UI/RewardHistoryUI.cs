// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/08/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: RewardHistoryUI.cs
// Summary: Slides the reward history panel out at RewardTransitionStarts,
//          syncing any icons chosen since it was last shown into a
//          GridLayoutGroup that fills top-to-bottom (wrapping to a new
//          column past maxItemsPerColumn), then hides it again on
//          RewardChosen - same slide/fade structure as DamageRecordUIManager.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RewardHistoryUI : MonoBehaviour
{
    [SerializeField] protected TransitionEventChannel transitionEventChannel;
    [SerializeField] protected RewardManager rewardManager;

    // The wrapper cell prefab GridLayoutGroup positions; its nested Image
    // (found via GetComponentInChildren) is set to the reward's sprite at
    // native size, independent of the cell's own uniform GridLayoutGroup size.
    [SerializeField] protected GameObject iconPrefab;
    [SerializeField] protected Transform content;
    [SerializeField] protected GridLayoutGroup gridLayoutGroup;

    // innerBackground hugs the grid via its own ContentSizeFitter (fitting
    // around content); outerBackground adds a fixed backgroundPadding margin
    // on every side around whatever size that resolves to.
    [SerializeField] protected RectTransform outerBackground;
    [SerializeField] protected RectTransform innerBackground;
    [SerializeField] protected float backgroundPadding = 10f;

    // GridLayoutGroup's constraint must be set to Fixed Row Count in the
    // Editor; constraintCount is then driven by UpdateLayout below rather
    // than authored, so a lone icon gets a 1-row grid instead of reserving
    // maxItemsPerColumn rows of empty space.
    [SerializeField] protected int maxItemsPerColumn = 6;

    [SerializeField] protected float landscapeHiddenX = -5.9f;
    [SerializeField] protected float portraitHiddenX = -5.9f;
    [SerializeField] protected float landscapeShownX = -1.9f;
    [SerializeField] protected float portraitShownX = -1.9f;
    [SerializeField] protected float slideDuration = 0.5f;

    protected float _hiddenX;
    protected float _shownX;

    protected RectTransform _rectTransform;
    protected CanvasGroup _canvasGroup;
    protected Tweener _tween;

    protected void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();

        bool isPortrait = Screen.height > Screen.width;
        _hiddenX = isPortrait ? portraitHiddenX : landscapeHiddenX;
        _shownX = isPortrait ? portraitShownX : landscapeShownX;

        Vector2 anchoredPosition = _rectTransform.anchoredPosition;
        anchoredPosition.x = _hiddenX;
        _rectTransform.anchoredPosition = anchoredPosition;

        _canvasGroup.alpha = 0;
    }

    protected void OnEnable()
    {
        transitionEventChannel.OnRaised += OnTransitionEvent;
    }

    protected void OnDisable()
    {
        transitionEventChannel.OnRaised -= OnTransitionEvent;
    }

    protected void OnTransitionEvent(TransitionPhase phase)
    {
        if (phase == TransitionPhase.RewardTransitionStarts)
        {
            SyncIcons();

            if (rewardManager.ChosenIcons.Count > 0)
            {
                ShowHistory();
            }
        }
        else if (phase == TransitionPhase.RewardChosen)
        {
            if (rewardManager.ChosenIcons.Count > 0)
            {
                HideHistory();
            }
        }
    }

    // Adds any reward icons chosen since content was last synced - picks
    // made while this panel was hidden (RewardManager.ChooseReward runs
    // regardless of visibility) only need to appear the next time the panel
    // is about to show, not the instant they're picked.
    protected void SyncIcons()
    {
        IReadOnlyList<Sprite> chosenIcons = rewardManager.ChosenIcons;
        for (int i = content.childCount; i < chosenIcons.Count; i++)
        {
            GameObject instance = Instantiate(iconPrefab, content);
            Image iconImage = instance.GetComponentInChildren<Image>();
            iconImage.sprite = chosenIcons[i];
            iconImage.SetNativeSize();
        }

        UpdateLayout(chosenIcons.Count);
    }

    protected void UpdateLayout(int count)
    {
        int rows = count > 0 ? Mathf.Min(count, maxItemsPerColumn) : 1;
        gridLayoutGroup.constraintCount = rows;

        // innerBackground's ContentSizeFitter only recomputes during Unity's
        // deferred layout rebuild, so force it now rather than reading a
        // size from before this sync's icons were added.
        LayoutRebuilder.ForceRebuildLayoutImmediate(innerBackground);

        Vector2 innerSize = innerBackground.rect.size;
        outerBackground.sizeDelta = new Vector2(innerSize.x + backgroundPadding * 2f, innerSize.y + backgroundPadding * 2f);
    }

    protected void ShowHistory()
    {
        _tween?.Kill();
        _tween = _rectTransform.DOAnchorPosX(_shownX, slideDuration).SetEase(Ease.OutCubic)
            .SetLink(gameObject);

        _canvasGroup.alpha = 1;
    }

    protected void HideHistory()
    {
        _tween?.Kill();
        _tween = _rectTransform.DOAnchorPosX(_hiddenX, slideDuration).SetEase(Ease.InCubic)
            .OnComplete(() => _canvasGroup.alpha = 0)
            .SetLink(gameObject);
    }
}
