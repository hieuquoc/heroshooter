using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace rescueforce
{
    public class Enemy : PooledObject, IDamageable
{
    private Health _health;
    private int _lastDamageTaken = 0;

    [Header("Weapon")]
    public WeaponData weaponData;
    public LayerMask playerMask = ~0;   // layer của player để raycast LOS

    [Header("Aim")]
    [SerializeField] private Transform aimPoint;   // constraint source — trỏ về phía player
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float yAngleOffset = 0f; // offset góc Y so với hướng player
    [SerializeField] private Animator animator; // để tắt khi ragdoll, tránh animation override
    [SerializeField] private AimConstraint aimConstraint; 
    [SerializeField] private Collider mainCollider; // collider chính của enemy, để tắt khi ragdoll
    public Transform markPoint; // điểm để hiển thị marker trên UI

    private float rayHitTime;
    private float rayHitInterval = 0.05f;
    private Vector3 hitDirection;   // hướng từ player đến enemy, dùng để apply force khi chết

    Transform _playerTransform;

    void Start()
    {
        if (GameManager.Player != null)
            _playerTransform = GameManager.Player.CenterPoint;
    }

    void OnEnable()
    {
        SetRagdoll(false);
        if (_health == null) _health = GetComponent<Health>();
        if (_health != null)
        {
            _health.onDamagedWithType += OnDamagedWithType;
            _health.onDeath.AddListener(OnHealthDeath);
        }
    }

    void OnDisable()
    {
        if (_health != null)
        {
            _health.onDamagedWithType -= OnDamagedWithType;
            _health.onDeath.RemoveListener(OnHealthDeath);
        }
    }

    void Update()
    {
        if(_health.GetCurrentHealth() <= 0)
        {
             return; // dead enemy does not rotate or shoot
        }
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
        Vector3 dir = _playerTransform.position - origin;
        float dist = dir.magnitude;

        Debug.DrawRay(origin, dir.normalized * dist, Color.red, 0.1f);

        if (!Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, playerMask, QueryTriggerInteraction.Ignore))
            {
                return;
            }   // bị chặn → skip interval

        if (!hit.collider.transform.root.CompareTag("Player"))
            {
                return;
            }   // raycast trúng vật khác trước player → skip

        // Bắn
        weaponData.currentAimDirection = dir.normalized;
        Quaternion shootRot = Quaternion.LookRotation(weaponData.currentAimDirection);
        Bullet bullet = ObjectPool.Instance
            .Get(weaponData.bulletPrefab, origin, shootRot)
            .GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Shoot(weaponData.currentAimDirection, weaponData.damage, weaponData.weaponType);
        }

        weaponData.fireTimer = weaponData.fireInterval;
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
            aimPoint.transform.position = _playerTransform.position;
        }
    }

    public void Initialize(int health)
    {
        if (_health == null) _health = GetComponent<Health>();
        if (_health != null)
            _health.SetMaxHealth(health);
    }

    // IDamageable implementation — handle weapon-specific rules, then forward to Health
    public void TakeDamage(float damage, WeaponType weaponType = WeaponType.Pistol)
    {
        hitDirection = (transform.position - GameManager.Player.transform.position).normalized;
        if (weaponType == WeaponType.Ray)
        {
            if (Time.time - rayHitTime < rayHitInterval)
            {
                return; // Ignore damage if hit too frequently
            }
            rayHitTime = Time.time;
        }

        _lastDamageTaken = Mathf.CeilToInt(damage);
        if (_health == null) _health = GetComponent<Health>();
        if (_health != null)
        {
            _health.TakeDamage(damage, weaponType);
        }
    }

    private void OnDamagedWithType(float amount, WeaponType type)
    {
        // Could trigger hit animations, particles, etc.
        Debug.Log($"Enemy took {amount} damage from {type}. Remaining: {_health?.GetCurrentHealth()}");
    }

    private void OnHealthDeath()
    {
        Die(_lastDamageTaken);
    }

    [Header("Ragdoll Force")]
    [SerializeField] private float ragdollHorizontalForce = 120f;   // lực đẩy ngang base
    [SerializeField] private float ragdollUpForce         = 40f;    // lực đẩy lên base
    [SerializeField] private float ragdollDamageScale     = 8f;     // nhân thêm theo damage (up = horizontal * scale, up = upForce * scale * 0.5)
    [SerializeField] private float ragdollMaxForce        = 600f;   // giới hạn trên để không bắn bay quá xa
    [SerializeField] private float ragdollReturnDelay     = 3f;

    private void Die(int damage)
    {
        // notify LevelManager to unregister this enemy (remove marker and stop tracking)
        if (LevelManager.Instance != null)
            LevelManager.Instance.UnregisterSpawnedEnemy(gameObject);

        Debug.Log("Enemy died!");
        SetRagdoll(true);

        // Scale lực theo damage, nhưng cap lại để không bay quá
        float scale = 1f + damage * ragdollDamageScale * 0.01f;
        float horizontal = Mathf.Min(ragdollHorizontalForce * scale, ragdollMaxForce);
        float upward     = Mathf.Min(ragdollUpForce * scale, ragdollMaxForce * 0.5f);

        Vector3 horizontalDir = new Vector3(hitDirection.x, 0f, hitDirection.z).normalized;
        Vector3 forceVec = horizontalDir * horizontal + Vector3.up * upward;

        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.AddForce(forceVec, ForceMode.Impulse);

        ObjectPool.Instance.ReturnDelayed(gameObject, ragdollReturnDelay);
        if(LevelManager.Instance.GetCurrentEnemyCount() <= 0)
        {
            GameManager.Instance.WinGame();
        }
    }

    void SetRagdoll(bool enabled)
    {
        foreach (var rb in GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = !enabled;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = enabled;

        animator.enabled = !enabled;
        // Tắt luôn main collider của enemy nếu có riêng
        aimConstraint.enabled = !enabled;
        mainCollider.enabled = !enabled;
    }
}

}

