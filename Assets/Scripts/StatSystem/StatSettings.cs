using System;
using UnityEngine;

// Stats having the same id shares the same StatSettings instance

[Serializable]
public class StatSettings
{
    [field: SerializeField] public bool IsPersistent { get; private set; }
    [field: SerializeField] public float Min { get; private set; } = 0f;
    [field: SerializeField] public float Max { get; private set; } = 500f;

    [field: Header("Dependencies with other stats")]
    [field: SerializeField] public bool HasUpperBound { get; private set; }
    [field: SerializeField] public StatId UpperBoundId { get; private set; }
}
