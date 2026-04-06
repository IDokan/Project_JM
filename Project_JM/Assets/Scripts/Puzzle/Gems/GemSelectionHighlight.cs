// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 03/20/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: GemSelectionHighlight.cs
// Summary: A script to highlight which gem or cell is selected.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;

public class GemSelectionHighlight : MonoBehaviour
{
    [SerializeField] protected GameObject arrowParent;
    [SerializeField] protected GameObject topArrow;
    [SerializeField] protected GameObject leftArrow;
    [SerializeField] protected GameObject bottomArrow;
    [SerializeField] protected GameObject rightArrow;

    [SerializeField] protected Animator arrowAnimator;



    protected void Start()
    {

    }

    public void EnableArrows(bool enable)
    {
        arrowParent.SetActive(enable);
    }

    public void EnableArrows(bool top, bool left, bool bottom, bool right)
    {
        arrowParent.SetActive(true);
        topArrow.SetActive(top);
        leftArrow.SetActive(left);
        bottomArrow.SetActive(bottom);
        rightArrow.SetActive(right);

        arrowAnimator.Update(0f);
    }
}
