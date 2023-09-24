using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed; // The speed at which the player moves.
    public LayerMask SolidObjectsLayer;
    public LayerMask InteractableLayer;
    public LayerMask EncountersLayer;
    [SerializeField] private AudioClip tallGrass;

    public event Action OnEncountered;

    public bool isMoving; // Indicates whether the player is currently in motion.

    private Vector2 input; // Stores input from the player for movement direction (x and y).

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    public void HandleUpdate()
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

                //If the tile is walkable, start the movement coroutine to move the player to the target position.
                if (IsWalkable(targetPos))
                    StartCoroutine(Move(targetPos));
            }
        }
        animator.SetBool("isMoving", isMoving);

        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            Interact();
    }


    void Interact()
    {
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);
       var collider = Physics2D.OverlapCircle(interactPos, 0.15f, InteractableLayer);
       if (collider != null)
       {
           collider.GetComponent<Interactable>()?.Interact();
       }
    }

    // Coroutine for smoothly moving the player to a target position.
    IEnumerator Move(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.15f, EncountersLayer) != null && isMoving == false)
        {
            AudioManager.i.PlaySfx(tallGrass, true, Random.Range(0, tallGrass.length));
        }
        isMoving = true; // Set the isMoving flag to true to prevent further input.
        
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Move the player's position closer to the target position over time.
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null; // Wait for the next frame before continuing the loop.
        }

        AudioManager.i.ClearSfx();
        // Ensure the player reaches the exact target position and set isMoving back to false.
        transform.position = targetPos;
        isMoving = false; // The player is no longer moving.

        CheckForEncounters();
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.15f, SolidObjectsLayer | InteractableLayer) != null)
        {
            return false;
        }
        return true;
    }

    private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.15f, EncountersLayer) != null)
        {
            
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", isMoving);
                OnEncountered();
            };

        }
    }
}
