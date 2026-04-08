// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 11/05/2025 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CombatController.cs
// Summary: Player controller for the combat scene; handles gem selection, dragging, and swapping on the board.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using UnityEngine;
using UnityEngine.InputSystem;

public class CombatController : BasePlayerController
{
    [Header("Refs")]
    [SerializeField] private BoardManager board;
    [SerializeField] private GemSelectionHighlightManager gemSelectionHighlightManager;
    [SerializeField] private TransitionManager transitionManager;
    [SerializeField] private PauseMenu pauseMenu;

    [Header("Tuning")]
    [SerializeField] private float dragThresholdPixels = 16f;
    [SerializeField] private float moveRepeatRate = 0.12f;
    [SerializeField] private float stickDeadZone = 0.25f;
    [SerializeField] private float swapRepeatRate = 0.4f;
    [SerializeField] private float holdingRepeatDelay = 0.8f;

    // Pointer-drag state
    private Vector2 _pressScreenPos;
    private bool _firedThisDrag;      // Ensures swap fires only once per drag.

    // Gamepad/keyboard selection
    private bool _hasSelection;
    private float _nextMoveHoldTime;
    private Vector2Int _lastMovedDirection;
    private float _nextSwapTime;

    private int _selRow = INVALID, _selCol = INVALID;
    private const int INVALID = -1;

    private bool _isConfirmPressing;
    private bool _isMoveHolding;

    private void Start()
    {
        // Explicitly disable the UI map before switching to Gameplay.
        // SwitchToGameplayMap() alone leaves the UI map active, causing both
        // maps to process any shared input binding simultaneously. Without this,
        // the first ESC press fires both Gameplay's Cancel (open pause) and UI's
        // Cancel (close pause) in the same frame — making ESC appear broken.
        playerInput.actions.FindActionMap("UI").Disable();
        playerInput.SwitchToGameplayMap();
    }

    private void Update()
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
            // Pointer drag: once threshold exceeded, decide a 4-way dir and trigger one swap.
            if (board.InBounds(_selRow, _selCol) && !_firedThisDrag && playerInput.actions["press"].IsPressed())
            {
                Vector2 delta = GetCurrentFollowPoint() - _pressScreenPos;
                if (delta.magnitude >= dragThresholdPixels)
                {
                    Vector2Int dir = Decide4Way(delta);
                    _firedThisDrag = true;
                    board.TrySwapFrom(new Vector2Int(_selRow, _selCol), dir);
                }
            }
        }
    }

    // ------- Pointer events --------
    protected override void OnPressStarted(InputAction.CallbackContext _)
    {
        if (pauseMenu.IsPaused)
        {
            clickVFXSpawner.SpawnClickVFX(GetCurrentFollowPoint());
            return;
        }

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

    protected override void OnPressCanceled(InputAction.CallbackContext _)
    {
        transitionManager.EndSkipHold();

        if (pauseMenu.IsPaused)
        {
            return;
        }

        ClearSelection();
        _firedThisDrag = false;
    }

    // ------- Gamepad / Keyboard --------

    // Stick started latching.
    protected override void OnMoveStarted(InputAction.CallbackContext _)
    {
        _isMoveHolding = true;
    }

    // Stick returned to idle position.
    protected override void OnMoveCanceled(InputAction.CallbackContext _)
    {
        _isMoveHolding = false;
        _lastMovedDirection = Vector2Int.zero;
    }

    // Called every time the stick value changes.
    protected override void OnMovePerformed(InputAction.CallbackContext context)
    {
        _isPadMode = true;

        Vector2 direction = context.ReadValue<Vector2>();

        if (direction.sqrMagnitude < stickDeadZone * stickDeadZone)
        {
            _lastMovedDirection = Vector2Int.zero;
            return;
        }

        // Skip actions queried by the same direction.
        Vector2Int directionInt = Decide4Way(direction);

        if (directionInt == _lastMovedDirection)
        {
            return;
        }

        bool isPerformed = false;

        if (_isConfirmPressing && IsBoardInputEnabled())
        {
            isPerformed = SwapGem(directionInt);
        }
        else if (!pauseMenu.IsPaused)        // Allow gem selection even when board is disabled.
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

    protected override void OnConfirmStarted(InputAction.CallbackContext _)
    {
        transitionManager.BeginSkipHold();
        _isPadMode = true;

        if (IsBoardInputEnabled())
        {
            _isConfirmPressing = true;
            gemSelectionHighlightManager.EnableArrows(_selRow, _selCol);
        }
    }

    protected override void OnConfirmCanceled(InputAction.CallbackContext _)
    {
        transitionManager.EndSkipHold();
        _isConfirmPressing = false;
        gemSelectionHighlightManager.DisableArrows();
    }

    protected override void OnCancelPerformed(InputAction.CallbackContext _)
    {
        pauseMenu.Show();
    }

    // ----- Helpers ---------

    private bool IsBoardInputEnabled() => board.InputEnabled && !pauseMenu.IsPaused;

    private Vector2Int GemIndexUnderCursor(Vector2 screenPos)
    {
        Vector2 world = cam.ScreenToWorldPoint(screenPos);
        Vector2 local = board.transform.InverseTransformPoint(world);
        return board.GetGemIndex(local);
    }

    private void SetSelection(int r, int c)
    {
        _hasSelection = true;
        _selRow = r;
        _selCol = c;
        gemSelectionHighlightManager.HighlightCell(_selRow, _selCol);
    }

    private void ClearSelection()
    {
        _hasSelection = false;
        _selRow = INVALID;
        _selCol = INVALID;
        gemSelectionHighlightManager.HighlightCell(_selRow, _selCol);
    }

    private bool SelectGem(Vector2Int direction)
    {
        if (!_hasSelection)
        {
            // Init: nothing selected, start centered.
            int r = Mathf.Clamp(board.Rows / 2, 0, board.Rows - 1);
            int c = Mathf.Clamp(board.Cols / 2, 0, board.Cols - 1);
            SetSelection(r, c);
            return true;
        }
        else
        {
            // Typical case: move cursor.
            int nr = _selRow + direction.y;
            int nc = _selCol + direction.x;
            if (board.InBounds(nr, nc))
            {
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

    private bool SwapGem(Vector2Int direction)
    {
        // Throttle repeats.
        if (!_hasSelection || Time.time < _nextSwapTime)
        {
            // Checks _hasSelection because response immediately for the first selection interaction.
            Debug.LogWarning("Tried swapping not selected. OR Skipped by response time not satisfied.", this);
            return false;
        }

        _nextSwapTime = Time.time + swapRepeatRate;
        board.TrySwapFrom(new Vector2Int(_selRow, _selCol), direction);
        return true;
    }

    private void HoldingMoveAction()
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
                SetSelection(nr, nc);
                _nextMoveHoldTime = Time.time + moveRepeatRate;
            }
        }
    }
}
