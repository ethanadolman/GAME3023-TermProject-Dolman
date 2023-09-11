using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon/Create new pokemon")]
public class PokemonBase : ScriptableObject
{
    [SerializeField] private string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;

    [SerializeField] private PokemonType type;


    //Base Stats
    [SerializeField] private int maxHp;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int spAttack;
    [SerializeField] private int spDefense;
    [SerializeField] private int speed;

    [SerializeField] private List<LearnableMove> learnableMoves;
    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public Sprite FrontSprite
    { 
        get { return frontSprite; }

    }
    public Sprite BackSprite
    { 
        get { return backSprite; }

    }
    public PokemonType Type
    {
        get { return type; }
    }
    public int MaxHp
    {
        get { return maxHp; }
    }

    public int Attack
    {
        get { return attack; }
    }

    public int Defense
    {
        get { return defense; }
    }

    public int SpAttack
    {
        get { return spAttack; }
    }

    public int SpDefense
    {
        get { return spDefense; }
    }

    public int Speed
    {
        get { return speed; }
    }

    public List<LearnableMove> LearnableMoves
    {
        get { return learnableMoves; }
    }
}

[System.Serializable]
public class LearnableMove
{
    [SerializeField] private MoveBase moveBase;
    [SerializeField] private int level;

    public MoveBase Base
    {
        get { return moveBase; }
    }

    public int Level
    {
        get { return level; }
    }

}

public enum PokemonType
{
    None,
    Normal,
    Fire,
    Water,
    Grass
}
