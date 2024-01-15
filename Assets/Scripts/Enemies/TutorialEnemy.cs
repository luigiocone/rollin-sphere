using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    WeaponController m_Weapon;

    void Start() => m_Weapon = GetComponentInChildren<WeaponController>();
    void Update() => m_Weapon.TryShoot();
}
