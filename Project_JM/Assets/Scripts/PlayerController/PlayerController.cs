// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/05/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PlayerController.cs
// Summary: A script for universal controller.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] protected BoardManager board;
    [SerializeField] protected GemSelectionHighlightManager gemSelectionHighlightManager;
    [SerializeField] protected TransitionManager transitionManager;
    [SerializeField] protected Camera cam;

    [Header("Actions (drag from Controls.input actions)")]
    public InputActionReference point;
    public InputActionReference press;
    public InputActionReference move;
    public InputActionReference confirm;
    public InputActionReference cancel;

    [Header("Tuning")]
    [SerializeField] protected float dragTresholdPixels = 16f;
    [SerializeField] protected float moveRepeatRate = 0.12f;
    [SerializeField] protected float stickDeadZone = 0.25f;
    [SerializeField] protected float swapRepeatRate = 0.4f;
    [SerializeField] protected float holdingRepeatDelay = 0.8f;

    [Header("Interaction VFX")]
    [SerializeField] protected ClickVFXSpawner clickVFXSpawner;

    [Header("Pause UI")]
    [SerializeField] protected PauseManager pauseManager;

    // Pointer-drag state
    protected Vector2 _pressScreenPos;
    protected bool _firedThisDrag;      // A boolean flag that make sure swap happens only once per one drag.

    // Gamepad/keyboard selection
    protected bool _hasSelection;
    protected float _nextMoveHoldTime;
    protected Vector2Int _lastMovedDirection;
    protected float _nextSwapTime;

    protected int _selRow = INVALID, _selCol = INVALID;

    private const int INVALID = -1;


    protected bool _isConfirmPressing;
    protected bool _isMoveHolding;

    protected bool _isPadMode = false;
    public bool IsPadMode => _isPadMode;

    protected void OnEnable()
    {
        // Make sure actions are enabled (PlayerInput usually does this, but it's safe)
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

    // Update is called once per frame
    void Update()
    {
        if (!IsBoardInputEnabled())
        {
            return;
        }

        if (_isPadMode)
        {
            if (_lastMovedDirection == Vector2Int.zero)
            {
                return;
            }

            // Keyboard & Gamepad mode
            HoldingMoveAction();
        }
        else
        {
            // Mouse & Mobile mode
            // Pointer drag : once threshold exceeded, decide a 4-way dir and trigger one swap
            if (board.InBounds(_selRow, _selCol) && !_firedThisDrag && press.action.IsPressed())
            {
                Vector2 delta = GetCurrentFollowPoint() - _pressScreenPos;
                if (delta.magnitude >= dragTresholdPixels)
                {
                    Vector2Int dir = Decide4Way(delta);
                    _firedThisDrag = true;
                    board.TrySwapFrom(new Vector2Int(_selRow, _selCol), dir);
                }
            }
        }
    }

    // ------- Pointer events --------
    protected void OnPressStarted(InputAction.CallbackContext _)
    {
        _isPadMode = false;

        transitionManager.BeginSkipHold();

        if (!IsBoardInputEnabled())
        {
            clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint());
            return;
        }


        _pressScreenPos = GetCurrentFollowPoint();
        var index = GemIndexUnderCursor(_pressScreenPos);
        if (!board.InBounds(index.x, index.y))
        {
            clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint());
            return;
        }
        _firedThisDrag = false;

        SetSelection(index.x, index.y);

        clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint(), board.GetGemColor(index));
    }

    protected void OnPressCanceled(InputAction.CallbackContext _)
    {
        transitionManager.EndSkipHold();

        ClearSelection();
        _firedThisDrag = false;
    }

    // Stick started latching
    protected void OnMoveStarted(InputAction.CallbackContext _)
    {
        _isMoveHolding = true;
    }

    // Stick placed idle position.
    protected void OnMoveCanceled(InputAction.CallbackContext _)
    {
        _isMoveHolding = false;

        _lastMovedDirection = Vector2Int.zero;
    }

    // At everytime stick value has changed,
    // ------- Gamepad \ Keyboard ---------
    protected void OnMovePerformed(InputAction.CallbackContext context)
    {
        _isPadMode = true;

        Vector2 direction = context.ReadValue<Vector2>();

        if (direction.sqrMagnitude < stickDeadZone * stickDeadZone)
        {
            _lastMovedDirection = Vector2Int.zero;

            return;
        }

        // Since this function invoked everytime stick updated, skip all actions that quried by the same direction.
        Vector2Int directionInt = Decide4Way(direction);

        if (directionInt == _lastMovedDirection)
        {
            // Skip movements to the same direction
            return;
        }

        bool isPerformed = false;

        if (_isConfirmPressing && IsBoardInputEnabled())
        {
            isPerformed = SwapGem(directionInt);
        }
        else if (!pauseManager.IsPaused)        // Enabled gem selection action when board disabled.
        {
            // It is very dangerous because below lines executed even board _gems is null.
            isPerformed = SelectGem(directionInt);
        }

        if (isPerformed)
        {
            _lastMovedDirection = directionInt;
            _nextMoveHoldTime = Time.time + holdingRepeatDelay;
        }
    }

    protected void OnConfirmStarted(InputAction.CallbackContext _)
    {
        transitionManager.BeginSkipHold();

        _isPadMode = true;

        if (IsBoardInputEnabled())
        {
            _isConfirmPressing = true;
            gemSelectionHighlightManager.EnableArrows(_selRow, _selCol);
        }
    }

    protected void OnConfirmCanceled(InputAction.CallbackContext _)
    {
        transitionManager.EndSkipHold();


        _isConfirmPressing = false;
        gemSelectionHighlightManager.DisableArrows();
    }

    protected bool IsBoardInputEnabled() => board.InputEnabled && !pauseManager.IsPaused;

    // ----- Helpers ---------
    protected Vector2Int Decide4Way(Vector2 v)
    {
        // It's result may different depends on uprrow is origin is top left.
        return Mathf.Abs(v.x) > Mathf.Abs(v.y) ?
            new Vector2Int(v.x > 0 ? 1 : -1, 0) :
            new Vector2Int(0, v.y > 0 ? 1 : -1);
    }

    protected Vector2Int GemIndexUnderCursor(Vector2 screenPos)
    {
        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        Vector2 local = board.transform.InverseTransformPoint(world);

        return board.GetGemIndex(local);
    }

    protected void SetSelection(int r, int c)
    {
        _hasSelection = true;
        _selRow = r;
        _selCol = c;

        gemSelectionHighlightManager.HighlightCell(_selRow, _selCol);
    }

    protected void ClearSelection()
    {
        _hasSelection = false;
        _selRow = INVALID;
        _selCol = INVALID;

        gemSelectionHighlightManager.HighlightCell(_selRow, _selCol);
    }

    public Vector2 GetCurrentFollowPoint()
    {
        return point.action.ReadValue<Vector2>();
    }

    protected bool SelectGem(Vector2Int direction)
    {


        if (!_hasSelection)
        {   // Init, nothing has selected.

            // start centered
            int r = Mathf.Clamp(board.Rows / 2, 0, board.Rows - 1);
            int c = Mathf.Clamp(board.Cols / 2, 0, board.Cols - 1);
            SetSelection(r, c);

            return true;
        }
        else
        {
            // Typical case, move cursor or try swapping gems
            int nr = _selRow + direction.y;
            int nc = _selCol + direction.x;
            if (board.InBounds(nr, nc))
            {
                // Move cursor
                SetSelection(nr, nc);

                return true;
            }
            else
            {
                // @@ TODO: Add feedback this is invalid and impossible.
            }
        }

        return false;
    }

    protected bool SwapGem(Vector2Int direction)
    {
        // Throttle repeats
        if (!_hasSelection || Time.time < _nextSwapTime)
        {
            // Checks _hasSelection because response immediately for the first selection interaction
            Debug.LogWarning("Tried swapping not selected. OR Skipped by response time not satisfied.", this);
            return false;
        }

        _nextSwapTime = Time.time + swapRepeatRate;

        // Try swapping gems
        board.TrySwapFrom(new Vector2Int(_selRow, _selCol), direction);

        return true;
    }

    protected void HoldingMoveAction()
    {
        if (Time.time < _nextMoveHoldTime)
        {
            return;
        }

        if (_isConfirmPressing)
        {
            board.TrySwapFrom(new Vector2Int(_selRow, _selCol), _lastMovedDirection);

            _nextMoveHoldTime = Time.time + swapRepeatRate;

            return;
        }

        if (_isMoveHolding)
        {
            int nr = _selRow + _lastMovedDirection.y;
            int nc = _selCol + _lastMovedDirection.x;
            if (board.InBounds(nr, nc))
            {
                // Move cursor
                SetSelection(nr, nc);
                _nextMoveHoldTime = Time.time + moveRepeatRate;
            }
        }

        return;



    }

    protected void OnCancelPerformed(InputAction.CallbackContext _)
    {
        // Open pause menu
        pauseManager.Pause();
    }
}