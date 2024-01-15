using System.Collections.Generic;
using UnityEngine;

public class ProjectileStandard : Projectile
{
    [Header("General")]

    [SerializeField, Tooltip("Radius of this projectile's collision detection")]
    float Radius = 0.01f;

    [SerializeField, Tooltip("Projectile's core positions (used for accurate collision detection)")]
    Transform Tail, Tip, Root;

    [SerializeField, Range(3f, 10f), Tooltip("Life time of the projectile")]
    float MaxLifeTime = 5f;

    [SerializeField, Tooltip("What this projectile can collide with")]
    LayerMask WhatIsHittable = -1;

    [Header("Movement")]
    [SerializeField, Tooltip("Initial speed of the projectile after shooting it")]
    float DefaultSpeed = 20f;

    [Header("Debug")]
    [Tooltip("Color of the projectile radius debug view")]
    public Color RadiusColor = Color.cyan * 0.2f;

    public Vector3 Velocity;
    public float Length { get; private set; }

    Vector3 m_LastRootPosition;
    float m_ShootTime;
    List<Collider> m_IgnoredColliders = new();
    
    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    void OnValidate()
    {
        if (Root && Tail)
            Length = (Root.position - Tail.position).magnitude;
    }

    void OnEnable()
    {
        this.onShoot += InitShoot;
    }

    void InitShoot()
    {
        m_ShootTime = Time.time;

        float inheritedVelocity = Vector3.Dot(InitialDirection, InheritedMuzzleVelocity);
        Velocity = InitialDirection * (DefaultSpeed + inheritedVelocity);
        transform.position = InitialPosition;
        m_LastRootPosition = Root.position;

        // Ignore colliders of owner
        m_IgnoredColliders.Clear();
        Collider[] ownerColliders = this.Owner.GetComponentsInChildren<Collider>();
        m_IgnoredColliders.AddRange(ownerColliders);
    }

    void Update()
    {
        if (m_ShootTime - Time.time > MaxLifeTime)
        {
            this.Release();
            return;
        }

        // Compute projectile movement and orientation
        transform.position += Velocity * Time.deltaTime;  // Projectile movement
        transform.forward = Velocity.normalized;          // Orient towards velocity

        bool foundHit = GetClosestHit(out RaycastHit closestHit);
        if (foundHit)
        {
            // Handle case of casting while already inside a collider
            if (closestHit.distance <= 0f)
            {
                // Project point on sphere
                Vector3 direction = (Root.position - closestHit.collider.bounds.center).normalized;
                Vector3 center = closestHit.collider.bounds.center;
                float radius = closestHit.collider.bounds.extents.x;
                closestHit.point = direction * radius + center;
                closestHit.normal = direction;
            }

            OnImpact(closestHit);
        }

        m_LastRootPosition = Root.position;
    }

    bool GetClosestHit(out RaycastHit closestHit)
    { 
        closestHit = new();
        closestHit.distance = Mathf.Infinity;
        bool foundHit = false;

        // Retrieve all the hits in the distance travelled on current and last frame
        Vector3 displacement = Tip.position - m_LastRootPosition;
        RaycastHit[] hits = Physics.SphereCastAll(
            m_LastRootPosition, Radius,
            displacement.normalized, displacement.magnitude,
            WhatIsHittable, k_TriggerInteraction
        );

        foreach (var hit in hits)
        {
            if (IsHitValid(hit) && hit.distance < closestHit.distance)
            {
                foundHit = true;
                closestHit = hit;
            }
        }
        return foundHit;
    }

    bool IsHitValid(RaycastHit hit)
    {
        // ignore hits with triggers 
        if (hit.collider.isTrigger)
            return false;

        // ignore hits with some colliders specified on runtime 
        if (m_IgnoredColliders != null && m_IgnoredColliders.Contains(hit.collider))
            return false;

        return true;
    }

    void OnImpact(RaycastHit hit)
    {
        onImpact?.Invoke(hit);

        var stats = hit.collider.gameObject.GetComponentInParent<StatsCollectionManager>();
        DamageManager dm = null;
        if (stats != null)
        { 
            var hm = stats.GetStatManager<HealthManager>();
            dm = hm.DamageManager;
	    }

        // Projectile is still alive if damage manager does not confirm hit
        bool reflected = dm != null && !dm.TryProjectileHit(this, hit);
        if (!reflected) 
	        Release();
    }


    void OnDrawGizmos()
    {
        Gizmos.color = RadiusColor;
        Gizmos.DrawSphere(m_LastRootPosition, Radius);
    }
}

