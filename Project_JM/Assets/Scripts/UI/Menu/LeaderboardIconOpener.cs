// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 30/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LeaderboardIconOpener.cs
// Summary: Manages one leaderboard icon slot: opens the selector on click, persists the
//          chosen icon, and plays an opacity pulse on the indicator image to signal a change.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardIconOpener : MonoBehaviour
{
    [SerializeField] private Image indicatorImage;
    [SerializeField] private IconSelectorMenu iconSelectorMenu;
    [SerializeField] private LeaderboardIconContainer iconContainer;

    [SerializeField] private Image iconImage;

    private Button _button;
    private int _slotIndex;
    private int _selectedId;

    public int SelectedId => _selectedId;

    private void Awake()
    {
        _button = GetComponent<Button>();

        Color color = indicatorImage.color;
        color.a = 0f;
        indicatorImage.color = color;
    }

    protected void OnEnable() => _button.onClick.AddListener(OnButtonClicked);
    protected void OnDisable() => _button.onClick.RemoveListener(OnButtonClicked);

    public void Init(int slotIndex, int initialId)
    {
        _slotIndex = slotIndex;
        _selectedId = initialId;
        UpdateIcon();
    }

    private void OnButtonClicked()
    {
        iconSelectorMenu.Open(_button, _selectedId, OnIconSelected);
    }

    private void OnIconSelected(int id)
    {
        _selectedId = id;
        UpdateIcon();
        PlayerPrefs.SetInt(LeaderboardIconPrefs.Keys[_slotIndex], id);
        PlayChangeAnimation();
    }

    private void UpdateIcon()
    {
        iconImage.sprite = iconContainer.GetSprite(_selectedId);
        iconImage.SetNativeSize();
    }

    private void PlayChangeAnimation()
    {
        indicatorImage.DOKill();
        indicatorImage.transform.DOKill();
        indicatorImage.transform.localEulerAngles = Vector3.zero;

        Sequence seq = DOTween.Sequence();
        seq.Append(indicatorImage.DOFade(1f, 0f));
        seq.Append(indicatorImage.DOFade(0.5f, 0.5f));
        seq.Append(indicatorImage.DOFade(1f, 0.5f));
        seq.Append(indicatorImage.DOFade(0f, 1f));
        seq.Insert(0f, indicatorImage.transform
            .DORotate(new Vector3(0f, 0f, -360f), 2f, RotateMode.FastBeyond360)
            .SetEase(Ease.Linear));
        seq.SetUpdate(true).SetLink(indicatorImage.gameObject);
    }
}
