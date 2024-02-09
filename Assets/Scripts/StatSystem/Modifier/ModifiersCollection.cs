using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Pool;

public class ModifiersCollection
{
    readonly IObjectPool<Modifier> m_Pool;
    readonly List<Modifier> m_ActiveModifiers = new();
    readonly ReadOnlyCollection<Modifier> m_Modifiers;

    bool m_Sorted = false;

    const int k_DefaultPoolSize = 5;
    const int k_MaxPoolSize = 30;

    public ModifiersCollection()
    {
        m_Pool = new ObjectPool<Modifier>(
	        CreatePooledItem, null, null, null, 
	        true, k_DefaultPoolSize, k_MaxPoolSize
	    );
        m_Modifiers = m_ActiveModifiers.AsReadOnly();
    }

    Modifier CreatePooledItem() => ScriptableObject.CreateInstance<Modifier>();


    public ReadOnlyCollection<Modifier> GetList()
    {
        this.Sort();
        return m_Modifiers;
    }

    public bool CheckChanges()
    {
        bool change = false;

        var toRemove = new List<Modifier>(); 
        foreach (Modifier mod in m_ActiveModifiers)
        {
            mod.UpdateTimeVariables();
            bool isExpired = mod.IsExpired();
            change |= (mod.IsAppliable || isExpired);
            if (isExpired)
                toRemove.Add(mod);
	    }

        foreach(Modifier r in toRemove)
            m_ActiveModifiers.Remove(r);

        return change;
    }

    public Modifier Copy(Modifier template)
    {
        Modifier copy = m_Pool.Get();
        copy.Copy(template);
        m_ActiveModifiers.Add(copy);
        m_Sorted = false;
        return copy;
    }

    public bool Remove(Modifier mod)
    {
        bool removed = m_ActiveModifiers.Remove(mod);
        if (removed) m_Pool.Release(mod);
        return removed;
    }

    public void Clear()
    {
        m_ActiveModifiers.Clear();
        m_Sorted = true;
    }

    void Sort()
    {
        if (m_Sorted) return;
        m_ActiveModifiers.Sort();
        m_Sorted = true;
    }
}
