// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/19/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageUI.cs
// Summary: A script for damage UI.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.


using TMPro;
using DG.Tweening;
using UnityEngine;
using GemEnums;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    [SerializeField] protected float lifetime = 1.2f;
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] protected GameObject shieldObject;

    protected Image _shieldImage = null;
    protected RectTransform _rect;
    protected Canvas _canvas;
    protected RectTransform _canvasRect;


    protected void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();
    }

    protected void OnEnable()
    {
        shieldObject.SetActive(false);
        if (_shieldImage != null)
        {
            Color shieldImageColor = _shieldImage.color;
            shieldImageColor.a = 1f;
            _shieldImage.color = shieldImageColor;
            _shieldImage = null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Show(int amount, AttackContext context, bool isCritical, bool shieldReduced, float sizeMultiplier)
    {

        Vector3 screenPos = Camera.main.WorldToScreenPoint(context.HitTransform.position);

        // Transform screen space position to the current rectangle space position
            // because canvas is Scree Space - Camera.
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            screenPos,
            _canvas.worldCamera,
            out Vector2 localPoint
            );

        _rect.anchoredPosition = localPoint;

        if (shieldReduced)
        {
            shieldObject.SetActive(true);
            _shieldImage = shieldObject.GetComponent<Image>();
        }

        text.text = amount.ToString();


        if (isCritical)
        {
            text.text += "<size=65%>!!</size>";
        }


        text.alpha = 1f;
        if (context.Attacker is Component c)
        {
            if (c.TryGetComponent<EnemyTag>(out _))
            {
                text.color = GemColorUtility.ConvertGemColor(GemColor.None);
            }
            else
            {
                text.color = GemColorUtility.ConvertGemColor(context.Attacker.Colors[0]);
            }
        }

        text.fontSize *= isCritical ? 2f * sizeMultiplier : sizeMultiplier;

        // Randomize
        float randomX = Random.Range(-50f, 50f);
        float randomRot = Random.Range(-15f, 15f);
        _rect.rotation = Quaternion.Euler(0, 0, randomRot);

        // Animation
        _rect.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.Append(_rect.DOAnchorPos(_rect.anchoredPosition + new Vector2(randomX, 80f), lifetime))
           .Join(text.DOFade(0f, lifetime).SetEase(Ease.InCubic));


        if (_shieldImage != null)
        {
            seq.Join(_shieldImage.DOFade(0f, lifetime).SetEase(Ease.InCubic));
        }

        seq.Join(_rect.DOScale(1.2f, lifetime / 4f).SetEase(Ease.OutBack))
           .Insert(lifetime / 4f, _rect.DOScale(0.8f, lifetime * 3f / 4f))
           .AppendInterval(0.2f)
           .OnComplete(() =>
           {
               Destroy(gameObject);
           });
    }
}
