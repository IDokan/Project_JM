// SPDX-License-Identifier: MIT
// Copyright (c) 02/23/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParallaxLooper.cs
// Summary: A script to loop to look sprite infinite.

using UnityEngine;

public class ParallaxLooper : MonoBehaviour
{
    [SerializeField] protected Transform cameraTransform;
    [SerializeField] protected SpriteRenderer tileRenderer; // One of the tiles to get width
    [SerializeField] protected Transform[] tiles;

    protected float _tileWidth;

    protected void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        _tileWidth = tileRenderer.bounds.size.x;

        for (int i = 1; i < tiles.Length; i++)
        {
            tiles[i].position = new Vector3(
                tiles[i - 1].position.x + _tileWidth,
                tiles[i - 1].position.y,
                tiles[i - 1].position.z
                );
        }

    }

    protected void LateUpdate()
    {
        Transform leftMost = tiles[0];
        Transform rightMost = tiles[0];

        for (int i = 1; i < tiles.Length; i++)
        {
            if (tiles[i].position.x < leftMost.position.x)
            {
                leftMost = tiles[i];
            }
            if (tiles[i].position.x > rightMost.position.x)
            {
                rightMost = tiles[i];
            }
        }

        // If the left most tile is outside from the camera screen, move it to opposite end
        float camX = cameraTransform.position.x;

        // "half width" threshold feels stable and avoids premature swaps
        if (camX - leftMost.position.x > _tileWidth)
        {
            leftMost.position = new Vector3(
                rightMost.position.x + _tileWidth,
                leftMost.position.y,
                leftMost.position.z
                );
        }
        if (rightMost.position.x - camX > _tileWidth)
        {
            rightMost.position = new Vector3(
                leftMost.position.x - _tileWidth,
                rightMost.position.y,
                rightMost.position.z
                );
        }
    }
}
