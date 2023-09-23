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
    [SerializeField] private bool alwaysHits;
    [SerializeField] private int pp;
    [SerializeField] private int priority;
    [SerializeField] MoveCategory category;
    [SerializeField] MoveEffects effects;
    [SerializeField] private List<SecondaryEffects> secondaries;
    [SerializeField] MoveTarget target;

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

    public bool AlwaysHits
    {
        get { return alwaysHits; }
    }

    public int PP
    {
        get { return pp; }
    }

    public int Priority
    {
        get { return priority; }
    }

    public MoveCategory Category
    {
        get { return category; }
    }

    public MoveEffects Effects
    {
        get { return effects; }
    }

    public List<SecondaryEffects> Secondaries
    {
        get { return secondaries; }
    }
    public MoveTarget Target
    {
        get { return target; }
    }

}

[System.Serializable]
public class MoveEffects
{
    [SerializeField] private List<StatBoost> boosts;
    [SerializeField] ConditionsID status;
    [SerializeField] ConditionsID volatileStatus;

    public List<StatBoost> Boosts
    {
        get { return boosts; }
    }

    public ConditionsID Status
    {
        get { return status; }
    }

    public ConditionsID VolatileStatus
    {
        get { return volatileStatus; }
    }
}

[System.Serializable]
public class SecondaryEffects : MoveEffects
{
    [SerializeField] private int chance;
    [SerializeField] private MoveTarget target;

    public int Chance
    {
        get { return chance; }
    }

    public MoveTarget Target
    {
        get { return target; }
    }
}

[System.Serializable]
public class StatBoost
{
    public Stat stat;
    public int boost;
}
public enum MoveCategory
{
    Physical, Special, Status
}

public enum MoveTarget
{
    Foe, Self
}