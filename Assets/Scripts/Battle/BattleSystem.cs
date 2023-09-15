using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public enum BattleState { Start, PlayerAction, PlayerMove, EnemyMove, Busy }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleHud playerHud;
    [SerializeField] private BattleHud enemyHud;
    [SerializeField] private BattleDialogBox dialogBox;

    [SerializeField] private AudioClip wildBattleMusic;
    [SerializeField] private AudioClip battleVictoryMusic;

    [SerializeField] private AudioClip pokemonCrySfx;
    [SerializeField] private AudioClip hitWeak;
    [SerializeField] private AudioClip hitNormal;
    [SerializeField] private AudioClip hitSuperEffective;
    [SerializeField] private AudioClip pokemonLowHealth;
    [SerializeField] private AudioClip pokemonFaint;
    [SerializeField] private AudioClip pokemonFlee;

    public event Action<bool> OnBattleOver;   

    BattleState state;
    private int currentAction;
    private int currentMove;
    private int fleeAttempts;

    private PokemonParty playerParty;
    private Pokemon wildPokemon;

    public void StartBattle(PokemonParty playerParty, Pokemon wildPokemon)
    {
        this.playerParty = playerParty;
        this.wildPokemon = wildPokemon;
        StartCoroutine(SetupBattle());
    }

    public IEnumerator SetupBattle()
    {
        playerUnit.SetUp(playerParty.GetHealthyPokemon());
        enemyUnit.SetUp(wildPokemon);
        playerHud.SetData(playerUnit.Pokemon);
        enemyHud.SetData(enemyUnit.Pokemon);

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        AudioManager.i.PlayMusic(wildBattleMusic);
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));
        AudioManager.i.PlaySfx(pokemonCrySfx);
        fleeAttempts = 0;
        PlayerAction();
    }

    void PlayerAction()
    {
        state = BattleState.PlayerAction;
        StartCoroutine(dialogBox.TypeDialog("choose an action"));
        dialogBox.EnableActionSelector(true);
    }

    void PlayerMove()
    {
        state = BattleState.PlayerMove;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PerformPlayerMove()
    {
        state = BattleState.Busy;
        var move = playerUnit.Pokemon.Moves[currentMove];
        move.PP--;
        yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} used {move.Base.Name}");

        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        enemyUnit.PlayHitAnimation();
        var damageDetails = enemyUnit.Pokemon.TakeDamage(move, playerUnit.Pokemon);
        yield return ShowDamageDetails(damageDetails);
        yield return enemyHud.UpdateHP();
        if (damageDetails.Fainted)
        {
              
            yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} Fainted");
            AudioManager.i.PlaySfx(pokemonFaint);
            enemyUnit.PlayFaintAnimation();

            AudioManager.i.PlayMusic(battleVictoryMusic, false);
            yield return new WaitForSeconds(5f);
                OnBattleOver(true);
        }
        else
        {
            StartCoroutine(EnemyMove());
        }
    }

    IEnumerator PerformPlayerFlee()
    {
        state = BattleState.Busy;
        fleeAttempts++;
        int escapeOdds = ((playerUnit.Pokemon.Speed * 128) / enemyUnit.Pokemon.Speed) + (30 * fleeAttempts);
        if (escapeOdds > Random.Range(0, 256))
        {
            AudioManager.i.PlaySfx(pokemonFlee);
            yield return dialogBox.TypeDialog($"you got away!");
            yield return new WaitForSeconds(0.25f);
            OnBattleOver(false);
        }
        else
        {
            yield return dialogBox.TypeDialog($"you couldn't get away!");
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.EnemyMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        move.PP--;
        yield return dialogBox.TypeDialog($"{enemyUnit.Pokemon.Base.Name} used {move.Base.Name}");

        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);

        playerUnit.PlayHitAnimation();
        var damageDetails = playerUnit.Pokemon.TakeDamage(move, enemyUnit.Pokemon);
        yield return ShowDamageDetails(damageDetails);
        yield return playerHud.UpdateHP();
        

        if (damageDetails.Fainted)
        {
            AudioManager.i.ClearSfx();
            yield return dialogBox.TypeDialog($"{playerUnit.Pokemon.Base.Name} Fainted");
            AudioManager.i.PlaySfx(pokemonFaint);
            playerUnit.PlayFaintAnimation();

            yield return new WaitForSeconds(2f);

            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
            {
                playerUnit.SetUp(nextPokemon);
                playerHud.SetData(nextPokemon);  

                dialogBox.SetMoveNames(nextPokemon.Moves);
                yield return StartCoroutine(dialogBox.TypeDialog($"Go {nextPokemon.Base.Name}!"));
                AudioManager.i.PlaySfx(pokemonCrySfx);
                fleeAttempts = 0;
                PlayerAction();
            }
            else
            {
                OnBattleOver(false);
            }
        }
        else
        {
            if (playerUnit.Pokemon.HP <= playerUnit.Pokemon.MaxHp * 0.10f)
            {
                AudioManager.i.ClearSfx();
                AudioManager.i.PlaySfx(pokemonLowHealth, true);
            }
            PlayerAction();
        }
    }

    IEnumerator ShowDamageDetails(DamageDetails damageDetails)
    {
        if (damageDetails.Critical > 1f)
            yield return dialogBox.TypeDialog($"A critical hit!");

        if (damageDetails.TypeEffectiveness > 1f)
        {
            AudioManager.i.PlaySfx(hitSuperEffective);
            yield return dialogBox.TypeDialog($"It's super effective!");
        }
        else if (damageDetails.TypeEffectiveness < 1f)
        {
            AudioManager.i.PlaySfx(hitWeak);
            yield return dialogBox.TypeDialog($"It's not very effective!");
        }
        else
        {
            AudioManager.i.PlaySfx(hitNormal);
            yield return null;
        }
    }

    public void HandleUpdate()
    {
        if (state == BattleState.PlayerAction)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.PlayerMove)
        {
            HandleMoveSelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (currentAction < 1)
                ++currentAction;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (currentAction > 0)
                --currentAction;
        }

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentAction == 0)
            {
                //Fight
                PlayerMove();
            }
            else if (currentAction == 1)
            {
                //Run
                StartCoroutine(PerformPlayerFlee());
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 1)
                ++currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            if (currentMove > 0)
                --currentMove;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (currentMove < playerUnit.Pokemon.Moves.Count - 2)
                currentMove += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (currentMove > 1)
                currentMove -= 2;
        }
        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PerformPlayerMove());
        }
    }
}
