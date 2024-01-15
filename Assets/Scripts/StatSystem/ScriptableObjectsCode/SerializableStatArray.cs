using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stats", menuName = "ScriptableObjects/Stats/Stats")]
public class SerializableStatArray : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField] List<Stat> stats = new();
    [SerializeField] List<Modifier> modifiers = new();

    public Stat[] Stats { get => stats.ToArray(); }
    public Modifier[] Modifiers { get => modifiers.ToArray(); }

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    { }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (stats.Count == 0)
            return;

        var keys = new List<StatId>();
        foreach (Stat stat in stats)
            keys.Add(stat.Id);

        var duplicates = keys.GroupBy(x => x)
                         .Where(g => g.Count() > 1)
                         .Select(x => new { Element = x.Key, Count = x.Count() })
                         .ToList();

        if (duplicates.Count > 0)
        {
            var str = string.Join(", ", duplicates);
            Debug.LogError($"Warning {GetType().FullName} keys has duplicates {str}");
        }
    }
}
