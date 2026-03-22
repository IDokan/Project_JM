// SPDX-License-Identifier: MIT
// Copyright (c) 11/05/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: PlayerController.cs
// Summary: A script for universal controller.

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] protected BoardManager _board;
    [SerializeField] protected GemSelectionHighlight gemSelectionHighlight;
    [SerializeField] protected TransitionManager transitionManager;
    [SerializeField] protected Camera _cam;

    [Header("Actions (drag from Controls.input actions)")]
    public InputActionReference point;
    public InputActionReference press;
    public InputActionReference move;
    public InputActionReference confirm;

    [Header("Tuning")]
    [SerializeField] protected float _dragTresholdPixels = 16f;
    [SerializeField] protected float _moveRepeatDelay = 0.25f;
    [SerializeField] protected float _moveRepeatRate = 0.12f;
    [SerializeField] protected float _stickDeadZone = 0.25f;
    [SerializeField] protected float _swapRepeatRate = 0.4f;
    [SerializeField] protected float _holdingRepeatDelay = 0.8f;

    [Header("Interaction VFX")]
    [SerializeField] protected ClickVFXSpawner clickVFXSpawner;

    // Pointer-drag state
    protected Vector2 _pressScreenPos;
    protected bool _firedThisDrag;      // A boolean flag that make sure swap happens only once per one drag.

    // Gamepad/keyboard selection
    protected bool _hasSelection;
    protected float _nextMoveTime;
    protected float _nextMoveHoldTime;
    protected Vector2Int _lastMovedDirection;
    protected float _nextSwapTime;
    protected float _nextSwapHoldTime;

    protected int _selRow = INVALID, _selCol = INVALID;

    private const int INVALID = -1;


    protected bool _isConfirmPressing;
    protected bool _isMoveHolding;

    protected bool _isPadMode = false;

    protected void OnEnable()
    {
        // Make sure actions are enabled (PlayerInput usually does this, but it's safe)
        point.action.Enable();
        press.action.Enable();
        move.action.Enable();
        confirm.action.Enable();

        press.action.started += OnPressStarted;
        press.action.canceled += OnPressCanceled;
        confirm.action.started += OnConfirmStarted;
        confirm.action.canceled += OnConfirmCanceled;
        move.action.started += OnMoveStarted;
        move.action.performed += OnMovePerformed;
        move.action.canceled += OnMoveCanceled;
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

        point.action.Disable();
        press.action.Disable();
        move.action.Disable();
        confirm.action.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_board.InputEnabled)
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
            if (_board.InBounds(_selRow, _selCol) && !_firedThisDrag && press.action.IsPressed())
            {
                Vector2 delta = GetCurrentFollowPoint() - _pressScreenPos;
                if (delta.magnitude >= _dragTresholdPixels)
                {
                    Vector2Int dir = Decide4Way(delta);
                    _firedThisDrag = true;
                    _board.TrySwapFrom(new Vector2Int(_selRow, _selCol), dir);
                }
            }
        }
    }

    // ------- Pointer events --------
    protected void OnPressStarted(InputAction.CallbackContext _)
    {
        _isPadMode = false;

        transitionManager.BeginSkipHold();

        if (!_board.InputEnabled)
        {
            clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint());
            return;
        }


        _pressScreenPos = GetCurrentFollowPoint();
        var index = GemIndexUnderCursor(_pressScreenPos);
        if (!_board.InBounds(index.x, index.y))
        {
            clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint());
            return;
        }
        _firedThisDrag = false;

        SetSelection(index.x, index.y);

        clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint(), _board.GetGemColor(index));
    }

    protected void OnPressCanceled(InputAction.CallbackContext _)
    {
        transitionManager.EndSkipHold();

        ClearSelection();
        _firedThisDrag = false;
    }


    protected void OnMoveStarted(InputAction.CallbackContext _)
    {
        _isMoveHolding = true;
    }

    protected void OnMoveCanceled(InputAction.CallbackContext _)
    {
        _isMoveHolding = false;
    }

    // ------- Gamepad \ Keyboard ---------
    protected void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!_board.InputEnabled)
        {
            return;
        }


        _isPadMode = true;

        Vector2 direction = context.ReadValue<Vector2>();

        if (direction.sqrMagnitude < _stickDeadZone * _stickDeadZone)
        {
            _lastMovedDirection = Vector2Int.zero;

            Debug.LogWarning("Skipped by stick dead zone", this);
            return;
        }


        if (_isConfirmPressing)
        {
            SwapGem(direction);
        }
        else
        {
            SelectGem(direction);
        }
    }

    protected void OnConfirmStarted(InputAction.CallbackContext _)
    {
        _isPadMode = true;

        _isConfirmPressing = true;
    }

    protected void OnConfirmCanceled(InputAction.CallbackContext _)
    {
        _isConfirmPressing = false;
    }

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
        Vector2 world = _cam.ScreenToWorldPoint(screenPos);
        Vector2 local = _board.transform.InverseTransformPoint(world);

        return _board.GetGemIndex(local);
    }

    protected void SetSelection(int r, int c)
    {
        _hasSelection = true;
        _selRow = r;
        _selCol = c;

        gemSelectionHighlight.HighlightCell(_selRow, _selCol);
    }

    protected void ClearSelection()
    {
        _hasSelection = false;
        _selRow = INVALID;
        _selCol = INVALID;

        gemSelectionHighlight.HighlightCell(_selRow, _selCol);
    }

    public Vector2 GetCurrentFollowPoint()
    {
        return point.action.ReadValue<Vector2>();
    }

    protected void SelectGem(Vector2 direction)
    {
        // Throttle repeats
        if (_hasSelection && Time.time < _nextMoveTime)
        {
            // Checks _hasSelection because response immediately for the first selection interaction
            Debug.LogWarning("Skipped by response time not satisfied", this);
            return;
        }


        Vector2Int directionInt = Decide4Way(direction);

        if (directionInt == _lastMovedDirection)
        {
            // Skip movements to the same direction
            Debug.LogWarning("directionInt == _lastMovedDirection", this);
            return;
        }


        bool hasMoved = false;

        // Move repeat delay needs to be larger than double time of gem moving duration.
        _nextMoveTime = Time.time + (_hasSelection ? _moveRepeatRate : _moveRepeatDelay);


        if (!_hasSelection)
        {   // Init, nothing has selected.

            // start centered
            int r = Mathf.Clamp(_board.Rows / 2, 0, _board.Rows - 1);
            int c = Mathf.Clamp(_board.Cols / 2, 0, _board.Cols - 1);
            SetSelection(r, c);

            hasMoved = true;
        }
        else
        {
            // Typical case, move cursor or try swapping gems
            int nr = _selRow + directionInt.y;
            int nc = _selCol + directionInt.x;
            if (_board.InBounds(nr, nc))
            {
                // Move cursor
                SetSelection(nr, nc);

                hasMoved = true;
            }
            else
            {
                // @@ TODO: Add feedback this is invalid and impossible.
            }
        }

        if (hasMoved)
        {
            _nextMoveHoldTime = Time.time + _holdingRepeatDelay;
            _lastMovedDirection = directionInt;
        }
    }

    protected void SwapGem(Vector2 direction)
    {
        // Throttle repeats
        if (!_hasSelection || Time.time < _nextSwapTime)
        {
            // Checks _hasSelection because response immediately for the first selection interaction
            Debug.LogWarning("Tried swapping not selected. OR Skipped by response time not satisfied.", this);
            return;
        }

        _nextSwapTime = Time.time + _swapRepeatRate;
        _nextSwapHoldTime = Time.time + _holdingRepeatDelay;

        Vector2Int directionInt = Decide4Way(direction);

        // Try swapping gems
        _board.TrySwapFrom(new Vector2Int(_selRow, _selCol), directionInt);

    }

    protected void HoldingMoveAction()
    {
        if (Time.time > _nextMoveHoldTime)
        {
            if (_isMoveHolding)
            {
                int nr = _selRow + _lastMovedDirection.y;
                int nc = _selCol + _lastMovedDirection.x;
                if (_board.InBounds(nr, nc))
                {
                    // Move cursor
                    SetSelection(nr, nc);
                    _nextMoveHoldTime = Time.time + _moveRepeatRate;
                }
            }

            return;
        }


        //if (Time.time > _nextSwapHoldTime)
        //{
        //    if (_isConfirmPressing && _isMoveHolding)
        //    {
        //        Vector2Int directionInt = Decide4Way(move.action.ReadValue<Vector2>());
        //        // Try swapping gems
        //        _board.TrySwapFrom(new Vector2Int(_selRow, _selCol), directionInt);

        //        _nextSwapHoldTime = Time.time + _swapRepeatRate;
        //    }
        //    return;
        //}
    }
}