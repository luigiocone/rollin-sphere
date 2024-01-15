using System.Collections.Generic;
using UnityEngine;

public class StatsCollectionManager : MonoBehaviour
{
    [SerializeField] SerializableStatArray InitialStats;

    protected StatsFactory Factory = new();
    public readonly Dictionary<StatId, StatManager> Managers = new();

    void Awake()
    {
        foreach (var stat in InitialStats.Stats)
        {
            var instance = Factory.GetStatInstance(stat, this);
            var manager = Factory.GetStatManager(instance, this);
            Managers.Add(stat.Id, manager);
        }

        ResolveStatsDependencies();
        CopyModifiers(InitialStats.Modifiers);
        UpdateStats();
    }

    void ResolveStatsDependencies()
    {
        // Copy the collection to iterate on and to modify the original version
        StatManager[] collection = new StatManager[Managers.Count];
        Managers.Values.CopyTo(collection, 0);

        foreach (var manager in collection)
            manager.CheckDependencies();
    }

    public void UpdateStats()
    {
        foreach (var manager in Managers.Values)
            manager.Stat.UpdateStat();
    }

    public Modifier CopyModifier(Modifier template)
    {
        // Return the copy
        if (Managers.ContainsKey(template.StatId))
            return Managers[template.StatId].Stat.CopyModifier(template);
        return null;
    }

    public Modifier[] CopyModifiers(Modifier[] templates)
    {
        Modifier[] copies = new Modifier[templates.Length];
        for (int i = 0; i < templates.Length; i++)
            copies[i] = CopyModifier(templates[i]);
        return copies;
    }

    public void RemoveModifier(Modifier mod)
    {
        if (Managers.ContainsKey(mod.StatId))
            Managers[mod.StatId].Stat.RemoveModifier(mod);
    }

    public bool TryAddStat(StatId id, float baseValue)
    {
        if (Managers.ContainsKey(id))
            return false;
        var instance = Factory.GetStatInstance(id, baseValue, this);
        var manager = Factory.GetStatManager(instance, this);
        Managers.Add(id, manager);
        manager.CheckDependencies();
        return true;
    }

    public bool TryAddStat(Stat stat) =>
        this.TryAddStat(stat.Id, stat.BaseValue);

    public Stat GetStat(StatId id)
    {
        if (!Managers.ContainsKey(id)) 
	        return null;
        return Managers[id].Stat;
    }

    public StatManager GetStatManager(StatId id) => 
	    Managers[id];

    public T GetStatManager<T>() where T : StatManager
    {
        foreach (var manager in Managers.Values)
        {
            if (manager is T)
                return manager as T;
	    }
        return null;
    }
}

