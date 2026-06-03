// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DifficultyCurvesSelector.cs
// Summary: Selects the active DifficultyCurves based on save data progress.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using TutorialEnums;
using UnityEngine;

public class DifficultyCurvesSelector : MonoBehaviour
{
    [SerializeField] private SaveDataManager saveDataManager;
    [SerializeField] private DifficultyCurves tutorialCurves;
    [SerializeField] private DifficultyCurves rankingCurves;

    public DifficultyCurves ActiveCurves { get; private set; }

    private void Awake()
    {
        if (saveDataManager == null)
        {
            Debug.LogError("SaveDataManager is not assigned.", this);
            enabled = false;
            return;
        }

        ActiveCurves = saveDataManager.Progress >= TutorialProgress.MediumPassed
            ? rankingCurves
            : tutorialCurves;
    }
}
