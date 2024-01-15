using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

public abstract class Projectile : MonoBehaviour
{
    [field: SerializeField] 
    public Damage Damage { get; private set; }
    public GameObject Owner { get; private set; }
    public Vector3 InitialPosition { get; private set; }
    public Vector3 InitialDirection { get; private set; }
    public Vector3 InheritedMuzzleVelocity { get; private set; }
    public UnityAction<RaycastHit> onImpact;
    public UnityAction onShoot;

    protected IObjectPool<Projectile> pool;

    public void Shoot(GameObject owner, Vector3 position, Vector3 direction, Vector3 inheritedVelocity)
    {
        Owner = owner;
        InitialPosition = position;
        InitialDirection = direction;
        InheritedMuzzleVelocity = inheritedVelocity;
        this.Damage.Source = owner;

        onShoot?.Invoke();
    }

    public void Shoot(WeaponController weapon)
    {
        Owner = weapon.Owner;
        InitialPosition = transform.position;
        InitialDirection = transform.forward;
        InheritedMuzzleVelocity = weapon.MuzzleWorldVelocity;
        pool = weapon.pool;
        this.Damage.Source = Owner;

        onShoot?.Invoke();
    }

    public void Release()
	{
        if (pool == null) Destroy(this);
        else pool.Release(this);
    }
}
