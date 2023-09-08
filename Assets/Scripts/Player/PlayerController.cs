using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed; // The speed at which the player moves.

    public bool isMoving; // Indicates whether the player is currently in motion.

    private Vector2 input; // Stores input from the player for movement direction (x and y).

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            // Get input from the player for horizontal and vertical movement.
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Remove diagonal input to ensure movement is in one direction at a time.
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);
                // Calculate the target position based on current position and input.
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                // Start the movement coroutine to move the player to the target position.
                StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    // Coroutine for smoothly moving the player to a target position.
    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true; // Set the isMoving flag to true to prevent further input.

        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Move the player's position closer to the target position over time.
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame before continuing the loop.
        }

        // Ensure the player reaches the exact target position and set isMoving back to false.
        transform.position = targetPos;
        isMoving = false; // The player is no longer moving.
    }
}
