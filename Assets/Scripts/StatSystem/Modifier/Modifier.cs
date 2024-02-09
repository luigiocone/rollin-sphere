using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New Modifier", menuName = "ScriptableObjects/Stats/Modifier")]
public class Modifier : ScriptableObject, IComparable<Modifier>
{
    public enum ModifierType
    {                           // Enum value defines the application order
        ADDER    = 100,
        FACTOR   = 200
    }

    public StatId StatId; 
    public ModifierType Type;
    public float Amount;
    [HideInInspector] public GameObject source;

    [SerializeField, Min(0f), Tooltip("Time measured in seconds")]
    public float Duration, ApplyPeriod;

    float FirstApplicationTime = 0f;
    float LastApplicationTime = 0f;
    int count = 0;

    public bool IsAppliable { get; private set; } = false;
    public bool IsInfinite => Duration == float.PositiveInfinity;

    public void ApplyModifier(Stat stat)
    {
        if (!IsAppliable) return;
        Action<Stat> applier = Type switch
        {
            ModifierType.ADDER => Addendum,
            ModifierType.FACTOR => Factor,
            _ => Addendum,
        };
        applier(stat);
        LastApplicationTime = Time.time;
        count++;
    }

    void Addendum(Stat stat) =>
        stat.CurrValue += Amount;

    void Factor(Stat stat) =>
        stat.CurrValue += stat.BaseValue * Amount;

    public bool IsExpired()
    {
        if (IsInfinite) return false;
        if (count == 0) return false;

        if (ApplyPeriod != 0f && count == Mathf.RoundToInt(Duration / ApplyPeriod))
            return true;
            
        if (LastApplicationTime - FirstApplicationTime >= Duration)
            return true;

        return false;
    }

    public void UpdateTimeVariables()
    {
        IsAppliable = false;  

        if (count == 0)
        {
            // First time that this modifier is applied
            IsAppliable = true;
            FirstApplicationTime = LastApplicationTime = Time.time;
            return;
	    }

        if (this.IsExpired())
            return;

        if ((Time.time - LastApplicationTime) < ApplyPeriod)
            return;

        IsAppliable = true;
    }

    public int CompareTo(Modifier mod)
    {
        // Useful to keep consistency in case of multiple modifiers
        var a = this;
        var b = mod;

        if (a.Type < b.Type)
            return -1;

        if (a.Type > b.Type)
            return 1;
        
        return 0; 
    }

    public void Reset()
    { 
        this.FirstApplicationTime = this.LastApplicationTime = 0f;
        this.count = 0;
        this.IsAppliable = false;
    }

    public void Copy(Modifier template)
    {
        this.StatId = template.StatId;
        this.Type = template.Type;
        this.Amount = template.Amount;
        this.source = template.source;
        this.Duration = template.Duration;
        this.ApplyPeriod = template.ApplyPeriod;
        this.Reset();
    }
}

