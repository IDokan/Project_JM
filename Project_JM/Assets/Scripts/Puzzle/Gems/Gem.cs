// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/03/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Gem.cs
// Summary: A script for gem.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System;
using UnityEngine;
using GemEnums;

public class Gem : MonoBehaviour
{
    public GemColor Color { get; private set; }

    [Header("Sprite References")]
    public Sprite redSprite;
    public Sprite greenSprite;
    public Sprite blueSprite;
    public Sprite yellowSprite;
    public Sprite noneSprite;

    [SerializeField] GameObject gemResolver;

    [SerializeField] SpriteRenderer spriteRenderer;

    public void Init(GemColor gemColor)
    {
        Color = gemColor;
        spriteRenderer.sprite = GetSpriteByColor(gemColor);
    }

    public void Resolve(PartyRoster partyRoster, Action<GemColor> onAbsorbed)
    {
        GemResolver resolver = Instantiate(gemResolver).GetComponent<GemResolver>();

        resolver.transform.SetPositionAndRotation(transform.position, transform.rotation);
        resolver.transform.localScale = transform.lossyScale;


        Transform target = partyRoster.GetCharacterTransform(Color);
        resolver.Init(Color, target, onAbsorbed);

        Destroy(gameObject);
    }

    public void ResolveNoTarget()
    {
        GemResolver resolver = Instantiate(gemResolver).GetComponent<GemResolver>();

        resolver.transform.SetPositionAndRotation(transform.position, transform.rotation);
        resolver.transform.localScale = transform.lossyScale;

        resolver.InitNoTarget(Color);

        Destroy(gameObject);
    }



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    protected Sprite GetSpriteByColor(GemColor color)
    {
        return color switch
        {
            GemColor.Red => redSprite,
            GemColor.Blue => blueSprite,
            GemColor.Green => greenSprite,
            GemColor.Yellow => yellowSprite,
            GemColor.None => noneSprite,
            _ => null
        };
    }
}
