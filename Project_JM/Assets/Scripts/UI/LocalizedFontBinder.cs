// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LocalizedFontBinder.cs
// Summary: Applies the selected locale's font to a TextMesh Pro label.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TMPro;
using UnityEngine;
using UnityEngine.Localization;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public class LocalizedFontBinder : MonoBehaviour
{
    [SerializeField] private LocalizedTmpFont localizedFont = new LocalizedTmpFont
    {
        TableReference = "Fonts",
        TableEntryReference = "UI"
    };

    private TMP_Text _text;

    private void Awake() => _text = GetComponent<TMP_Text>();

    private void OnEnable() => localizedFont.AssetChanged += ApplyFont;

    private void OnDisable() => localizedFont.AssetChanged -= ApplyFont;

    private void ApplyFont(TMP_FontAsset font)
    {
        if (font == null || _text.font == font)
        {
            return;
        }

        _text.font = font;
        _text.SetAllDirty();
    }
}
