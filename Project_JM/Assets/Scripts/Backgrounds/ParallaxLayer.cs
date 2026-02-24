// SPDX-License-Identifier: MIT
// Copyright (c) 02/23/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParallaxLayer.cs
// Summary: A script to move object in parallax.

using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 10f)] public float parallaxFactor = 0.3f; // Stuck to world, 1 = follows camera, >1 = faster

    [SerializeField] protected Transform cameraTransform;

    protected Vector3 _startPos;
    protected float _startCamX;

    void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        _startPos = transform.position;
        _startCamX = cameraTransform.position.x;
    }

    void LateUpdate()
    {
        float camDeltaX = cameraTransform.position.x - _startCamX;

        transform.position = new Vector3(
            _startPos.x + camDeltaX * parallaxFactor,
            _startPos.y,
            _startPos.z
            );
    }
}
