// SPDX-License-Identifier: MIT
// Copyright (c) 02/10/2026 Sinil Kang
// Project: Project JM - https://github.com/IDokan/Project_JM
// File: BrokenEggController.cs
// Summary: A script to control transform and torque of broken eggs.

using UnityEngine;

public class BrokenEggController : MonoBehaviour
{
    [SerializeField] protected GameObject leftEgg;
    [SerializeField] protected GameObject rightEgg;

    [SerializeField] protected Vector3 leftOffset;
    [SerializeField] protected Vector3 rightOffset;

    [SerializeField] protected float reactionMultiplier = 0.5f;

    public void Init(Transform eggTransform, float speed)
    {
        SetBrokenEgg(leftEgg, leftOffset, true, eggTransform, speed);


        SetBrokenEgg(rightEgg, rightOffset, false, eggTransform, speed);
    }

    protected void SetBrokenEgg(GameObject egg, Vector3 offset, bool isLeft, Transform parentTransform, float speed)
    {
        egg.transform.position = parentTransform.transform.position;
        egg.transform.rotation = parentTransform.transform.rotation;
        egg.transform.localScale = parentTransform.transform.localScale;

        Vector3 worldOffset = parentTransform.TransformDirection(offset);
        egg.transform.localPosition += worldOffset;

        egg.transform.SetParent(null);

        Rigidbody2D rigidbody = egg.GetComponent<Rigidbody2D>();

        if (rigidbody != null)
        {
            rigidbody.AddTorque(isLeft ? 5 : -5, ForceMode2D.Impulse);

            Vector3 worldVelocity = parentTransform.TransformDirection(new Vector3(0f, reactionMultiplier * speed, 0f));
            rigidbody.linearVelocity = new Vector2(worldVelocity.x, worldVelocity.y);
        }
    }
}
