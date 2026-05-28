// SPDX-License-Identifier: LicenseRef-Proprietary
// Copyright (c) 12/04/2026 Sinil Kang. All Rights Reserved.
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ConstrainedWander.cs
// Summary: If this object strays beyond maxDistance from its origin, call Wander()
//          to move it to a random destination within destinationRange, then stop.
// Unauthorized copying, distribution, or modification of this file is strictly prohibited.

using System.Collections;
using UnityEngine;

public class ConstrainedWander : MonoBehaviour
{
    public enum Axis { XY, XOnly, YOnly }

    [SerializeField] protected Axis wanderAxis = Axis.XY;
    [SerializeField] protected float maxDistance = 1f;
    [SerializeField] protected Vector2 destinationRange = Vector2.one;
    [SerializeField] protected float moveSpeed = 1f;
    [SerializeField] protected float wanderDelay = 1f;

    private Vector3 _origin;
    private Coroutine _wanderRoutine;

    protected void Awake()
    {
        _origin = wanderAxis switch
        {
            Axis.XOnly => new Vector3(0, transform.localPosition.y, 0),
            Axis.YOnly => new Vector3(transform.localPosition.x, 0, 0),
            _          => transform.localPosition,
        };
    }

    protected void OnEnable()
    {
        StartCoroutine(AutoWanderLoop());
    }

    private IEnumerator AutoWanderLoop()
    {
        while (true)
        {
            yield return GlobalTimeManager.WaitForGlobalSeconds(wanderDelay);
            Wander();
        }
    }

    public void Wander()
    {
        if (!IsOutOfRange())
            return;

        if (_wanderRoutine != null)
        {
            StopCoroutine(_wanderRoutine);
        }
        _wanderRoutine = StartCoroutine(WanderRoutine());
    }

    private bool IsOutOfRange()
    {
        Vector3 delta = transform.localPosition - _origin;
        float distance = wanderAxis switch
        {
            Axis.XOnly => Mathf.Abs(delta.x),
            Axis.YOnly => Mathf.Abs(delta.y),
            _          => delta.magnitude,
        };
        return distance > maxDistance;
    }

    private IEnumerator WanderRoutine()
    {
        float x = wanderAxis != Axis.YOnly ? Random.Range(-destinationRange.x, destinationRange.x) : 0f;
        float y = wanderAxis != Axis.XOnly ? Random.Range(-destinationRange.y, destinationRange.y) : 0f;
        Vector3 destination = _origin + new Vector3(x, y, 0f);

        while (transform.localPosition != destination)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                destination,
                moveSpeed * GlobalTimeManager.DeltaTime);
            yield return null;
        }

        _wanderRoutine = null;
    }
}
