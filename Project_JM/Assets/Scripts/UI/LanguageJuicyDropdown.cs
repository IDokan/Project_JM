// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/09/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: LanguageJuicyDropdown.cs
// Summary: Assigns a dedicated font to each language option and selected caption.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageJuicyDropdown : JuicyDropdown
{
    private readonly List<TMP_FontAsset> _optionFonts = new List<TMP_FontAsset>();
    private int _nextItemFontIndex;

    public void SetOptionFonts(IReadOnlyList<TMP_FontAsset> fonts)
    {
        _optionFonts.Clear();
        if (fonts != null)
        {
            for (int i = 0; i < fonts.Count; ++i)
            {
                _optionFonts.Add(fonts[i]);
            }
        }

        ApplySelectedOptionFont(value);
    }

    public void ApplySelectedOptionFont(int index)
    {
        if (captionText == null || index < 0 || index >= _optionFonts.Count)
        {
            return;
        }

        TMP_FontAsset font = _optionFonts[index];
        if (font != null)
        {
            captionText.font = font;
            captionText.SetAllDirty();
        }
    }

    protected override GameObject CreateDropdownList(GameObject template)
    {
        _nextItemFontIndex = 0;
        return base.CreateDropdownList(template);
    }

    protected override DropdownItem CreateItem(DropdownItem itemTemplate)
    {
        DropdownItem item = base.CreateItem(itemTemplate);
        if (item.text != null && _nextItemFontIndex < _optionFonts.Count)
        {
            TMP_FontAsset font = _optionFonts[_nextItemFontIndex];
            if (font != null)
            {
                item.text.font = font;
            }
        }

        ++_nextItemFontIndex;
        return item;
    }
}
