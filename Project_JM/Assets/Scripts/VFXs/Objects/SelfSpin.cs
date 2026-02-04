// SPDX-License-Identifier: MIT
// Copyright (c) 02/01/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: SelfSpin.cs
// Summary: A script to rotate itself.

using UnityEngine;

public class SelfSpin : MonoBehaviour
{
    [SerializeField] protected float degreePerSecond = 180;
    [SerializeField] protected bool initRandomRotation = true;

    protected float _angleZ = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (initRandomRotation)
        {
            _angleZ = Random.Range(0, 360);
        }
        else
        {
            _angleZ = transform.localEulerAngles.z;
        }
    }

    // Update is called once per frame
    void Update()
    {
        float newAngle = Mathf.Repeat(_angleZ + degreePerSecond * Time.deltaTime, 360f);

        transform.localRotation = Quaternion.Euler(0f, 0f, newAngle);

        _angleZ = newAngle;
    }
}
