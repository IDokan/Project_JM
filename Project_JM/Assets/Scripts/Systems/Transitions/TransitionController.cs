// SPDX-License-Identifier: MIT
// Copyright (c) 03/11/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: TransitionController.cs
// Summary: An abstract script for all transition controller classes.

using System;
using UnityEngine;

public abstract class TransitionController : MonoBehaviour
{
    public event Action<TransitionController> Started;
    public event Action<TransitionController> Completed;

    protected void RaiseStarted()
    {
        Started?.Invoke(this);
    }

    protected void RaiseCompleted()
    {
        Completed?.Invoke(this);
    }
}
