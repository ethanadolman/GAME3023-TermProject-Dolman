using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour
{
    [SerializeField] private Dialog dialog;
    [SerializeField] private GameObject exclamation;
    [SerializeField] private GameObject fov;

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        //Show Exclamation
        exclamation.SetActive(true);
        AudioManager.i.PlayClip("TrainerExclamation");
        yield return new WaitForSeconds(0.5f);
        AudioManager.i.PlayMusic("TrainerEncounter");
        exclamation.SetActive(false);

        //Walk towards the player
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(MathF.Round(moveVec.x), MathF.Round(moveVec.y));

       yield return character.Move(moveVec);

       //Show dialog
       StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () => {
           Debug.Log("Start Trainer Battle");
       }));
    }

    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if (dir == FacingDirection.Right)
            angle = 90f;
        else if (dir == FacingDirection.Up)
            angle = 180f;
        else if (dir == FacingDirection.Left)
            angle = 270f;

        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
    }
}
