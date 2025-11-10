// SPDX-License-Identifier: MIT
// Copyright (c) 11/07/2025 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: CharacterCombatant.cs
// Summary: A combatant script for action characters.

using System.Collections;
using UnityEngine;

public interface ICombatant
{
    void ReceiveDamage(float damage);
}

public class CharacterCombatant : MonoBehaviour, ICombatant
{
    [SerializeField] protected string displayName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReceiveDamage(float damage)
    {
        // @@ TODO: later add HP, armor, UI, etc...
        Debug.Log($"{displayName} takes {damage} damage");
    }

}
