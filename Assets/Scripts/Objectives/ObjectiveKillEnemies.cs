using System.Collections.Generic;
using UnityEngine;

public class ObjectiveKillEnemies : Objective
{
    [SerializeField] List<GameObject> Enemies;

    string CounterText => m_Kills + " / " + (m_Kills + Enemies.Count);
    int m_Kills;

    protected override void Start()
    {
        if (Enemies.Count == 0)
        {
            this.enabled = false;
            return;
	    }

        base.Start();
        EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);
    }
    
    void OnDestroy()
    { 
        EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
    }

    void OnEnemyKilled(EnemyKillEvent evt)
    {
        if (IsCompleted || !Enemies.Remove(evt.Enemy)) 
	        return;

        m_Kills++;

        // Update the objective
        if (Enemies.Count > 0)
        {
            UpdateObjective(string.Empty, CounterText, string.Empty);
            return;
        }
        CompleteObjective(string.Empty, CounterText, "Objective completed: <color=#00FFC2>" + Title + "</color>");
    }
}

