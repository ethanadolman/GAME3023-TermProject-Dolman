using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using Random = UnityEngine.Random;

public class Character : MonoBehaviour
{
    public float moveSpeed; // The speed at which the player moves.

    private CharacterAnimator animator;

    public bool IsMoving { get; private set; }

    private void Awake()
    {
        animator = GetComponent<CharacterAnimator>();
    }

    public IEnumerator Move(Vector2 moveVec, Action OnMoveOver=null)
    {
        animator.MoveX = Mathf.Clamp(moveVec.x, -1f, 1f);
        animator.MoveY = Mathf.Clamp(moveVec.y, -1f, 1f);
        // Calculate the target position based on current position and input.
        var targetPos = transform.position;
        targetPos.x += moveVec.x;
        targetPos.y += moveVec.y;

        

        if(!IsWalkable(targetPos))
            yield break;
        IsMoving = true; // Set the isMoving flag to true to prevent further input.

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Move the player's position closer to the target position over time.
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame before continuing the loop.
        }

        // Ensure the player reaches the exact target position and set isMoving back to false.
        transform.position = targetPos;
        IsMoving = false; // The player is no longer moving.

        OnMoveOver?.Invoke();
    }

    public void HandleUpdate()
    {
        animator.IsMoving = IsMoving;
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.15f, GameLayers.i.SolidLayer | GameLayers.i.InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    public CharacterAnimator Animator => animator;
}
