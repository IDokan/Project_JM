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
    [Header("Actions")]
    [SerializeField] protected PlayerInput playerInput;

    [Header("Common Refs")]
    [SerializeField] protected ClickVFXSpawner clickVFXSpawner;
    [SerializeField] protected Camera cam;

    protected bool _isPadMode = false;
    public bool IsPadMode => _isPadMode;

    protected virtual void OnEnable()
    {
        playerInput.onActionTriggered += OnActionTriggered;
    }

    protected virtual void OnDisable()
    {
        playerInput.onActionTriggered -= OnActionTriggered;
    }

    private void OnActionTriggered(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "press":
                if (context.phase == InputActionPhase.Started) OnPressStarted(context);
                else if (context.phase == InputActionPhase.Canceled) OnPressCanceled(context);
                break;
            case "move":
                if (context.phase == InputActionPhase.Started) OnMoveStarted(context);
                else if (context.phase == InputActionPhase.Performed) OnMovePerformed(context);
                else if (context.phase == InputActionPhase.Canceled) OnMoveCanceled(context);
                break;
            case "confirm":
                if (context.phase == InputActionPhase.Started) OnConfirmStarted(context);
                else if (context.phase == InputActionPhase.Canceled) OnConfirmCanceled(context);
                break;
            case "cancel":
                if (context.phase == InputActionPhase.Performed) OnCancelPerformed(context);
                break;
        }
    }

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

    public Vector2 GetCurrentFollowPoint() => playerInput.actions["point"].ReadValue<Vector2>();

    protected Vector2Int Decide4Way(Vector2 v)
    {
        return Mathf.Abs(v.x) > Mathf.Abs(v.y)
            ? new Vector2Int(v.x > 0 ? 1 : -1, 0)
            : new Vector2Int(0, v.y > 0 ? 1 : -1);
    }
}
