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

    [Range(0f, 2f)] [SerializeField] protected float parallaxFactor = 0.3f;     // 1 on camera; 0.1f slower than cam; 2 fater than cam

    public float ParallaxFactor => parallaxFactor;

    [SerializeField] protected Transform cameraTransform;
    [SerializeField] protected CameraShake cameraShake;
    [SerializeField] protected FollowMode followMode = FollowMode.Parallax;
    [SerializeField] protected bool followShake = false;


    protected Vector3 _startPos;
    protected float _startCamX;
    protected float _startCamY;

    void Awake()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraShake == null)
        {
            cameraShake = Camera.main.GetComponent<CameraShake>();
        }

        CameraOrientationSetter cameraOrientationSetter = cameraTransform.GetComponent<CameraOrientationSetter>();
        Vector3 initialCameraPosition = cameraOrientationSetter != null ? cameraOrientationSetter.OriginalPosition : cameraTransform.position;
        CacheParallaxOrigin(initialCameraPosition);
    }

    void LateUpdate()
    {
        if (followMode != FollowMode.Parallax)
        {
            return;
        }

        float camDeltaX;
        float camDeltaY;
        if (followShake)
        {
            camDeltaX = cameraTransform.position.x - _startCamX;
            camDeltaY = cameraTransform.position.y - _startCamY;
        }
        else
        {
            Vector3 shakeOffset = cameraShake != null ? cameraShake.ShakeOffset : Vector3.zero;
            camDeltaX = (cameraTransform.position.x - shakeOffset.x) - _startCamX;
            camDeltaY = (cameraTransform.position.y - shakeOffset.y) - _startCamY;
        }

        transform.position = new Vector3(
            _startPos.x + camDeltaX * parallaxFactor,
            _startPos.y + camDeltaY * parallaxFactor,
            _startPos.z
            );
    }

    public void SetManualMode()
    {
        followMode = FollowMode.Manual;
    }

    public void SetParallaxMode()
    {
        CacheParallaxOrigin(cameraTransform.position);
        followMode = FollowMode.Parallax;
    }

    protected void CacheParallaxOrigin(Vector3 cameraPosition)
    {
        _startPos = transform.position;
        _startCamX = cameraPosition.x;
        _startCamY = cameraPosition.y;
    }
}
