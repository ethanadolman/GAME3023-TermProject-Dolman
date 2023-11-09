using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private AudioClip tallGrass;

    [SerializeField] private string name;
    [SerializeField] private Sprite sprite;

    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainersView;

    private Vector2 input; // Stores input from the player for movement direction (x and y).

    private Character character;

    void Awake()
    {
        character = GetComponent<Character>();
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!character.IsMoving)
        {
            // Get input from the player for horizontal and vertical movement.
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            // Remove diagonal input to ensure movement is in one direction at a time.
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                if (Physics2D.OverlapCircle(transform.position, 0.15f, GameLayers.i.EncountersLayer) != null && character.Animator.IsMoving == false)
                {
                    AudioManager.i.PlayClip("TallGrass", true, Random.Range(0, tallGrass.length));
                }
                StartCoroutine(character.Move(input, OnMoveOver));
            }
        }

        character.HandleUpdate();

        if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            Interact();
    }


    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);
       var collider = Physics2D.OverlapCircle(interactPos, 0.15f, GameLayers.i.InteractableLayer);
       if (collider != null)
       {
           collider.GetComponent<Interactable>()?.Interact(transform);
       }
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckIfInTrainersView();
    }
    private void CheckForEncounters()
    {
        AudioManager.i.StopClip("TallGrass");
        if (Physics2D.OverlapCircle(transform.position, 0.15f, GameLayers.i.EncountersLayer) != null)
        {
            
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                character.Animator.IsMoving = false;
                OnEncountered();
            };

        }
    }

    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position, 0.15f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);
        }
    }

    public string Name { get => name; }
    public Sprite Sprite { get => sprite; }
    private void OnDestroy()
    {
        SaveData.i.SaveGame(character.transform.position);
    }
}
