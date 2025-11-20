// SPDX-License-Identifier: MIT
// Copyright (c) 11/19/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageUI.cs
// Summary: A script for damage UI.


using System;
using TMPro;
using DG.Tweening;
using UnityEngine;

public class DamageUI : MonoBehaviour
{
    [SerializeField] protected float _lifetime = 1.2f;

    protected TextMeshProUGUI _text;
    protected RectTransform _rect;

    protected void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _text = GetComponent<TextMeshProUGUI>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(int amount, Vector3 screenPos)
    {
        _rect.anchoredPosition = screenPos;

        if (amount <= 0)
        {
            _text.text = $"Blocked!";
        }
        else
        {
            _text.text = amount.ToString();
        }
        _text.alpha = 1f;

        // Randomize
        float randomX = GlobalRNG.Instance.NextFloat(-50f, 50f);
        float randomRot = GlobalRNG.Instance.NextFloat(-15f, 15f);
        _rect.rotation = Quaternion.Euler(0, 0, randomRot);

        // Animation
        Sequence seq = DOTween.Sequence();
        seq.Append(_rect.DOAnchorPos(_rect.anchoredPosition + new Vector2(randomX, 80f), _lifetime))
           .Join(_text.DOFade(0f, _lifetime))
           .Join(_rect.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack))
           .AppendInterval(0.2f)
           .OnComplete(() =>
           {
               Destroy(gameObject);
           });
    }
}
