// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/27/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GraphicsMenu.cs
// Summary: A script to perform graphics menu actions.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GraphicsMenu : Menu
{
    [Header("Graphics Modifiers")]
    [SerializeField] protected Toggle vSyncToggle;
    [SerializeField] protected Toggle fullScreenModeToggle;
    [SerializeField] protected TMP_Dropdown resolutionDropdown;
    [SerializeField] protected ToggleImageBinder fullScreenModeToggleBinder;

    protected Resolution[] _availableResolutions;
    protected List<Resolution> _uniqueResolutions;
    protected string _resolutionSignature;

    protected override void OnEnable()
    {
        base.OnEnable();

        vSyncToggle.onValueChanged.AddListener(OnVSyncToggled);
        fullScreenModeToggle.onValueChanged.AddListener(OnFullScreenModeToggled);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionValueChanged);

        RefreshDropdownsIfNeeded();
        SyncGraphicsWidgets();
        UpdateDropdownSelectionWithoutNotify();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        vSyncToggle.onValueChanged.RemoveListener(OnVSyncToggled);
        fullScreenModeToggle.onValueChanged.RemoveListener(OnFullScreenModeToggled);
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionValueChanged);
    }

    protected void OnVSyncToggled(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
    }

    protected void OnFullScreenModeToggled(bool isOn)
    {
        Screen.fullScreen = isOn;
    }

    protected void OnResolutionValueChanged(int index)
    {
        if (index < 0 || index >= _uniqueResolutions.Count)
        {
            return;
        }

        ApplyCurrentSettings();
    }

    protected void SyncGraphicsWidgets()
    {
        vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
        fullScreenModeToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        fullScreenModeToggleBinder.Refresh();
    }

    protected void InitializeDropdowns()
    {
        _availableResolutions = Screen.resolutions;

        BuildUniqueResolutionList();
        _resolutionSignature = BuildResolutionSignature(_uniqueResolutions);
        PopulateResolutionDropdown();
        UpdateDropdownSelectionWithoutNotify();
    }

    protected void BuildUniqueResolutionList()
    {
        _uniqueResolutions = new List<Resolution>(_availableResolutions.Length);

        for (int i = 0; i < _availableResolutions.Length; ++i)
        {
            Resolution r = _availableResolutions[i];
            if (r.width * 9 != r.height * 16)
            {
                continue;
            }

            bool alreadyAdded = false;
            for (int j = 0; j < _uniqueResolutions.Count; ++j)
            {
                if (_uniqueResolutions[j].width == r.width && _uniqueResolutions[j].height == r.height)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
            {
                _uniqueResolutions.Add(r);
            }
        }
    }

    protected void PopulateResolutionDropdown()
    {
        List<string> options = new List<string>(_uniqueResolutions.Count);

        for (int i = 0; i < _uniqueResolutions.Count; ++i)
        {
            options.Add($"{_uniqueResolutions[i].width} x {_uniqueResolutions[i].height}");
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    protected void ApplyCurrentSettings()
    {
        int resIndex = resolutionDropdown.value;
        if (resIndex < 0 || resIndex >= _uniqueResolutions.Count)
        {
            return;
        }

        Resolution selected = _uniqueResolutions[resIndex];
        Screen.SetResolution(selected.width, selected.height, Screen.fullScreenMode);
    }

    protected void UpdateDropdownSelectionWithoutNotify()
    {
        if (_availableResolutions == null || _availableResolutions.Length == 0 ||
            _uniqueResolutions == null || _uniqueResolutions.Count == 0)
        {
            return;
        }

        int resIndex = FindCurrentResolutionIndex();
        resolutionDropdown.SetValueWithoutNotify(resIndex);
        resolutionDropdown.RefreshShownValue();
    }

    protected int FindCurrentResolutionIndex()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        for (int i = 0; i < _uniqueResolutions.Count; ++i)
        {
            Resolution r = _uniqueResolutions[i];
            if (r.width == currentWidth && r.height == currentHeight)
            {
                return i;
            }
        }

        return 0;
    }

    // -------- Below functions are for efficient drop down construction
    protected string BuildResolutionSignature(IList<Resolution> resolutions)
    {
        if (resolutions == null || resolutions.Count <= 0)
        {
            return string.Empty;
        }

        System.Text.StringBuilder builder = new System.Text.StringBuilder(resolutions.Count * 16);

        for (int i = 0; i < resolutions.Count; ++i)
        {
            Resolution resolution = resolutions[i];

            builder.Append(resolution.width).Append('x').Append(resolution.height).Append(';');
        }

        return builder.ToString();
    }

    protected void RefreshDropdownsIfNeeded()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        if (_uniqueResolutions != null && _uniqueResolutions.Count > 0)
        {
            return;
        }

        Resolution[] currentResolutions = Screen.resolutions;
        List<Resolution> current169 = new List<Resolution>(currentResolutions.Length);
        for (int i = 0; i < currentResolutions.Length; ++i)
        {
            Resolution r = currentResolutions[i];
            if (r.width * 9 != r.height * 16)
            {
                continue;
            }

            bool alreadyAdded = false;
            for (int j = 0; j < current169.Count; ++j)
            {
                if (current169[j].width == r.width && current169[j].height == r.height)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
            {
                current169.Add(r);
            }
        }

        string currentSignature = BuildResolutionSignature(current169);
        if (currentSignature != _resolutionSignature)
        {
            InitializeDropdowns();
        }
    }


    // -------- End of functions for efficient graphics drop down.
}
