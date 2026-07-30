// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 29/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: IconSelectorMenu.cs
// Summary: Scrollable icon-grid menu for picking a leaderboard icon. Icons are sorted by ID
//          and separated into visual row-groups every IdsPerCategory IDs. Clicking an icon
//          immediately fires onIconSelected and closes the menu. Navigating focus (keyboard/
//          gamepad) only centres the viewport without applying a selection. Half-viewport
//          padding ensures edge icons can be centred.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IconSelectorMenu : Menu
{
    private const int IdsPerCategory = 250;

    private struct GridIcon
    {
        public int id;
        public int slotIndex;
        public Button button;
    }

    [Header("Icon Selector")]
    [SerializeField] private LeaderboardIconContainer iconContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private Button iconButtonPrefab;

    [Header("Layout")]
    [SerializeField] private int columns = 10;
    [SerializeField] private float scrollDuration = 0.25f;
    [SerializeField] private InputActionReference pressAction;

    private readonly List<GridIcon> _icons = new List<GridIcon>();
    private readonly Dictionary<int, Button> _slotToButton = new Dictionary<int, Button>();
    private int _selectedId = -1;
    private Action<int> _onIconSelected;

    protected override void Awake()
    {
        base.Awake();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;
    }

    private void Start()
    {
        BuildGrid();
        WireNavigation();
    }

    public override Selectable GetFirstSelectable()
    {
        foreach (GridIcon icon in _icons)
        {
            if (icon.id == _selectedId)
            {
                return icon.button;
            }
        }
        return base.GetFirstSelectable();
    }

    public void Open(Selectable returnTo, int initialId, Action<int> onIconSelected)
    {
        _selectedId = initialId;
        _onIconSelected = onIconSelected;
        Show(returnTo);
    }

    public override void Show(Selectable returnTo)
    {
        base.Show(returnTo);
        StartCoroutine(InitialCenterRoutine());
    }

    private IEnumerator InitialCenterRoutine()
    {
        // Wait one frame so DOTween advances localScale off zero.
        // ScrollRect.SetNormalizedPosition uses world-space bounds internally;
        // with scale = 0 all world corners collapse and centering produces garbage.
        yield return null;
        ApplyPadding();
        Canvas.ForceUpdateCanvases();
        CenterOnItem(_selectedId, animate: false);
    }

    private void BuildGrid()
    {
        List<List<LeaderboardIconContainer.Entry>> categories = GroupByCategory(iconContainer.GetEntries());

        int slotIndex = 0;

        for (int c = 0; c < categories.Count; c++)
        {
            if (c > 0)
            {
                int remainder = slotIndex % columns;
                int separatorCount = remainder == 0 ? 0 : columns - remainder;
                for (int i = 0; i < separatorCount; i++)
                {
                    InsertSpacer();
                    slotIndex++;
                }
            }

            List<LeaderboardIconContainer.Entry> category = categories[c];
            int fullRowCount = (category.Count / columns) * columns;
            int lastRowCount = category.Count % columns;

            for (int i = 0; i < fullRowCount; i++)
            {
                SpawnIconButton(category[i], slotIndex);
                slotIndex++;
            }

            if (lastRowCount > 0)
            {
                int leadingSpacers = (columns - lastRowCount) / 2;
                for (int i = 0; i < leadingSpacers; i++)
                {
                    InsertSpacer();
                    slotIndex++;
                }

                for (int i = fullRowCount; i < category.Count; i++)
                {
                    SpawnIconButton(category[i], slotIndex);
                    slotIndex++;
                }
            }
        }
    }

    private List<List<LeaderboardIconContainer.Entry>> GroupByCategory(IReadOnlyList<LeaderboardIconContainer.Entry> entries)
    {
        List<List<LeaderboardIconContainer.Entry>> result = new List<List<LeaderboardIconContainer.Entry>>();
        int prevCategory = -1;

        foreach (LeaderboardIconContainer.Entry entry in entries)
        {
            int category = entry.id / IdsPerCategory;
            if (category != prevCategory)
            {
                result.Add(new List<LeaderboardIconContainer.Entry>());
                prevCategory = category;
            }
            result[result.Count - 1].Add(entry);
        }

        return result;
    }

    private void SpawnIconButton(LeaderboardIconContainer.Entry entry, int slotIndex)
    {
        Button button = Instantiate(iconButtonPrefab, content);
        int capturedId = entry.id;
        button.onClick.AddListener(() => OnIconClicked(capturedId));
        button.GetComponent<LeaderboardIconSelectNotifier>().Init(capturedId, OnButtonNavigated);

        Image icon = null;
        foreach (Transform child in button.transform)
        {
            icon = child.GetComponent<Image>();
            if (icon != null)
            {
                break;
            }
        }

        if (icon != null)
        {
            icon.sprite = entry.sprite;
            icon.SetNativeSize();
        }

        _slotToButton[slotIndex] = button;
        _icons.Add(new GridIcon { id = entry.id, slotIndex = slotIndex, button = button });
    }

    private void InsertSpacer()
    {
        GameObject spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(content, false);
    }

    private void WireNavigation()
    {
        int maxSlot = 0;
        foreach (GridIcon icon in _icons)
        {
            if (icon.slotIndex > maxSlot) { maxSlot = icon.slotIndex; }
        }

        foreach (GridIcon icon in _icons)
        {
            int s = icon.slotIndex;
            Navigation nav = icon.button.navigation;
            nav.mode = Navigation.Mode.Explicit;
            nav.selectOnLeft = s % columns > 0 ? GetButtonAt(s - 1) : null;
            nav.selectOnRight = s % columns < columns - 1 ? GetButtonAt(s + 1) : null;
            nav.selectOnUp = FindButtonInDirection(s, -1, 0);
            nav.selectOnDown = FindButtonInDirection(s, +1, maxSlot);
            icon.button.navigation = nav;
        }
    }

    private Button GetButtonAt(int slotIndex)
    {
        _slotToButton.TryGetValue(slotIndex, out Button button);
        return button;
    }

    private Button FindButtonInDirection(int startSlot, int rowStep, int bound)
    {
        int currentRow = startSlot / columns + rowStep;
        int limitRow = bound / columns;
        while (rowStep < 0 ? currentRow >= limitRow : currentRow <= limitRow)
        {
            int rowStart = currentRow * columns;
            for (int col = 0; col < columns; col++)
            {
                if (_slotToButton.ContainsKey(rowStart + col))
                {
                    return FindClosestInRow(rowStart, startSlot % columns);
                }
            }
            currentRow += rowStep;
        }
        return null;
    }

    private Button FindClosestInRow(int rowStart, int targetCol)
    {
        Button best = null;
        int bestDist = int.MaxValue;
        for (int col = 0; col < columns; col++)
        {
            if (_slotToButton.TryGetValue(rowStart + col, out Button button))
            {
                int dist = Mathf.Abs(col - targetCol);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = button;
                }
            }
        }
        return best;
    }

    private void ApplyPadding()
    {
        float halfW = scrollRect.viewport.rect.width * 0.5f;
        float halfH = scrollRect.viewport.rect.height * 0.5f;
        float halfCellW = gridLayout.cellSize.x * 0.5f;
        float halfCellH = gridLayout.cellSize.y * 0.5f;

        gridLayout.padding = new RectOffset(
            Mathf.RoundToInt(halfW - halfCellW),
            Mathf.RoundToInt(halfW - halfCellW),
            Mathf.RoundToInt(halfH - halfCellH),
            Mathf.RoundToInt(halfH - halfCellH)
        );
    }

    private void OnButtonNavigated(int id)
    {
        if (pressAction == null || !pressAction.action.IsPressed())
        {
            CenterOnItem(id, animate: true);
        }
    }

    private void OnIconClicked(int id)
    {
        _selectedId = id;

        _onIconSelected?.Invoke(_selectedId);
        Hide();
    }

    private void CenterOnItem(int id, bool animate)
    {
        if (!TryGetSlotIndex(id, out int slotIndex))
        {
            return;
        }

        int row = slotIndex / columns;
        int col = slotIndex % columns;

        // Cell centre in content local space — uses only rect values, safe during scale animation
        float cellCenterX = gridLayout.padding.left
            + col * (gridLayout.cellSize.x + gridLayout.spacing.x)
            + gridLayout.cellSize.x * 0.5f;
        float cellCenterY = gridLayout.padding.top
            + row * (gridLayout.cellSize.y + gridLayout.spacing.y)
            + gridLayout.cellSize.y * 0.5f;

        float viewportW = scrollRect.viewport.rect.width;
        float viewportH = scrollRect.viewport.rect.height;
        float scrollableW = content.rect.width - viewportW;
        float scrollableH = content.rect.height - viewportH;

        float targetH = scrollableW > 0f
            ? Mathf.Clamp01((cellCenterX - viewportW * 0.5f) / scrollableW)
            : 0.5f;
        float targetV = scrollableH > 0f
            ? 1f - Mathf.Clamp01((cellCenterY - viewportH * 0.5f) / scrollableH)
            : 0.5f;

        scrollRect.DOKill();
        if (animate)
        {
            scrollRect.DOHorizontalNormalizedPos(targetH, scrollDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .SetLink(scrollRect.gameObject);
            scrollRect.DOVerticalNormalizedPos(targetV, scrollDuration)
                .SetEase(Ease.OutCubic)
                .SetUpdate(true)
                .SetLink(scrollRect.gameObject);
        }
        else
        {
            scrollRect.horizontalNormalizedPosition = targetH;
            scrollRect.verticalNormalizedPosition = targetV;
        }
    }

    private bool TryGetSlotIndex(int id, out int slotIndex)
    {
        foreach (GridIcon icon in _icons)
        {
            if (icon.id == id)
            {
                slotIndex = icon.slotIndex;
                return true;
            }
        }

        slotIndex = -1;
        return false;
    }
}
