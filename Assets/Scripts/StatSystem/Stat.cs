using System;
using UnityEngine;

public enum StatId
{
    HEALTH,
    MAX_HEALTH,
    STAMINA,
    MAX_STAMINA,
    AGILITY,
    SPEED,
}

[Serializable]
public class Stat
{
    [field: SerializeField] 
    public StatId Id { get; protected set; }

    [SerializeField] 
    protected float baseValue;
    protected float currValue;
    protected float lastValue;     // Used to raise OnChange events
    protected int lastFrame = -1;  // Last frame at which this stat was updated
    protected bool dirty = true;   // Recompute currValue if something changes

    public virtual float BaseValue
    {
        get => baseValue; 
	    set { RaiseDirtyFlag(); baseValue = value; }
    }
    public virtual float CurrValue 
    {
        get => currValue; 
	    set { RaiseDirtyFlag(); currValue = value; }
    }

    public Func<float> GetMaxValue; // TODO: Class "Constraints"
    public Func<float> GetMinValue;

    protected ModifiersCollection modifiers = new();
    public Action<StatValueChangeEvent> OnValueChange;
    public readonly StatSettings Settings;
    [HideInInspector] public bool Paused = false;


    public Stat(StatId id, float baseValue, StatSettings settings)
    {
        Id = id;
        BaseValue = CurrValue = lastValue = baseValue;
        Settings = settings;
    }

    public Stat(Stat stat, StatSettings settings)
    {
        // Make a copy
        Id = stat.Id;
        BaseValue = CurrValue = lastValue = stat.BaseValue;
        Settings = settings;
    }

    public void RaiseDirtyFlag() => dirty = true;

    public float GetCurrValue() 
    {
        if (dirty || lastFrame != Time.frameCount)
            UpdateStat();
        return CurrValue;
    }

    public void Reset()
    {
        modifiers.Clear();
        UpdateStat();
        lastValue = CurrValue = BaseValue;
        lastFrame = Time.frameCount;
        dirty = false;
    }

    public Modifier CopyModifier(Modifier template)
    {
        RaiseDirtyFlag();
        return modifiers.Copy(template);
    }

    public void RemoveModifier(Modifier mod)
    {
        dirty = modifiers.Remove(mod);
    }

    public void UpdateStat()
    {
        if (Paused)
            return;

        lastFrame = Time.frameCount;
        dirty |= modifiers.CheckChanges();
        if (!dirty) 
	        return;
        ComputeValue();
    }

    protected void ComputeValue()
    {
        ApplyAllModifiers();
        dirty = false;

        DispatchEvents();
    }

    protected void ApplyAllModifiers()
    { 
        CurrValue = BaseValue;
        var list = modifiers.GetList();
        foreach (Modifier mod in list)
            mod.ApplyModifier(this);

        float min = GetMinValue();
        float max = GetMaxValue();
        CurrValue = Mathf.Clamp(CurrValue, min, max);
    }

    protected void DispatchEvents()
    {
        if (Mathf.Approximately(lastValue, CurrValue)) 
	        return;

        var evt = new StatValueChangeEvent(lastValue, CurrValue, this);
        OnValueChange?.Invoke(evt);
        lastValue = CurrValue;
    }
}

