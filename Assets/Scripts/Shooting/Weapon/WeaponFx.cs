using UnityEngine;

public class WeaponFx : MonoBehaviour
{
    [SerializeField]
    WeaponController Weapon;

    [SerializeField, Tooltip("Prefab of the muzzle flash")]
    GameObject MuzzleFlashPrefab;

    [SerializeField]
    AudioSource ShootSfx;

    Transform muzzle;

    void Start()
    {
        Weapon.OnShoot += OnShoot;
        muzzle = Weapon.WeaponMuzzle;
    }

    void OnShoot()
    {
        if (MuzzleFlashPrefab != null)
        {
            GameObject muzzleFlashInstance = Instantiate(MuzzleFlashPrefab, muzzle.position,
                muzzle.rotation, muzzle.transform);
            muzzleFlashInstance.GetComponent<ParticleSystem>().Play();

            Destroy(muzzleFlashInstance, 2f);
        }


        if (ShootSfx)
        {
            ShootSfx.Play();
        }
    }
}
