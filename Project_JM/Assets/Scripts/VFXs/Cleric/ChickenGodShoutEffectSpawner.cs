// SPDX-License-Identifier: MIT
// Copyright (c) 02/14/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: ChickenGodShoutEffectSpawner.cs
// Summary: A script to spawn Chicken God's shout effect.

using UnityEngine;

public class ChickenGodShoutEffectSpawner : AbstractAnimEventPrefabSpawner<GameObject>
{
    protected Renderer _renderer;
    protected MaterialPropertyBlock _materialPropertyBlock;
    protected int _startTimeID;

    public void AnimEvent_ShoutEffectSpawner()
    {
        GameObject shoutVFX = Spawn();


        _renderer = shoutVFX.GetComponent<Renderer>();
        if (_materialPropertyBlock == null)
        {
            _materialPropertyBlock = new MaterialPropertyBlock();
        }
        _startTimeID = Shader.PropertyToID("_StartTime");

        _renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetFloat(_startTimeID, Time.time);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }
}
