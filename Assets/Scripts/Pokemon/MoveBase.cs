using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Move/Create new move")]
public class MoveBase : ScriptableObject
{
    [SerializeField] private string name;

    [TextArea]
    [SerializeField] string description;

    [SerializeField] private PokemonType type;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private int pp;

    public string Name
    {
        get { return name; }
    }

    public string Description
    {
        get { return description; }
    }

    public PokemonType Type
    {
        get { return type; }
    }

    public int Power
    {
        get { return power; }
    }

    public int Accuracy
    {
        get { return accuracy; }
    }

    public int PP
    {
        get { return pp; }
    }

    public bool isSpecial
    {
        get
        {
            if (type is PokemonType.Normal or PokemonType.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
