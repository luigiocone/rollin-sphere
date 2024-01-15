using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public List<EnemyController> Enemies { get; private set; }
    public int TotalNumberOfEnemies { get; private set; }
    public int RemainingNumberOfEnemies => Enemies.Count;

    void Awake()
    {
        Enemies = new List<EnemyController>();
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        Enemies.Add(enemy);
        TotalNumberOfEnemies++;
    }

    public void UnregisterEnemy(EnemyController enemyKilled)
    {
        int remaining = RemainingNumberOfEnemies - 1;

        // Build and broadcast an event
        EnemyKillEvent evt = Events.EnemyKillEvent;
        evt.Enemy = enemyKilled.gameObject;
        evt.RemainingEnemyCount = remaining;
        EventManager.Broadcast(evt);

        Enemies.Remove(enemyKilled);
    }
}
