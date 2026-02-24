// SPDX-License-Identifier: MIT
// Copyright (c) 02/23/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyActivation.cs
// Summary: A script to enable and disable scripts.

using UnityEngine;

public class EnemyActivation : MonoBehaviour
{
    [Header("Disable theses while sleeping")]
    [SerializeField] protected MonoBehaviour[] scriptsToHandle;
    [SerializeField] protected Collider2D[] collidersToHandle;

    [SerializeField] protected bool enableOnAwake = true;

    protected void Awake()
    {
        ActivateScripts(enableOnAwake);
    }

    public void EnableScripts()
    {
        ActivateScripts(true);
    }

    public void DisableScripts()
    {
        ActivateScripts(false);
    }

    protected void ActivateScripts(bool enabled)
    {
        foreach (var s in scriptsToHandle)
        {
            if (s)
            {
                s.enabled = enabled;
            }
        }

        foreach (var c in collidersToHandle)
        {
            if (c)
            {
                c.enabled = enabled;
            }
        }
    }
}
