// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 22/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LanguageMenu.cs
// Summary: Populates the language dropdown and applies the selected locale.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LanguageMenu : Menu
{
    [SerializeField] private LanguageJuicyDropdown languageDropdown;

    private readonly List<Locale> _locales = new List<Locale>();
    private readonly List<TMP_FontAsset> _fonts = new List<TMP_FontAsset>();
    private bool _isInitialized;

    protected override void OnEnable()
    {
        base.OnEnable();
        languageDropdown.onValueChanged.AddListener(OnLanguageValueChanged);
        StartCoroutine(InitializeDropdown());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        languageDropdown.onValueChanged.RemoveListener(OnLanguageValueChanged);
    }

    public override void Show(Selectable returnTo)
    {
        base.Show(returnTo);
        if (_isInitialized)
        {
            SyncSelection();
        }
    }

    public override Selectable GetFirstSelectable() => languageDropdown;

    private IEnumerator InitializeDropdown()
    {
        yield return LocalizationSettings.InitializationOperation;

        _locales.Clear();
        _locales.AddRange(LocalizationSettings.AvailableLocales.Locales);
        _fonts.Clear();

        List<string> options = new List<string>(_locales.Count);
        for (int i = 0; i < _locales.Count; ++i)
        {
            options.Add(_locales[i].LocaleName);

            AsyncOperationHandle<TMP_FontAsset> fontOperation =
                LocalizationSettings.AssetDatabase.GetLocalizedAssetAsync<TMP_FontAsset>(
                    "Fonts", "UI", _locales[i]);
            yield return fontOperation;
            _fonts.Add(fontOperation.Status == AsyncOperationStatus.Succeeded
                ? fontOperation.Result
                : null);
        }

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);
        languageDropdown.SetOptionFonts(_fonts);
        _isInitialized = true;
        SyncSelection();
    }

    private void OnLanguageValueChanged(int index)
    {
        if (!_isInitialized || index < 0 || index >= _locales.Count)
        {
            return;
        }

        Locale locale = _locales[index];
        languageDropdown.ApplySelectedOptionFont(index);
        LocalizationSettings.SelectedLocale = locale;
    }

    private void SyncSelection()
    {
        Locale selectedLocale = LocalizationSettings.SelectedLocale;
        int index = _locales.IndexOf(selectedLocale);
        if (index < 0)
        {
            index = 0;
        }

        languageDropdown.SetValueWithoutNotify(index);
        languageDropdown.RefreshShownValue();
        languageDropdown.ApplySelectedOptionFont(index);
    }
}
