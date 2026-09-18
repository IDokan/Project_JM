// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StatsMenu.cs
// Summary: Stats overlay menu opened from the main menu; internal tabs (score/enemy/jewel/character)
//          are switched by TabButton/TabGroup within the panel and explain their records with tooltips.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;
using UnityEngine.Localization;

public class StatsMenu : Menu
{
    [Serializable]
    private struct SectionTooltip
    {
        public TabButton tab;
        public LocalizedString title;
        public LocalizedString description;
    }

    [Header("Tooltips")]
    [SerializeField] private GameObject tooltipPresenterPrefab;
    [SerializeField] private SectionTooltip[] sectionTooltips;

    private TooltipPresenter _tooltipPresenter;
    private GameObject _tooltipPresenterInstance;
    private TooltipTrigger[] _tooltipTriggers;

    protected override void Awake()
    {
        base.Awake();
        CreateSectionTooltips();
    }

    public override void Hide()
    {
        DismissTooltips();
        base.Hide();
    }

    private void OnDestroy()
    {
        if (_tooltipPresenterInstance != null)
        {
            Destroy(_tooltipPresenterInstance);
        }
    }

    private void CreateSectionTooltips()
    {
        if (tooltipPresenterPrefab == null)
        {
            Debug.LogError(
                $"[{nameof(StatsMenu)}] A tooltip presenter prefab is required.",
                this);
            return;
        }

        _tooltipPresenterInstance = Instantiate(tooltipPresenterPrefab, transform.root);
        _tooltipPresenter = _tooltipPresenterInstance.GetComponentInChildren<TooltipPresenter>();
        if (_tooltipPresenter == null)
        {
            Debug.LogError(
                $"[{nameof(StatsMenu)}] The tooltip presenter prefab needs a {nameof(TooltipPresenter)}.",
                _tooltipPresenterInstance);
            Destroy(_tooltipPresenterInstance);
            _tooltipPresenterInstance = null;
            return;
        }

        _tooltipTriggers = new TooltipTrigger[sectionTooltips.Length];
        for (int i = 0; i < sectionTooltips.Length; i++)
        {
            SectionTooltip sectionTooltip = sectionTooltips[i];
            if (sectionTooltip.tab == null)
            {
                continue;
            }

            TooltipTrigger tooltipTrigger = sectionTooltip.tab.GetComponent<TooltipTrigger>();
            if (tooltipTrigger == null)
            {
                tooltipTrigger = sectionTooltip.tab.gameObject.AddComponent<TooltipTrigger>();
            }

            tooltipTrigger.SetPresenter(_tooltipPresenter);
            tooltipTrigger.SetContent(sectionTooltip.title, sectionTooltip.description);
            _tooltipTriggers[i] = tooltipTrigger;
        }
    }

    private void DismissTooltips()
    {
        if (_tooltipTriggers == null)
        {
            return;
        }

        for (int i = 0; i < _tooltipTriggers.Length; i++)
        {
            if (_tooltipTriggers[i] != null)
            {
                _tooltipTriggers[i].Dismiss();
            }
        }
    }
}
