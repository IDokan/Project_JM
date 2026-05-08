// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 07/05/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PauseButton.cs
// Summary: Attaches to the pause Button UI; opens PauseMenu and plays a sound on click or direct call.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour
{
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private AudioCueSO pressSFX;

    private Button _button;

    public bool IsPaused => pauseMenu.IsPaused;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable() => _button.onClick.AddListener(Open);
    private void OnDisable() => _button.onClick.RemoveListener(Open);

    public void Open()
    {
        AudioManager.Instance.PlayUISFX(pressSFX);
        pauseMenu.Show();
    }
}
