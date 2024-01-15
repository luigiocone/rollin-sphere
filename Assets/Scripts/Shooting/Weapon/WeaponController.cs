using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public class WeaponController : MonoBehaviour
{
    [field: SerializeField, Tooltip("The root object for the weapon")]
    public GameObject WeaponRoot { get; private set; }

    [field: SerializeField, Tooltip("Tip of the weapon, where the projectiles are shot")]
    public Transform WeaponMuzzle { get; private set; }

    [SerializeField, Tooltip("The projectile prefab")] 
    Projectile ProjectilePrefab;

    [SerializeField, Min(0.05f)]
    float DelayBetweenShots = 0.5f;

    [SerializeField, Range(0f, 5f), Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
    float BulletSpreadAngle = 1f;

    [SerializeField, Range(1, 5)]
    int BulletsPerShot = 1;
    
    [SerializeField, Min(0f), Tooltip("Attack interval duration before cooldown")]
    float AttackDuration = 2f;

    [SerializeField, Min(0f), Tooltip("Rest time before attacking again")]
    float CooldownDuration = 0f;

    [Header("Ammo Parameters")]
    [SerializeField, Tooltip("Bullet shell casing")]
    GameObject ShellCasing;

    [SerializeField, Tooltip("Weapon Ejection Port for shell casing")]
    Transform EjectionPort;

    [SerializeField, Range(0.0f, 5.0f), Tooltip("Force applied on the shell on ejection")]
    float ShellCasingEjectionForce = 2.0f;

    [SerializeField, Range(0, 30), Tooltip("Maximum number of shell that can be spawned before reuse")]
    int ShellPoolSize = 5;


    public UnityAction OnShoot;

    public GameObject Owner { get; set; }
    public GameObject SourcePrefab { get; set; }
    public Vector3 MuzzleWorldVelocity { get; private set; }
    public bool IsReloading { get; private set; }
    public bool HasShells => ShellCasing && ShellPoolSize > 0;

    float m_LastTimeShot = Mathf.NegativeInfinity;
    bool m_InBurst, m_InCooldown;
    Vector3 m_LastMuzzlePosition;
    private Queue<Rigidbody> m_ShellPool;
    public IObjectPool<Projectile> pool { get; private set; }


    void Awake()
    {
        Owner = this.gameObject;
        m_LastMuzzlePosition = WeaponMuzzle.position;
        pool = new ProjectilePoolFactory(ProjectilePrefab).Pool;

        if (HasShells)
        {
            m_ShellPool = new Queue<Rigidbody>(ShellPoolSize);
            for (int i = 0; i < ShellPoolSize; i++)
            {
                GameObject shell = Instantiate(ShellCasing, transform);
                shell.SetActive(false);
                m_ShellPool.Enqueue(shell.GetComponent<Rigidbody>());
            }
        }
    }

    void Update()
    {
        if (Time.deltaTime > 0)
        {
            MuzzleWorldVelocity = (WeaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = WeaponMuzzle.position;
        }
    }

    public bool TryShoot()
    {
        bool canShoot = !m_InCooldown && m_LastTimeShot + DelayBetweenShots < Time.time;
        if (!canShoot) 
	        return false;

        if (CooldownDuration > 0f && !m_InBurst)
            StartCoroutine(StartBurst());

        HandleShoot();
        return true;
    }

    IEnumerator StartBurst()
    {
        m_InBurst = true;
        var start = Time.time;
        while (start + AttackDuration > Time.time)
            yield return null;

        m_InCooldown = true;
        start = Time.time;
        while (start + CooldownDuration > Time.time)
            yield return null;

        m_InCooldown = false;
        m_InBurst = false;
    }

    void HandleShoot()
    {
        // Spawn all bullets with random direction
        for (int i = 0; i < BulletsPerShot; i++)
        {
            Projectile p = pool.Get();

            Vector3 shotDirection = AddRandomSpread(WeaponMuzzle);
            Quaternion projectileRotation = Quaternion.LookRotation(shotDirection);
            p.transform.SetPositionAndRotation(
		        WeaponMuzzle.position, projectileRotation
		    );
            p.Shoot(this);
        }

        if (HasShells)
            ShootShell();

        m_LastTimeShot = Time.time;
        OnShoot?.Invoke();
    }

    public Vector3 AddRandomSpread(Transform shootTransform)
    {
        float angle = UnityEngine.Random.Range(-BulletSpreadAngle, BulletSpreadAngle);
        var quaternion = Quaternion.Euler(angle, angle, angle);
        var newDirection = quaternion * shootTransform.forward;
        return newDirection;
    }

    void ShootShell()
    {
        Rigidbody nextShell = m_ShellPool.Dequeue();

        nextShell.transform.SetPositionAndRotation(
	        EjectionPort.transform.position,
            EjectionPort.transform.rotation
	    );
        nextShell.gameObject.SetActive(true);
        nextShell.transform.SetParent(null);
        nextShell.AddForce(nextShell.transform.up * ShellCasingEjectionForce, ForceMode.Impulse);

        m_ShellPool.Enqueue(nextShell);
    }
}

