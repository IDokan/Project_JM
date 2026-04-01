// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 01/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BasePlayerController.cs
// Summary: Abstract base for all player controllers; owns input action wiring and click VFX spawning.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.InputSystem;

public abstract class BasePlayerController : MonoBehaviour
{
    [Header("Actions (drag from Controls.input actions)")]
    public InputActionReference point;
    public InputActionReference press;
    public InputActionReference move;
    public InputActionReference confirm;
    public InputActionReference cancel;

    [Header("Common Refs")]
    [SerializeField] protected ClickVFXSpawner clickVFXSpawner;
    [SerializeField] protected Camera cam;

    protected bool _isPadMode = false;
    public bool IsPadMode => _isPadMode;

    protected void OnEnable()
    {
        point.action.Enable();
        press.action.Enable();
        move.action.Enable();
        confirm.action.Enable();
        cancel.action.Enable();

        press.action.started += OnPressStarted;
        press.action.canceled += OnPressCanceled;
        confirm.action.started += OnConfirmStarted;
        confirm.action.canceled += OnConfirmCanceled;
        move.action.started += OnMoveStarted;
        move.action.performed += OnMovePerformed;
        move.action.canceled += OnMoveCanceled;
        cancel.action.performed += OnCancelPerformed;
    }

    protected void OnDisable()
    {
        press.action.started -= OnPressStarted;
        press.action.canceled -= OnPressCanceled;
        confirm.action.started -= OnConfirmStarted;
        confirm.action.canceled -= OnConfirmCanceled;
        move.action.started -= OnMoveStarted;
        move.action.performed -= OnMovePerformed;
        move.action.canceled -= OnMoveCanceled;
        cancel.action.performed -= OnCancelPerformed;

        point.action.Disable();
        press.action.Disable();
        move.action.Disable();
        confirm.action.Disable();
        cancel.action.Disable();
    }

    // Base: switch to mouse mode and spawn plain click VFX.
    // CombatController overrides this entirely for gem-color-aware spawning.
    protected virtual void OnPressStarted(InputAction.CallbackContext _)
    {
        _isPadMode = false;
        clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint());
    }

    protected virtual void OnPressCanceled(InputAction.CallbackContext _) { }

    protected virtual void OnMoveStarted(InputAction.CallbackContext _) { }

    protected virtual void OnMoveCanceled(InputAction.CallbackContext _) { }

    protected virtual void OnMovePerformed(InputAction.CallbackContext context)
    {
        _isPadMode = true;
    }

    protected virtual void OnConfirmStarted(InputAction.CallbackContext _)
    {
        _isPadMode = true;
    }

    protected virtual void OnConfirmCanceled(InputAction.CallbackContext _) { }

    protected virtual void OnCancelPerformed(InputAction.CallbackContext _) { }

    public Vector2 GetCurrentFollowPoint() => point.action.ReadValue<Vector2>();

    protected Vector2Int Decide4Way(Vector2 v)
    {
        return Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2Int(v.x > 0 ? 1 : -1, 0)
            : new Vector2Int(0, v.y > 0 ? 1 : -1);
    }
}
