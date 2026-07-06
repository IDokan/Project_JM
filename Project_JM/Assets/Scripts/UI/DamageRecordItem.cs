// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 05/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: DamageRecordItem.cs
// Summary: A single damage record row: shows a gem-colored icon, a fill bar, and the damage dealt.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using DG.Tweening;
using GemEnums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageRecordItem : MonoBehaviour
{
    [SerializeField] protected Image iconImage;
    [SerializeField] protected Image sliderImage;
    [SerializeField] protected TextMeshProUGUI damageText;
    [SerializeField] protected float fillDuration = 0.6f;
    [SerializeField] protected float startDelay = 0.5f;

    [SerializeField] protected GemColorIconData gemColorIconData;

    protected const float MaxSliderWidth = 2.36f;
    protected const float MinSliderWidth = 0.37f;

    protected Tweener _sliderTween;
    protected Tweener _damageTween;

    public void SetData(GemColor color, int damage, int maxDamage)
    {
        Color tint = color.ConvertGemColor();

        iconImage.sprite = gemColorIconData.GetIconByColor(color);
        iconImage.SetNativeSize();
        sliderImage.color = tint;
        damageText.color = tint;

        AnimateDamageText(damage);
        AnimateSliderWidth(damage, maxDamage);
    }

    protected void OnDisable()
    {
        _sliderTween?.Kill();
        _damageTween?.Kill();
    }

    protected void AnimateDamageText(int damage)
    {
        damageText.text = "0";

        _damageTween?.Kill();
        int displayedDamage = 0;
        _damageTween = DOTween.To(() => displayedDamage, x => displayedDamage = x, damage, fillDuration)
            .SetDelay(startDelay)
            .SetEase(Ease.OutCubic)
            .OnUpdate(() => damageText.text = displayedDamage.ToString());
    }

    protected void AnimateSliderWidth(int damage, int maxDamage)
    {
        float ratio = maxDamage > 0 ? Mathf.Clamp01((float)damage / maxDamage) : 0f;
        float targetWidth = Mathf.Lerp(MinSliderWidth, MaxSliderWidth, ratio);

        RectTransform rectTransform = sliderImage.rectTransform;
        Vector2 sizeDelta = rectTransform.sizeDelta;
        sizeDelta.x = MinSliderWidth;
        rectTransform.sizeDelta = sizeDelta;

        _sliderTween?.Kill();
        _sliderTween = rectTransform.DOSizeDelta(new Vector2(targetWidth, sizeDelta.y), fillDuration)
            .SetDelay(startDelay)
            .SetEase(Ease.OutCubic);
    }
}
