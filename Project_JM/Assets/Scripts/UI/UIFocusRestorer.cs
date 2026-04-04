// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 04/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: UIFocusRestorer.cs
// Summary: Restores EventSystem selection to the menu's first selected object when the user clicks on empty space.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIFocusRestorer : MonoBehaviour, IPointerClickHandler
{
    private Menu _menu;

    private void Awake()
    {
        _menu = GetComponentInParent<Menu>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_menu == null)
        {
            return;
        }

        Selectable firstSelected = _menu.GetFirstSelectable();
        if (firstSelected != null && firstSelected.gameObject.activeInHierarchy)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
    }
}
