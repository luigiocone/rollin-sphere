using System;
using UnityEngine;

public abstract class Objective : MonoBehaviour
{
    [Tooltip("Name of the objective that will be shown on screen")]
    public string Title;

    [Tooltip("Short text explaining the objective that will be shown on screen")]
    public string Description;

    public bool IsCompleted { get; private set; }

    public static event Action<Objective> OnObjectiveCreated;
    public static event Action<Objective> OnObjectiveCompleted;

    protected virtual void Start()
    {
        OnObjectiveCreated?.Invoke(this);
    }

    public void UpdateObjective(string description, string counter, string notification)
    {
        ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
        evt.Objective = this;
        evt.DescriptionText = description;
        evt.CounterText = counter;
        evt.NotificationText = notification;
        evt.IsComplete = IsCompleted;
        EventManager.Broadcast(evt);
    }

    public void CompleteObjective(string description, string counter, string notification)
    {
        IsCompleted = true;

        ObjectiveUpdateEvent evt = Events.ObjectiveUpdateEvent;
        evt.Objective = this;
        evt.DescriptionText = description;
        evt.CounterText = counter;
        evt.NotificationText = notification;
        evt.IsComplete = IsCompleted;
        EventManager.Broadcast(evt);

        OnObjectiveCompleted?.Invoke(this);
    }
}