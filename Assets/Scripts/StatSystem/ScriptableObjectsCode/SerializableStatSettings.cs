using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New StatsSettings", menuName = "ScriptableObjects/Stats/StatsSettings")]
public class SerializableStatSettings : SerializableDictionary<StatId, StatSettings>
{
    [field: SerializeField]
    public StatSettings DefaultSettings { get; private set; }
}

public class SerializableDictionary<TKey, TValue> : ScriptableObject, ISerializationCallbackReceiver
{
    [Serializable]
    class KeyValueEntry
    {
        public TKey key;
        public TValue value;
    }

    [SerializeField]
    List<KeyValueEntry> entries;

    List<TKey> keys = new();
    public Dictionary<TKey, TValue> dictionary = new();

    void ISerializationCallbackReceiver.OnAfterDeserialize()
    {
        dictionary.Clear();
        foreach (KeyValueEntry entry in entries)
            dictionary.Add(entry.key, entry.value);
    }

    void ISerializationCallbackReceiver.OnBeforeSerialize()
    {
        if (entries == null)
            return;

        keys.Clear();
        foreach (KeyValueEntry entry in entries)
            keys.Add(entry.key);

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
