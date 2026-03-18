using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rescueforce
{
    public class Weapon : MonoBehaviour
{
    public WeaponData handShoot;
    public WeaponData rayShoot;
    public WeaponData rocketAim;

    public WeaponType currentWeaponType = WeaponType.Pistol;

    [SerializeField] private Launcher launcher;

    void Start()
    {
        rayShoot.bulletPrefab.SetActive(false);
        launcher.SetUp(rocketAim);
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
        if(GameManager.Player.CurrentState == PlayerState.LaserShoot)
        {
            if(rayShoot.fireTimer <= 0f)
            {
                rayShoot.bulletPrefab.SetActive(false);
                GameManager.Player.ChangeState(PlayerState.Idle);
            }else
            
            UpdateRayAim();
            
        }
    }

    public void ShootHand()
    {
        Quaternion shootRotation = Quaternion.LookRotation(GameManager.Camera.transform.forward);
        Bullet bullet = ObjectPool.Instance.Get(handShoot.bulletPrefab, handShoot.shootingPoints[0].transform.position, shootRotation).GetComponent<Bullet>();
        bullet.Damage = handShoot.damage;
        bullet.Direction = handShoot.currentAimDirection;
        handShoot.fireTimer = handShoot.fireInterval;
        bullet.Shoot(handShoot.currentAimDirection, handShoot.damage, handShoot.weaponType);
        GameManager.Player.AnimatorManager.ShootAnimation();
        CrossHair.NotifyPistolShot();
        AudioManager.Instance.PlayShootSfx();
    }

    public void ShootRay()
    {
        rayShoot.bulletPrefab.SetActive(true);
        rayShoot.fireTimer = rayShoot.fireInterval;
        rayShoot.cooldownTimer = rayShoot.cooldown;
        AudioManager.Instance.PlayLaserSfx();
        
    }

    public void UpdateRayAim()
    {
        Vector3 cameraForward = GameManager.Camera.transform.forward;
        if(Physics.Raycast(GameManager.Camera.transform.position, cameraForward, out RaycastHit hit, rayShoot.range))
        {
            rayShoot.currentAimDirection = (hit.point - rayShoot.shootingPoints[0].position).normalized;
            Debug.DrawRay(rayShoot.shootingPoints[0].position, rayShoot.currentAimDirection * rayShoot.range, Color.red);
        }else
        {
            rayShoot.currentAimDirection = cameraForward;
            Debug.DrawRay(rayShoot.shootingPoints[0].position, rayShoot.currentAimDirection * rayShoot.range, Color.blue);
        }
        rayShoot.bulletPrefab.transform.rotation = Quaternion.LookRotation(rayShoot.currentAimDirection);
        CrossHair.NotifyLaserFiring();
    }

    public void AimRocket()
    {
        launcher.StartAiming();
        CrossHair.NotifyRocketFired();
    }

    public bool CheckTargetHandShoot()
    {
        if(handShoot.CanShoot() == false) return false;
        if(Physics.Raycast(GameManager.Camera.transform.position, GameManager.Camera.transform.forward, out RaycastHit hit, handShoot.range))
        {
            // Check if the hit object is an enemy or a valid target
            if (hit.collider.CompareTag("enemy"))
            {
                handShoot.currentAimDirection = (hit.point - handShoot.shootingPoints[0].position).normalized;
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

    public float LaserCooldown()
    {
        return rayShoot.cooldown;
    }

    public float RocketDuration()
    {
        return rocketAim.cooldown + rocketAim.fireInterval * 3 + 2;
    }

    public void Reset()
    {
        rocketAim.cooldownTimer = 0f;
        rayShoot.cooldownTimer = 0f;
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
    public float fireInterval;
    public float fireTimer;
    public float cooldown;
    public float cooldownTimer;
    public Vector3 currentAimDirection;

    public void UpdateCooldown()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void UpdateFireTime()
    {
        if (fireTimer > 0f)
            fireTimer -= Time.deltaTime;
    }

    public bool CanShoot()
    {
        if(PlayerData.IsFreeFire && fireTimer <= 0f) return true;
        return cooldownTimer <= 0f && fireTimer <= 0f;
    }
}

public enum WeaponType
{
    Pistol,
    Ray,
    RocketAim,
}

}

