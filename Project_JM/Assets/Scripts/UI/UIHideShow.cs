// SPDX-License-Identifier: MIT
// Copyright (c) 03/09/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM 
// File: UIHideShow.cs
// Summary: A script to hide and show serialized objects.

using UnityEngine;

public class UIHideShow : MonoBehaviour
{
    [SerializeField] protected GameObject[] objectsToControl;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HideObjects();
    }

    public void ShowObjects()
    {
        foreach (GameObject go in objectsToControl)
        {
            if (go != null)
            {
                go.SetActive(true);
            }
        }
    }

    public void HideObjects()
    {
        foreach (GameObject go in objectsToControl)
        {
            if (go != null)
            {
                go.SetActive(false);
            }
        }
    }
}
