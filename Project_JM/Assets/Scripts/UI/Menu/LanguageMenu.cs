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
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class LanguageMenu : Menu
{
    [SerializeField] private LanguageJuicyDropdown languageDropdown;

    private readonly List<Locale> _locales = new List<Locale>();
    private readonly List<TMP_FontAsset> _fonts = new List<TMP_FontAsset>();
    private readonly List<AsyncOperationHandle<TMP_FontAsset>> _fontHandles =
        new List<AsyncOperationHandle<TMP_FontAsset>>();
    private Coroutine _initializationRoutine;
    private TMP_FontAsset _defaultCaptionFont;
    private bool _isInitialized;

    protected override void OnEnable()
    {
        base.OnEnable();
        languageDropdown.onValueChanged.AddListener(OnLanguageValueChanged);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        languageDropdown.onValueChanged.RemoveListener(OnLanguageValueChanged);
        if (_initializationRoutine != null)
        {
            StopCoroutine(_initializationRoutine);
            _initializationRoutine = null;
        }

        if (_fontHandles.Count > 0 || _isInitialized)
        {
            // TMP's OnDisable destroys any open option list immediately; Hide
            // would leave its font references alive throughout the fade-out.
            bool dropdownWasEnabled = languageDropdown.enabled;
            languageDropdown.enabled = false;
            languageDropdown.ClearOptions();
            languageDropdown.SetOptionFonts(null);
            if (languageDropdown.captionText != null)
            {
                languageDropdown.captionText.font = _defaultCaptionFont;
            }
            languageDropdown.interactable = false;
            languageDropdown.enabled = dropdownWasEnabled;
        }

        for (int i = 0; i < _fontHandles.Count; ++i)
        {
            AsyncOperationHandle<TMP_FontAsset> handle = _fontHandles[i];
            Locale locale = _locales[i];
            if (handle.IsDone)
            {
                ReleaseFont(handle, locale);
            }
            else
            {
                // Stopping a coroutine does not cancel Addressables. Finish cleanup
                // without accessing this menu when an outstanding request completes.
                handle.Completed += completed => ReleaseFont(completed, locale);
            }
        }

        _fontHandles.Clear();
        _fonts.Clear();
        _locales.Clear();
        _isInitialized = false;
    }

    public override void Show(Selectable returnTo)
    {
        languageDropdown.interactable = _isInitialized;
        base.Show(returnTo);
        if (_isInitialized)
        {
            SyncSelection();
        }
        else if (_initializationRoutine == null && isActiveAndEnabled)
        {
            _defaultCaptionFont = languageDropdown.captionText != null
                ? languageDropdown.captionText.font
                : null;
            _initializationRoutine = StartCoroutine(InitializeDropdown());
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
            // The Localization request auto-releases; retain our own reference
            // until OnDisable, including across selected-locale cache resets.
            Addressables.ResourceManager.Acquire(fontOperation);
            _fontHandles.Add(fontOperation);
            yield return fontOperation;
            _fonts.Add(fontOperation.Status == AsyncOperationStatus.Succeeded
                ? fontOperation.Result
                : null);
        }

        languageDropdown.ClearOptions();
        languageDropdown.AddOptions(options);
        languageDropdown.SetOptionFonts(_fonts);
        _isInitialized = true;
        _initializationRoutine = null;
        languageDropdown.interactable = true;
        SyncSelection();
    }

    private static void ReleaseFont(AsyncOperationHandle<TMP_FontAsset> handle, Locale locale)
    {
        // Keep the active locale's shared cache available to other UI. Other
        // consumers with acquired handles remain protected when caches release.
        if (locale != LocalizationSettings.SelectedLocale)
        {
            LocalizationSettings.AssetDatabase.ReleaseTable("Fonts", locale);
        }

        Addressables.Release(handle);
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
