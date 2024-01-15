using System;
using UnityEngine;

public class PersistentStat : Stat 
{
    public PersistentStat(StatId id, float baseValue, StatSettings settings) 
	    : base(id, baseValue, settings) => this.OnValueChange += CheckValueChange; 

    public PersistentStat(Stat stat, StatSettings settings) 
	    : base(stat, settings) => this.OnValueChange += CheckValueChange; 

    public Action<StatValueChangeEvent> OnZero, OnIncrease, OnDecrease;

    // Base and Curr values will always be equal
    public override float BaseValue
    { 
	    get => base.BaseValue;
        set => base.BaseValue = currValue = value;
    }
    public override float CurrValue 
    { 
	    get => base.CurrValue;
        set => base.CurrValue = baseValue = value;
    }

    void CheckValueChange(StatValueChangeEvent evt)
    {
        if (Mathf.Approximately(evt.curr, 0f))
        {
            OnZero?.Invoke(evt);
            return;
        }

        if (evt.prev > evt.curr)
        {
            OnDecrease?.Invoke(evt);
            return;
        }

        OnIncrease?.Invoke(evt);
    }
}

