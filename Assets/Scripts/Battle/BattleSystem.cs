using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

public enum BattleState { Start, ActionSelection, MoveSelection, PerformMove, Busy, PartyScreen, BattleOver }

public class BattleSystem : MonoBehaviour
{
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleDialogBox dialogBox;
    [SerializeField] private PartyScreen partyScreen;

    [SerializeField] private AudioClip wildBattleMusic;
    [SerializeField] private AudioClip battleVictoryMusic;

    [SerializeField] private AudioClip pokemonCrySfx;
    [SerializeField] private AudioClip hitWeak;
    [SerializeField] private AudioClip hitNormal;
    [SerializeField] private AudioClip hitSuperEffective;
    [SerializeField] private AudioClip pokemonLowHealth;
    [SerializeField] private AudioClip pokemonFaint;
    [SerializeField] private AudioClip pokemonFlee;
    [SerializeField] private AudioClip statDown;
    [SerializeField] private AudioClip statUp;

    public event Action<bool> OnBattleOver;

    BattleState state;
    private int currentAction;
    private int currentMove;
    private int currentMember;
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

        partyScreen.Init();

        dialogBox.SetMoveNames(playerUnit.Pokemon.Moves);
        AudioManager.i.PlayMusic(wildBattleMusic);
        yield return StartCoroutine(dialogBox.TypeDialog($"A wild {enemyUnit.Pokemon.Base.Name} appeared."));
        AudioManager.i.PlaySfx(pokemonCrySfx);
        if (playerUnit.Pokemon.HP <= playerUnit.Pokemon.MaxHp * 0.10f)
        {
            AudioManager.i.PlaySfx(pokemonLowHealth, true);
        }
        fleeAttempts = 0;
        ChooseFirstTurn();
    }

    void ChooseFirstTurn()
    {
        if (playerUnit.Pokemon.Speed >= enemyUnit.Pokemon.Speed)
            ActionSelection();
        else
            StartCoroutine(EnemyMove());
    }

    void ActionSelection()
    {
        state = BattleState.ActionSelection;
        dialogBox.SetDialog("choose an action");
        dialogBox.EnableActionSelector(true);
    }

    void OpenPartyScreen()
    {
        state = BattleState.PartyScreen;
        partyScreen.SetPartyData(playerParty.Pokemons);
        partyScreen.gameObject.SetActive(true);
        dialogBox.EnableActionSelector(false);
    }

    void MoveSelection()
    {
        state = BattleState.MoveSelection;
        dialogBox.EnableActionSelector(false);
        dialogBox.EnableDialogText(false);
        dialogBox.EnableMoveSelector(true);
    }

    IEnumerator PlayerMove()
    {
        state = BattleState.PerformMove;
        var move = playerUnit.Pokemon.Moves[currentMove];
        yield return RunMove(playerUnit, enemyUnit, move);

        //If the battle stat was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
            StartCoroutine(EnemyMove());

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
            AudioManager.i.ClearSfx();
            BattleOver(false);
        }
        else
        {
            yield return dialogBox.TypeDialog($"you couldn't get away!");
            StartCoroutine(EnemyMove());
        }

    }

    IEnumerator EnemyMove()
    {
        state = BattleState.PerformMove;

        var move = enemyUnit.Pokemon.GetRandomMove();
        yield return RunMove(enemyUnit, playerUnit, move);

        //If the battle stat was not changed by RunMove, then go to next step
        if (state == BattleState.PerformMove)
            ActionSelection();

    }

    void BattleOver(bool won)
    {
        state = BattleState.BattleOver;
        playerParty.Pokemons.ForEach(p => p.OnBattleOver());
        OnBattleOver(won);
    }

    IEnumerator RunMove(BattleUnit sourceUnit, BattleUnit targetUnit, Move move)
    {
        bool canRunMove = sourceUnit.Pokemon.OnBeforeMove();
        if (!canRunMove)
        {
            yield return ShowStatusChange(sourceUnit.Pokemon);
            yield return sourceUnit.Hud.UpdateHP();
            yield break;
        }
        yield return ShowStatusChange(sourceUnit.Pokemon);

        move.PP--;
        yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} used {move.Base.Name}");

        sourceUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(1f);
        targetUnit.PlayHitAnimation();

        if (move.Base.Category == MoveCategory.Status)
        {
            yield return RunMoveEffects(move, sourceUnit.Pokemon, targetUnit.Pokemon);
        }
        else
        {
            var damageDetails = targetUnit.Pokemon.TakeDamage(move, sourceUnit.Pokemon);
            yield return ShowDamageDetails(damageDetails);
            yield return targetUnit.Hud.UpdateHP();
        }

        if (targetUnit.Pokemon.HP <= 0)
        {

            yield return dialogBox.TypeDialog($"{targetUnit.Pokemon.Base.Name} Fainted");
            AudioManager.i.PlaySfx(pokemonFaint);
            targetUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1f);

            CheckForBattleOver(targetUnit);
        }

        //Statuses like burn or psn will hurt the pokemon after the turn
        sourceUnit.Pokemon.OnAfterTurn();
        yield return ShowStatusChange(sourceUnit.Pokemon);
        yield return sourceUnit.Hud.UpdateHP();
        if (sourceUnit.Pokemon.HP <= 0)
        {

            yield return dialogBox.TypeDialog($"{sourceUnit.Pokemon.Base.Name} Fainted");
            AudioManager.i.PlaySfx(pokemonFaint);
            sourceUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(1f);

            CheckForBattleOver(sourceUnit);
        }


        if (playerUnit.Pokemon.HP <= playerUnit.Pokemon.MaxHp * 0.10f && playerUnit.Pokemon.HP > 0)
        {
            AudioManager.i.ClearSfx();
            AudioManager.i.PlaySfx(pokemonLowHealth, true);
        }
    }

    IEnumerator RunMoveEffects(Move move, Pokemon source, Pokemon target)
    {
        var effects = move.Base.Effects;

        //Stat Boosting
        if (effects.Boosts != null)
        {
            if (move.Base.Target == MoveTarget.Self)
            {
                source.ApplyBoosts(effects.Boosts);
                AudioManager.i.PlaySfx(statUp);
            }
            else
            {
                target.ApplyBoosts(effects.Boosts);
                AudioManager.i.PlaySfx(statDown);
            }
        }

        //Status Condition
        if (effects.Status != ConditionsID.none)
        {
            target.SetStatus(effects.Status);
        }

        //Volatile Status Condition
        if (effects.VolatileStatus != ConditionsID.none)
        {
            target.SetVolatileStatus(effects.VolatileStatus);
        }

        yield return ShowStatusChange(source);
        yield return ShowStatusChange(target);
    }

    IEnumerator ShowStatusChange(Pokemon pokemon)
    {
        while (pokemon.StatusChanges.Count > 0)
        {
            var message = pokemon.StatusChanges.Dequeue();
            yield return dialogBox.TypeDialog(message);
        }
    }

    void CheckForBattleOver(BattleUnit faintedUnit)
    {
        if (faintedUnit.IsPlayerUnit)
        {
            var nextPokemon = playerParty.GetHealthyPokemon();
            if (nextPokemon != null)
                OpenPartyScreen();
            else
                BattleOver(false);
        }
        else
        {
            AudioManager.i.PlayMusic(battleVictoryMusic, false);
            BattleOver(true);
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
        if (state == BattleState.ActionSelection)
        {
            HandleActionSelection();
        }
        else if (state == BattleState.MoveSelection)
        {
            HandleMoveSelection();
        }
        else if (state == BattleState.PartyScreen)
        {
            HandlePartySelection();
        }
    }

    void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ++currentAction;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            --currentAction;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            currentAction += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            currentAction -= 2;

        currentAction = Mathf.Clamp(currentAction, 0, 3);

        dialogBox.UpdateActionSelection(currentAction);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            if (currentAction == 0)
            {
                //Fight
                MoveSelection();
            }
            else if (currentAction == 1)
            {
                //Bag
            }
            else if (currentAction == 2)
            {
                //Pokemon
                OpenPartyScreen();
            }
            else if (currentAction == 3)
            {
                //Run
                StartCoroutine(PerformPlayerFlee());
            }
        }
    }

    void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ++currentMove;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            --currentMove;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            currentMove += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            currentMove -= 2;

        currentMove = Mathf.Clamp(currentMove, 0, playerUnit.Pokemon.Moves.Count - 1);

        dialogBox.UpdateMoveSelection(currentMove, playerUnit.Pokemon.Moves[currentMove]);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            StartCoroutine(PlayerMove());
        }
        else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
        {
            dialogBox.EnableMoveSelector(false);
            dialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }

    void HandlePartySelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            ++currentMember;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            --currentMember;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            currentMember += 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            currentMember -= 2;

        currentMember = Mathf.Clamp(currentMember, 0, playerParty.Pokemons.Count - 1);

        partyScreen.UpdateMemberSelection(currentMember);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
        {
            var selectedMember = playerParty.Pokemons[currentMember];
            if (selectedMember.HP <= 0)
            {
                partyScreen.SetMessageText("You can't send out a fainted pokemon");
                return;
            }

            if (selectedMember == playerUnit.Pokemon)
            {
                partyScreen.SetMessageText("You can't switch with the same pokemon");
                return;
            }

            partyScreen.gameObject.SetActive(false);
            state = BattleState.Busy;
            StartCoroutine(SwitchPokemon(selectedMember));
        }
        else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
        {
            partyScreen.gameObject.SetActive(false);
            ActionSelection();
        }
    }

    IEnumerator SwitchPokemon(Pokemon newPokemon)
    {
        playerUnit.Pokemon.CureVolatileStatus();
        bool currentPokemonFainted = true;
        if (playerUnit.Pokemon.HP > 0)
        {
            currentPokemonFainted = false;
            AudioManager.i.ClearSfx();
            yield return dialogBox.TypeDialog($"Come back {playerUnit.Pokemon.Base.Name}");
            AudioManager.i.PlaySfx(pokemonFaint);
            playerUnit.PlayFaintAnimation();
            yield return new WaitForSeconds(2f);
        }

        playerUnit.SetUp(newPokemon);
        dialogBox.SetMoveNames(newPokemon.Moves);
        yield return dialogBox.TypeDialog($"Go {newPokemon.Base.Name}!");
        AudioManager.i.PlaySfx(pokemonCrySfx);
        if (playerUnit.Pokemon.HP <= playerUnit.Pokemon.MaxHp * 0.10f)
        {
            AudioManager.i.PlaySfx(pokemonLowHealth, true);
        }
        yield return new WaitForSeconds(1f);

        if (currentPokemonFainted)
            ChooseFirstTurn();
        else
            StartCoroutine(EnemyMove());
    }
}
