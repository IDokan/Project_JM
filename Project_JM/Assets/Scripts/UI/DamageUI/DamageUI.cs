// SPDX-License-Identifier: MIT
// Copyright (c) 11/19/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageUI.cs
// Summary: A script for damage UI.


using TMPro;
using DG.Tweening;
using UnityEngine;
using GemEnums;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    [SerializeField] protected float _lifetime = 1.2f;
    [SerializeField] protected TextMeshProUGUI text;
    [SerializeField] protected GameObject shieldObject;

    protected Image shieldImage = null;
    protected RectTransform _rect;


    protected void Awake()
    {
        _rect = GetComponent<RectTransform>();
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
    }

    protected void OnEnable()
    {
        shieldObject.SetActive(false);
        if (shieldImage != null)
        {
            Color shieldImageColor = shieldImage.color;
            shieldImageColor.a = 1f;
            shieldImage.color = shieldImageColor;
            shieldImage = null;
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

    public void Show(int amount, AttackContext context, bool isCritical, float sizeMultiplier)
    {

        Vector3 screenPos = Camera.main.WorldToScreenPoint(context.HitTransform.position) - transform.position;

        _rect.anchoredPosition = screenPos;

        if (amount <= 0)
        {
            shieldObject.SetActive(true);
            shieldImage = shieldObject.GetComponent<Image>();
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
        Sequence seq = DOTween.Sequence();
        seq.Append(_rect.DOAnchorPos(_rect.anchoredPosition + new Vector2(randomX, 80f), _lifetime))
           .Join(text.DOFade(0f, _lifetime).SetEase(Ease.InCubic));


        if (shieldImage != null)
        {
            seq.Join(shieldImage.DOFade(0f, _lifetime).SetEase(Ease.InCubic));
        }

        seq.Join(
            _rect.DOScale(1.2f, _lifetime / 4f).SetEase(Ease.OutBack)
            .OnComplete(() => _rect.DOScale(0.8f, _lifetime * 3f / 4f))
            )
           .AppendInterval(0.2f)
           .OnComplete(() =>
           {
               Destroy(gameObject);
           });
    }
}
