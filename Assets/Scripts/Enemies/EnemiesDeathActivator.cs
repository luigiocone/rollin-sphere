using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemiesDeathActivator : MonoBehaviour
{
    [SerializeField] List<GameObject> Enemies;
    [SerializeField] UnityEvent OnAllEnemiesDead = default;

    void Start()
    {
        EventManager.AddListener<EnemyKillEvent>(OnEnemyDeath);
    }

    void OnEnemyDeath(EnemyKillEvent evt)
    {
        var enemy = evt.Enemy;
        if (!Enemies.Contains(enemy))
            return;

        Enemies.Remove(enemy);
        if (Enemies.Count != 0)
            return;

        OnAllEnemiesDead?.Invoke();

        EventManager.RemoveListener<EnemyKillEvent>(OnEnemyDeath);
        this.enabled = false;
    }
}

