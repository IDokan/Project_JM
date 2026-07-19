// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/07/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: InputDiagnosticLogger.cs
// Summary: Dumps PlayerInput action-map state, pause/tutorial-freeze flags, and touch device state to the log; used to diagnose the mobile input-dead-zone bug when it recurs.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputDiagnosticLogger : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PauseButton pauseButton;
    [SerializeField] private TutorialOverlayUI tutorialOverlayUI;
    [SerializeField] private BoardManager boardManager;

    private void OnApplicationFocus(bool hasFocus)
    {
        LogSnapshot(hasFocus ? "ApplicationFocus:Gained" : "ApplicationFocus:Lost");
    }

    public void LogSnapshot(string reason)
    {
        InputActionMap gameplayMap = playerInput.actions.FindActionMap("Gameplay");
        InputActionMap uiMap = playerInput.actions.FindActionMap("UI");

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"[InputDiagnostic] reason={reason} realtime={Time.realtimeSinceStartup:F2}");
        sb.AppendLine($"  currentActionMap={playerInput.currentActionMap?.name ?? "null"}");
        sb.AppendLine($"  currentControlScheme={playerInput.currentControlScheme ?? "null"}");
        sb.AppendLine($"  neverAutoSwitchControlSchemes={playerInput.neverAutoSwitchControlSchemes}");
        sb.AppendLine($"  GameplayMap.enabled={gameplayMap != null && gameplayMap.enabled}");
        sb.AppendLine($"  UIMap.enabled={uiMap != null && uiMap.enabled}");
        sb.AppendLine($"  pauseButton.IsPaused={pauseButton.IsPaused}");
        sb.AppendLine($"  Time.timeScale={Time.timeScale}");
        sb.AppendLine($"  GlobalTimeManager.IsPaused={GlobalTimeManager.Instance.IsPaused}");
        sb.AppendLine($"  GlobalTimeManager.IsTutorialFrozen={GlobalTimeManager.Instance.IsTutorialFrozen}");
        sb.AppendLine($"  tutorialOverlayUI.IsWaitingForConfirm={tutorialOverlayUI.IsWaitingForConfirm}");
        sb.AppendLine($"  tutorialOverlayUI.IsSequenceActive={tutorialOverlayUI.IsSequenceActive}");
        sb.AppendLine($"  boardManager.InputEnabled={boardManager.InputEnabled}");
        sb.AppendLine($"  Touchscreen.current={DescribeDevice(Touchscreen.current)}");

        StringBuilder devices = new StringBuilder();
        foreach (InputDevice device in InputSystem.devices)
        {
            devices.Append(DescribeDevice(device));
            devices.Append(' ');
        }
        sb.AppendLine($"  devices={devices}");

        Debug.Log(sb.ToString(), this);
    }

    private string DescribeDevice(InputDevice device)
    {
        if (device == null)
        {
            return "null";
        }

        return $"[{device.deviceId}:{device.GetType().Name}:{device.displayName}:enabled={device.enabled}]";
    }
}
