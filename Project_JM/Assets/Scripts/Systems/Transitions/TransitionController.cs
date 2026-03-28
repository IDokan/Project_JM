// SPDX-License-Identifier: MIT
// Copyright (c) 03/11/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TransitionController.cs
// Summary: An abstract script for all transition controller classes.

using System;
using UnityEngine;

public abstract class TransitionController : MonoBehaviour
{
    [SerializeField] protected TransitionManager transitionManager;

    protected virtual void Awake()
    {
        if (transitionManager == null)
        {
            transitionManager = GetComponentInParent<TransitionManager>();
        }
    }

    public bool RequestTransitionStart(Action startAction)
    {
        if (startAction == null)
        {
            return false;
        }

        if (transitionManager == null)
        {
            startAction();
            return true;
        }

        return transitionManager.TryStartTransition(this, startAction);
    }

    public void CompleteTransition()
    {
        if (transitionManager != null)
        {
            transitionManager.CompleteTransition(this);
        }
    }
}
