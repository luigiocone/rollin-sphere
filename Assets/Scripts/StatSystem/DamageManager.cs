using System;
using UnityEngine;
using EZCameraShake;

[Serializable]
public class Damage
{
    public float Value;
    public Modifier[] Modifiers;
    [HideInInspector] public GameObject Source { get; set; }

    public Damage(float d, Modifier[] m, GameObject s)
    {
        Value = d; Modifiers = m; Source = s;
    }
}

public class DamageManager
{
    readonly HealthManager m_HealthManager;

    StatsCollectionManager Stats => m_HealthManager.Stats;
    PersistentStat Health => m_HealthManager.Health;
    bool Invincible => m_HealthManager.Invincible;

    public Action<Damage> OnDamage;
    public SphereShield Shield;

    public DamageManager(HealthManager hm)
    {
        m_HealthManager = hm;
    }

    public void TakeDamage(Damage damage)
    {
        if (Invincible) 
	        return;

        Health.CurrValue -= damage.Value;
	    Stats.CopyModifiers(damage.Modifiers);
        OnDamage?.Invoke(damage);
        CameraShaker.Instance.ShakeOnce(1f, 1f, 0.1f, 1f);
    }

    public bool TryProjectileHit(Projectile projectile, RaycastHit hit)
    {
        if (Shield && Shield.IsShieldOn)
        {
            Shield.OnProjectileHit(projectile, hit);
            return false;
        }

        TakeDamage(projectile.Damage);
        return true;
    }

    public void Kill()
    {
        if (Invincible)
	        return;
        Health.Reset();
        Health.CurrValue = 0f;
    }
}

