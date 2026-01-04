// SPDX-License-Identifier: MIT
// Copyright (c) 11/25/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BoardCoverController.cs
// Summary: A script to control board cover.

using UnityEngine;
using DG.Tweening;

public class BoardCoverController : MonoBehaviour
{

    [SerializeField] protected GameObject coverPrefab;
    [SerializeField] protected float slideDuration = 0.5f;
    [SerializeField] protected float hiddenY = -800f;       // Off-screen bottom
    [SerializeField] protected float shownY = 0f;           // On-screen
    protected Vector3 _hiddenLocation;
    protected Vector3 _shownLocation;

    protected GameObject _instance;
    protected Tweener _tween;

    protected void Awake()
    {
        _instance = Instantiate(coverPrefab, transform);

        _instance.transform.localPosition = _hiddenLocation;
    }

    public void SetBoardSizeData(int row, int col, float cellSize, float spacing)
    {
        float gemSize = (cellSize + spacing);
        float width = col * gemSize;
        float height = row * gemSize;
        Vector3 centerLocation = new Vector3(width / 2f - (gemSize / 2f), height / 2f - (gemSize / 2f), 0f);

        _instance.transform.localScale = new Vector3(width, height, 0f);
        _hiddenLocation = new Vector3(centerLocation.x, centerLocation.y + hiddenY, 0f);
        _shownLocation = new Vector3(centerLocation.x, centerLocation.y + shownY, 0f);

        _instance.transform.localPosition = _hiddenLocation;
    }

    public void ShowCover()
    {
        if (_instance == null) return;

        Transform t = _instance.transform;
        _tween?.Kill();
        _tween = t.DOLocalMove(_shownLocation, slideDuration)
                 .SetEase(Ease.OutCubic);
    }

    public void HideCover()
    {
        if (_instance == null) return;

        Transform t = _instance.transform;
        _tween?.Kill();
        _tween = t.DOLocalMove(_hiddenLocation, slideDuration)
                 .SetEase(Ease.InCubic);
    }
}
