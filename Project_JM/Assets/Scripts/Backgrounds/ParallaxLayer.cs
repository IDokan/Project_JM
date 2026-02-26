// SPDX-License-Identifier: MIT
// Copyright (c) 02/23/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParallaxLayer.cs
// Summary: A script to move object in parallax.

using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Range(0f, 2f)] public float parallaxFactor = 0.3f;     // 1 on camera; 0.1f slower than cam; 2 fater than cam

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
