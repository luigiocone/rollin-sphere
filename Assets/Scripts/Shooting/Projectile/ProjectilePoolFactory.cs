using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePoolFactory
{
    public IObjectPool<Projectile> Pool { get; private set; }
    Projectile projectilePrefab;

    public ProjectilePoolFactory(Projectile projectilePrefab, int defaultPoolSize=10, int maxPoolSize=50)
    {
        this.projectilePrefab = projectilePrefab;

        Pool = new ObjectPool<Projectile>(
            CreatePooledItem, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject,
            true, defaultPoolSize, maxPoolSize
        );
    }

    Projectile CreatePooledItem()
    {
        Projectile p = Object.Instantiate(projectilePrefab);
        p.gameObject.SetActive(false);
        return p;
    }

    void OnTakeFromPool(Projectile p) =>
        p.gameObject.SetActive(true);

    void OnReturnedToPool(Projectile p) =>
        p.gameObject.SetActive(false);

    void OnDestroyPoolObject(Projectile p) =>
        Object.Destroy(p.gameObject);
}
