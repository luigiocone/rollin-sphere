using UnityEngine;

public class ProjectileFx : MonoBehaviour
{
    [SerializeField]
    Projectile projectile;

    [Header("VFX")]
    [SerializeField] GameObject ImpactVfx;

    [SerializeField, Range(-1f, 1f), Tooltip("Offset along the hit normal where the VFX will be spawned")]
    float ImpactVfxSpawnOffset = 0.1f;

    [Header("Impact SFX")]
    [SerializeField] AudioSource ImpactSfx;
    [SerializeField] AudioSource ShootSfx;

    ParticleSystem particles;

    void Awake()
    {
        GameObject go = Instantiate(ImpactVfx);
        go.SetActive(false);
        particles = go.GetComponent<ParticleSystem>();

        projectile.onImpact += PlayImpactFX;
        projectile.onShoot += OnShoot;
    }

    void PlayImpactFX(RaycastHit hit)
    {
        if (!ImpactVfx) return;

        Vector3 point = hit.point;
        Vector3 normal = hit.normal;
        particles.gameObject.SetActive(true);
        particles.gameObject.transform.SetPositionAndRotation(
            point + (normal * ImpactVfxSpawnOffset),
            Quaternion.LookRotation(normal)
        );
        particles.Play();

        if (ImpactSfx)
        {
            ImpactSfx.Play();
        }
    }

    void OnShoot()
    { 
        if (ShootSfx)
        {
            ShootSfx.Play();
        }
    }
}
