// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CameraOrientationSetter.cs
// Summary: Applies the correct orthographic size for the current screen orientation at startup, so portrait reveals the intended vertical view instead of just cropping the landscape framing.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class CameraOrientationSetter : MonoBehaviour
{
    [SerializeField] protected Camera targetCamera;
    [SerializeField] protected float landscapeOrthographicSize = 5f;
    [SerializeField] protected float portraitOrthographicSize = 5f;
    [SerializeField] protected Vector3 landscapeCameraPosition;
    [SerializeField] protected Vector3 portraitCameraPosition;

    protected static bool IsPortrait => Screen.height > Screen.width;

    public Vector3 OriginalPosition => IsPortrait ? portraitCameraPosition : landscapeCameraPosition;

    private void Awake()
    {
        if (targetCamera == null)
        {
            return;
        }

        targetCamera.orthographicSize = IsPortrait ? portraitOrthographicSize : landscapeOrthographicSize;
        targetCamera.transform.position = OriginalPosition;
    }
}
