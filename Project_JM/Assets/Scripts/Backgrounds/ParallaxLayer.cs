// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 02/23/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ParallaxLayer.cs
// Summary: A script to move object in parallax.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    public enum FollowMode
    {
        Manual,
        Parallax
    }

    [Range(0f, 2f)] public float parallaxFactor = 0.3f;     // 1 on camera; 0.1f slower than cam; 2 fater than cam

    [SerializeField] protected Transform cameraTransform;
    [SerializeField] protected FollowMode followMode = FollowMode.Parallax;


    protected Vector3 _startPos;
    protected float _startCamX;

    void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        CacheParallaxOrigin();
    }

    void LateUpdate()
    {
        if (followMode != FollowMode.Parallax)
        {
            return;
        }

        float camDeltaX = cameraTransform.position.x - _startCamX;

        transform.position = new Vector3(
            _startPos.x + camDeltaX * parallaxFactor,
            _startPos.y,
            _startPos.z
            );
    }

    public void SetManualMode()
    {
        followMode = FollowMode.Manual;
    }

    public void SetParallaxMode()
    {
        CacheParallaxOrigin();
        followMode = FollowMode.Parallax;
    }

    protected void CacheParallaxOrigin()
    {
        _startPos = transform.position;
        _startCamX = cameraTransform.position.x;
    }
}
