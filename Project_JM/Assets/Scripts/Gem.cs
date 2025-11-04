using System;
using UnityEngine;
using GemEnums;

public class Gem : MonoBehaviour
{
    public GemColor color { get; set; }

    public void Init(GemColor gemColor)
    {
        color = gemColor;
        GetComponent<SpriteRenderer>().color = GemColorUtility.ConvertGemColor(gemColor);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
