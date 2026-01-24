// SPDX-License-Identifier: MIT
// Copyright (c) 01/23/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: StunRepresenter.cs
// Summary: A script that manages Stun effects.

using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class StunRepresenter : MonoBehaviour
{
    [SerializeField] protected GameObject stunVFXPrefab;
    [SerializeField] protected Transform vfxAnchor;

    protected Animator _animator;
    protected static readonly int StunTrig = Animator.StringToHash("StunTrig");
    protected static readonly int IsStunned = Animator.StringToHash("IsStunned");
    protected Coroutine _stunRoutine;

    protected void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Stun(float duration)
    {
        if (_animator == null || stunVFXPrefab == null)
        {
            return;
        }

        if (_stunRoutine != null)
        {
            StopCoroutine(_stunRoutine);
            _stunRoutine = null;
        }

        _animator.SetBool(IsStunned, true);
        _animator.ResetTrigger(StunTrig);
        _animator.SetTrigger(StunTrig);

        Transform spawnTransform = vfxAnchor ? vfxAnchor : transform;
        GameObject stunVFX = Instantiate(stunVFXPrefab, spawnTransform);

        RotateStunStars rotateScript = stunVFX.GetComponent<RotateStunStars>();
        if (rotateScript != null)
        {
            rotateScript.SetDuration(duration);
        }

        _stunRoutine = StartCoroutine(OnStunEnded(duration));
    }

    protected IEnumerator OnStunEnded(float duration)
    {
        if (duration > 0f)
        {
            yield return GlobalTimeManager.WaitForGlobalSeconds(duration);
        }

        _animator.SetBool(IsStunned, false);

        _stunRoutine = null;
    }
}
