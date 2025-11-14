// SPDX-License-Identifier: MIT
// Copyright (c) 11/13/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BarUI.cs
// Summary: A UI type of bar.

using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour
{
    [SerializeField] protected RectTransform _bar;

    protected Vector2 initSize;
    
    protected void Awake()
    {
        initSize = _bar.rect.size;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateValue(float current, float max)
    {
        float ratio = current / max;

        _bar.sizeDelta = new Vector2(initSize.x * ratio, initSize.y);
    }
}
