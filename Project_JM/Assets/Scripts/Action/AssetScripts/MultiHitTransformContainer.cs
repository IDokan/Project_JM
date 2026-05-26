// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 26/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: MultiHitTransformContainer.cs
// Summary: Per-hit Transform array for multi-hit attack logics; not used by any low-level system.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public struct MultiHitTransformContainer
{
    private Transform[] _transforms;

    public MultiHitTransformContainer(Transform[] transforms) => _transforms = transforms;

    public bool IsValid => _transforms != null && _transforms.Length > 0;

    public Transform GetTransform(int index)
    {
        if (!IsValid)
        {
            Debug.LogError("MultiHitTransformContainer: GetTransform called on an invalid container.");
            return null;
        }

        if (index < 0)
        {
            Debug.LogError($"MultiHitTransformContainer: negative index {index}. Using first element.");
            return _transforms[0];
        }

        if (index < _transforms.Length)
        {
            return _transforms[index];
        }

        Debug.LogError($"MultiHitTransformContainer: index {index} exceeds length {_transforms.Length}. Using last element.");
        return _transforms[_transforms.Length - 1];
    }
}
