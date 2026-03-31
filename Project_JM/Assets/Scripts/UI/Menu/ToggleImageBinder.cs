// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 31/03/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ToggleImageBinder.cs
// Summary: Swaps an Image sprite based on a Toggle's on/off state.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class ToggleImageBinder : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image image;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Sprite offSprite;

    protected void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleValueChanged);
        Refresh();
    }

    protected void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        Refresh();
    }

    public void Refresh()
    {
        image.sprite = toggle.isOn ? onSprite : offSprite;
        image.SetNativeSize();
    }
}
