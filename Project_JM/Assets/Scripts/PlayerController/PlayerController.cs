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
    [SerializeField] protected Camera _cam;

    [Header("Actions (drag from Controls.inpujtactions)")]
    public InputActionReference point;
    public InputActionReference press;
    public InputActionReference move;
    public InputActionReference confirm;

    [Header("Tuning")]
    [SerializeField] protected float _dragTresholdPixels = 16f;
    [SerializeField] protected float _moveRepeatDelay = 0.25f;
    [SerializeField] protected float _moveRepeatRate = 0.12f;
    [SerializeField] protected float _stickDeadZone = 0.25f;

    // Pointer-drag state
    protected Vector2 _pressScreenPos;
    protected bool _firedThisDrag;

    // Gamepad/keyboard selection
    protected bool _hasSelection;
    protected float _nextMoveTime;

    protected int _selRow, _selCol;

    static private int INVALID = -1;

    protected void OnEnable()
    {
        // Make sure actions are enabled (PlayerInput usually does this, but it's safe)
        point.action.Enable();
        press.action.Enable();
        move.action.Enable();
        confirm.action.Enable();

        press.action.started += OnPressStarted;
        press.action.canceled += OnPressCanceled;
        confirm.action.performed += OnConfirm;
        move.action.performed += OnMovePerformed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_board.InputEnabled) return;

        // Pointer drag : once threshold exceeded, decide a 4-way dir and trigger one swap
        if (_board.InBounds(_selRow, _selCol) && !_firedThisDrag && press.action.IsPressed())
        {
            Vector2 delta = point.action.ReadValue<Vector2>() - _pressScreenPos;
            if (delta.magnitude >= _dragTresholdPixels)
            {
                Vector2Int dir = Decide4Way(delta);
                _firedThisDrag = true;
                _board.TrySwapFrom(new Vector2Int(_selRow, _selCol), dir);
            }
        }
    }

    // ------- Pointer events --------
    protected void OnPressStarted(InputAction.CallbackContext _)
    {
        if (!_board.InputEnabled) return;

        _pressScreenPos = point.action.ReadValue<Vector2>();
        var index = GemIndexUnderCursor(_pressScreenPos);
        if (!_board.InBounds(index.x, index.y))
        {
            return;
        }
        _firedThisDrag = false;

        SetSelection(index.x, index.y, true);
    }

    protected void OnPressCanceled(InputAction.CallbackContext _)
    {
        _selRow = INVALID;
        _selCol = INVALID;
        _firedThisDrag = false;
    }

    // ------- Gamepad \ Keyboard ---------
    protected void OnMovePerformed(InputAction.CallbackContext context)
    {
        if (!_board.InputEnabled)
        {
            return;
        }

        Vector2 v = context.ReadValue<Vector2>();
        if (v.sqrMagnitude < _stickDeadZone * _stickDeadZone)
        {
            return;
        }

        // Throttle repeats
        if (_hasSelection && Time.time < _nextMoveTime)
        {
            return;
        }

        _nextMoveTime = Time.time + (_hasSelection ? _moveRepeatRate : _moveRepeatDelay);

        Vector2Int step = Decide4Way(v);
        if (!_hasSelection)
        {
            // @@ TODO: Change start anchor.
            // start centered
            int r = Mathf.Clamp(_board.Rows / 2, 0, _board.Rows - 1);
            int c = Mathf.Clamp(_board.Cols / 2, 0, _board.Cols - 1);
            SetSelection(r, c, true);
        }
        else
        {
            int nr = _selRow + step.y;
            int nc = _selCol + step.x;
            if (_board.InBounds(nr, nc))
            {
                SetSelection(nr, nc, true);
            }
            else
            {
                // @@ TODO: Add feedback this is invalid and impossible.
            }
        }
    }

    protected void OnConfirm(InputAction.CallbackContext _)
    {
        if (!_board.InputEnabled || !_hasSelection)
        {
            return;
        }

        // "Confirm + direction" pattern: Need current stick/keys direction
        Vector2 v = move.action.ReadValue<Vector2>();
        if (v.sqrMagnitude < _stickDeadZone * _stickDeadZone)
        {
            return;
        }

        Vector2Int dir = Decide4Way(v);
        if (_board.InBounds(_selRow, _selCol))
        {
            _board.TrySwapFrom(new Vector2Int(_selRow, _selCol), dir);
        }
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

    protected void SetSelection(int r, int c, bool highlight)
    {
        _hasSelection = true;
        _selRow = r; 
        _selCol = c;

        // board.HighlightCell(r, c, highlight); // optional visual (outline/ cursor)
    }
}