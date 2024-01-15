using System.Collections.Generic;
using UnityEngine;

public class ObjectivePickupItem : Objective
{
    [SerializeField] List<Pickup> Items;

    string CounterText => m_Collected + " / " + (m_Collected + Items.Count);
    int m_Collected;

    protected override void Start()
    {
        base.Start();
        EventManager.AddListener<PickupEvent>(OnPickupEvent);
    }

    void OnDestroy()
    {
        EventManager.RemoveListener<PickupEvent>(OnPickupEvent);
    }

    void OnPickupEvent(PickupEvent evt)
    {
        if (IsCompleted)
            return;

        if (!Items.Contains(evt.Pickup))
            return;

        m_Collected++;
        Items.Remove(evt.Pickup);
        if (Items.Count > 0)
        {
            UpdateObjective(string.Empty, CounterText, string.Empty);
            return;
	    }

        CompleteObjective(string.Empty, CounterText, "Objective completed: <color=#00FFC2>" + Title + "</color>");
    }
}

