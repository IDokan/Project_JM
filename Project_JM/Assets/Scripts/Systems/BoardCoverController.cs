// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/25/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardCoverController.cs
// Summary: A script to control board cover.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using DG.Tweening;

public class BoardCoverController : MonoBehaviour
{

    [SerializeField] protected GameObject coverPrefab;
    [SerializeField] protected float slideDuration = 0.5f;
    [SerializeField] protected float landscapeHiddenY = -800f;       // Off-screen bottom
    [SerializeField] protected float portraitHiddenY = -800f;
    [SerializeField] protected float landscapeShownY = 0f;           // On-screen
    [SerializeField] protected float portraitShownY = 0f;
    protected Vector3 _hiddenLocation;
    protected Vector3 _shownLocation;

    protected float _hiddenY;
    protected float _shownY;

    protected GameObject _instance;
    protected Tweener _tween;

    protected void Awake()
    {
        bool isPortrait = Screen.height > Screen.width;
        _hiddenY = isPortrait ? portraitHiddenY : landscapeHiddenY;
        _shownY = isPortrait ? portraitShownY : landscapeShownY;

        _instance = Instantiate(coverPrefab, transform);

        _instance.transform.localPosition = _hiddenLocation;
    }

    public void SetBoardSizeData(int row, int col, float cellSize, float spacing, bool startsHide = false)
    {
        float gemSize = (cellSize + spacing);
        float width = col * gemSize;
        float height = row * gemSize;
        Vector3 centerLocation = new Vector3(width / 2f - (gemSize / 2f), height / 2f - (gemSize / 2f), 0f);

        _instance.transform.localScale = new Vector3(width, height, 0f);
        _hiddenLocation = new Vector3(centerLocation.x, centerLocation.y + _hiddenY, 0f);
        _shownLocation = new Vector3(centerLocation.x, centerLocation.y + _shownY, 0f);

        _instance.transform.localPosition = startsHide ? _hiddenLocation : _shownLocation;
    }

    public void ShowCover()
    {
        if (_instance == null) return;

        Transform t = _instance.transform;
        _tween?.Kill();
        _tween = t.DOLocalMove(_shownLocation, slideDuration)
                 .SetEase(Ease.OutCubic)
                 .SetLink(_instance);
    }

    public void HideCover()
    {
        if (_instance == null) return;

        Transform t = _instance.transform;
        _tween?.Kill();
        _tween = t.DOLocalMove(_hiddenLocation, slideDuration)
                 .SetEase(Ease.InCubic)
                 .SetLink(_instance);
    }
}
