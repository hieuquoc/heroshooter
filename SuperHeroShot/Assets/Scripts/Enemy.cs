using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PooledObject
{
    public int MaxHealth = 1;
    public int Health = 1;

    [Header("Weapon")]
    public WeaponData weaponData;
    public LayerMask playerMask = ~0;   // layer của player để raycast LOS

    [Header("Aim")]
    [SerializeField] private Transform aimPoint;   // constraint source — trỏ về phía player
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float yAngleOffset = 0f; // offset góc Y so với hướng player

    private float rayHitTime;
    private float rayHitInterval = 0.5f;

    Transform _playerTransform;

    void Start()
    {
        if (GameManager.Player != null)
            _playerTransform = GameManager.Player.transform;
    }

    void Update()
    {
        RotateTowardPlayer();
        TryShoot();
    }

    void TryShoot()
    {
        if (weaponData == null || _playerTransform == null) return;

        weaponData.UpdateFireTime();
        weaponData.UpdateCooldown();

        if (!weaponData.CanShoot()) return;

        // Xác định điểm bắn
        Transform shootPoint = (weaponData.shootingPoints != null && weaponData.shootingPoints.Length > 0)
            ? weaponData.shootingPoints[0]
            : (aimPoint != null ? aimPoint : transform);

        // Raycast LOS từ shootPoint đến player
        Vector3 origin = shootPoint.position;
        Vector3 dir    = _playerTransform.position - origin;
        float   dist   = dir.magnitude;

        if (!Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, playerMask, QueryTriggerInteraction.Ignore))
            return;   // bị chặn → skip interval

        if (!hit.collider.CompareTag("Player"))
            return;   // raycast trúng vật khác trước player → skip

        // Bắn
        weaponData.currentAimDirection = dir.normalized;
        Quaternion shootRot = Quaternion.LookRotation(weaponData.currentAimDirection);
        Bullet bullet = ObjectPool.Instance
            .Get(weaponData.bulletPrefab, origin, shootRot)
            .GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Damage    = weaponData.damage;
            bullet.Direction = weaponData.currentAimDirection;
        }

        weaponData.fireTimer     = weaponData.fireInterval;
        weaponData.cooldownTimer = weaponData.cooldown;
    }

    // Xoay trục Y của enemy về hướng player, giữ Y level
    void RotateTowardPlayer()
    {
        if (_playerTransform == null) return;

        Vector3 dir = _playerTransform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, yAngleOffset, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotateSpeed * Time.deltaTime);

        // Cập nhật aimPoint luôn nhìn thẳng về vị trí player (bao gồm Y — để aim không bị flat)
        if (aimPoint != null)
        {
            Vector3 aimDir = _playerTransform.position - aimPoint.position;
            if (aimDir.sqrMagnitude > 0.0001f)
                aimPoint.rotation = Quaternion.LookRotation(aimDir);
        }
    }

    public void Initialize(int health)
    {
        MaxHealth = health;
        Health = health;
    }

    public void TakeDamage(int damage, WeaponType weaponType)
    {
        if(weaponType == WeaponType.Ray)
        {
            if(Time.time - rayHitTime < rayHitInterval)
            {
                return; // Ignore damage if hit too frequently
            }
            rayHitTime = Time.time;
        }
        Health -= damage;
        if (Health <= 0)
        {
            Die();
        }
        else
        {
            // Handle taking damage, e.g., play hit animation, etc.
            Debug.Log($"Enemy took {damage} damage! Remaining health: {Health}");
        }
    }

    private void Die()
    {
        // Handle enemy death, e.g., play death animation, disable enemy, etc.
        Debug.Log("Enemy died!");
        ObjectPool.Instance.Return(gameObject);
    }
}
