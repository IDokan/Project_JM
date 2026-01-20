// SPDX-License-Identifier: MIT
// Copyright (c) 01/19/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: HitBurst.cs
// Summary: A script for main mechanism of hit burst.

using UnityEngine;

public class HitBurst : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetColor(Color color)
    {
        spriteRenderer.color = color;
    }
}
