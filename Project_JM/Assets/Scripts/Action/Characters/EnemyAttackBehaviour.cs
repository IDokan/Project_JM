// SPDX-License-Identifier: MIT
// Copyright (c) 11/11/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: EnemyAttackBehaviour.cs
// Summary: A script for enemy combat behaviour.


using System.Collections;
using UnityEngine;

public class EnemyAttackBehaviour : MonoBehaviour
{
    [SerializeField] protected EnemyAttackEventChannel _attackChannel;
    [SerializeField] protected AttackLogic _attackLogic;
    [SerializeField] protected BoardDisableEventChannel _boardDisableChannel;
    [SerializeField] protected BoardDisableLogic _boardDisableLogic;
    [SerializeField, Min(0.001f)] protected float _cooldown = 5f;

    protected Coroutine _loop;

    protected void OnEnable()
    {
        _loop = StartCoroutine(Loop());
    }
    protected void OnDisable()
    {
        if(_loop != null )
        {
            StopCoroutine(_loop);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    protected IEnumerator Loop()
    {
        var wait = new WaitForSeconds(_cooldown );

        while(true)
        {
            Attack();
            yield return DisableBoard();

            yield return wait;
        }
    }

    protected void Attack()
    {
        _attackChannel.Raise(_attackLogic);
    }


    protected IEnumerator DisableBoard()
    {
        yield return (_boardDisableLogic.Execute(_boardDisableChannel));
    }
}
