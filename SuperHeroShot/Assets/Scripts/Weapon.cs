using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData handShoot;
    public WeaponData rayShoot;
    public WeaponData rocketAim;

    public WeaponType currentWeaponType;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GameManager.Player.CurrentState == PlayerState.Idle || GameManager.Player.CurrentState == PlayerState.Move)
        {
            if(currentWeaponType == WeaponType.Pistol && CheckTargetHandShoot())
            {
                ShootHand();
            }
        }

        UpdateGun();
    }

    public void ShootHand()
    {
        Quaternion shootRotation = Quaternion.LookRotation(GameManager.Camera.transform.forward);
        Bullet bullet = ObjectPool.Instance.Get(handShoot.bulletPrefab, handShoot.shootingPoints[0].transform.position, shootRotation).GetComponent<Bullet>();
        bullet.Damage = handShoot.damage;
        bullet.Direction = GameManager.Camera.transform.forward;
    }

    public void ShootRay()
    {
        
    }

    public void AimRocket()
    {
        
    }

    public void ShootSpecialWeapon(WeaponType weaponType)
    {
        if(currentWeaponType != WeaponType.Pistol) return;
        if(weaponType == WeaponType.Ray)
        {
            ShootRay();
        }
        else if(weaponType == WeaponType.RocketAim)
        {
            AimRocket();
        }
    }

    public bool CheckTargetHandShoot()
    {
        if(Physics.Raycast(GameManager.Camera.transform.position, GameManager.Camera.transform.forward, out RaycastHit hit, handShoot.range))
        {
            // Check if the hit object is an enemy or a valid target
            if (hit.collider.CompareTag("Enemy"))
            {
                ShootHand();
                return true;
            }
        }
        return false;
    }

    public void UpdateGun()
    {
        handShoot.UpdateCooldown();
        handShoot.UpdateFireTime();
        rayShoot.UpdateCooldown();
        rayShoot.UpdateFireTime();
        rocketAim.UpdateCooldown();
        rocketAim.UpdateFireTime();
    }
}

[System.Serializable]
public class WeaponData
{
    public GameObject bulletPrefab;
    public WeaponType weaponType;
    public Transform[] shootingPoints;
    public int damage;
    public float range;
    public float fireCooldown;
    public float fireTime;
    public float cooldown;
    public float cooldownTimer;

    public void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void UpdateFireTime()
    {
        if (fireTime > 0f)
            fireTime -= Time.deltaTime;
    }

    public bool CanShoot()
    {
        return cooldownTimer <= 0f && fireTime <= 0f;
    }
}

public enum WeaponType
{
    Pistol,
    Ray,
    RocketAim,
}
