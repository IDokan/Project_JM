// SPDX-License-Identifier: MIT
// Copyright (c) 11/03/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Gem.cs
// Summary: A script for gem.

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

    [SerializeField] GameObject gemResolver;

    public void Init(GemColor gemColor)
    {
        Color = gemColor;

        if (Color == GemColor.None)
        {
            GetComponent<SpriteRenderer>().color = GemColorUtility.ConvertGemColor(gemColor);
        }
        else
        {
            GetComponent<SpriteRenderer>().sprite = GetSpriteByColor(gemColor);
        }
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



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected Sprite GetSpriteByColor(GemColor color)
    {
        return color switch
        {
            GemColor.Red => redSprite,
            GemColor.Blue => blueSprite,
            GemColor.Green => greenSprite,
            GemColor.Yellow => yellowSprite,
            _ => null
        };
    }
}
