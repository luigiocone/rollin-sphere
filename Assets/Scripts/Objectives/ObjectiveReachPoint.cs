using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ObjectiveReachPoint : Objective
{
    void OnTriggerEnter(Collider other)
    {
        if (IsCompleted)
            return;

        var player = other.GetComponent<PlayerEvents>();
        if (player != null)
            CompleteObjective(string.Empty, string.Empty, "Objective completed: <color=#00FFC2>" + Title + "</color>");
    }
}
