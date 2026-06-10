// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 10/06/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ShadowFollower.cs
// Summary: Keeps the shadow sprite at a fixed world-Y while mirroring the target's world-X.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class ShadowFollower : MonoBehaviour
{
    [SerializeField] private Transform target;

    private float _xOffset;
    private float _yOffset;
    private float _initialY;

    private void Start()
    {
        _xOffset = transform.position.x - target.position.x;
        _yOffset = transform.position.y - target.position.y;
        _initialY = transform.position.y;
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.x = target.position.x + _xOffset;
        pos.y = Mathf.Min(_initialY, target.position.y + _yOffset);
        transform.position = pos;
    }
}
