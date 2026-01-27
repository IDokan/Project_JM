// SPDX-License-Identifier: MIT
// Copyright (c) 01/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: Rotate.cs
// Summary: A script to rotate an attached object.

using System.Collections;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    [SerializeField] protected Vector3 initialEuler = new Vector3(0f, 0f, 0f);
    [SerializeField] protected Vector3 targetEuler = new Vector3(0f, 0f, 180f);

    [SerializeField] protected float duration = 0.5f;

    [SerializeField] protected AnimationCurve ease = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    protected void OnEnable()
    {
        SetRotationEuler(initialEuler);

        StartCoroutine(RotateRoutine(initialEuler, targetEuler, duration));
    }

    protected IEnumerator RotateRoutine(Vector3 fromEuler, Vector3 toEuler, float seconds)
    {
        Quaternion from = Quaternion.Euler(fromEuler);
        Quaternion to = Quaternion.Euler(toEuler);

        float t = 0f;
        while (t < 1f)
        {
            t += GlobalTimeManager.DeltaTime / seconds;
            float eased = ease.Evaluate(Mathf.Clamp01(t));

            Quaternion q = Quaternion.SlerpUnclamped(from, to, eased);
            SetRotation(q);

            yield return null;
        }

        SetRotation(to);

    }

    protected void SetRotation(Quaternion q)
    {
        transform.localRotation = q;
    }

    protected void SetRotationEuler(Vector3 euler)
    {
        SetRotation(Quaternion.Euler(euler));
    }
}
