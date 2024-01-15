using UnityEngine;

// The Game Events used across the Game.
// Anytime there is a need for a new event, it should be added here.

public static class Events
{
    public static ObjectiveUpdateEvent ObjectiveUpdateEvent = new();
    public static AllObjectivesCompletedEvent AllObjectivesCompletedEvent = new();
    public static GameOverEvent GameOverEvent = new();
    public static PlayerDeathEvent PlayerDeathEvent = new();
    public static EnemyKillEvent EnemyKillEvent = new();
    public static PickupEvent PickupEvent = new();
    public static DisplayToastEvent DisplayToastEvent = new();
    public static ChangeSceneEvent ChangeSceneEvent = new();
    public static StatValueChangeEvent StatValueChangeEvent = new();
}

public class ObjectiveUpdateEvent : GameEvent
{
    public Objective Objective;
    public string DescriptionText;
    public string CounterText;
    public bool IsComplete;
    public string NotificationText;
}

public class AllObjectivesCompletedEvent : GameEvent { }

public class GameOverEvent : GameEvent
{
    public bool Win;
}

public class PlayerDeathEvent : GameEvent { }

public class EnemyKillEvent : GameEvent
{
    public GameObject Enemy;
    public int RemainingEnemyCount;
}

public class PickupEvent : GameEvent
{
    public Pickup Pickup;
}

public class DisplayToastEvent : GameEvent
{
    public Toast toast;
}

public class ChangeSceneEvent : GameEvent 
{
    public string sceneName;
}

public class StatValueChangeEvent
{
    public Stat stat;
    public float prev, curr;

    public StatValueChangeEvent() { }

    public StatValueChangeEvent(float p, float c, Stat s)
    {
        prev = p; curr = c; stat = s;
    }
}

