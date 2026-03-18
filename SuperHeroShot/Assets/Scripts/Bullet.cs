using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledObject
{
    public int Damage { get; set; }
    public Vector3 Direction { get; set; }
    public float Speed = 20f;
    public WeaponType WeaponType { get; set; }

    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private Vector3 _target;
    [SerializeField] private float _bezierControlXZDistance = 5f;
    [SerializeField] private float _bezierControlHeight = 5f;
    [SerializeField] private float _explosionRadius = 3f;

    [SerializeField] private float _lifeTimer;
    [SerializeField] private LayerMask _enemyLayerMask;
    private Vector3 _spawnPosition;
    private Vector3 _bezierControlPoint;
    private float _bezierT;

    public void Shoot(Vector3 direction, int damage, WeaponType weaponType)
    {
        Direction = direction;
        Damage = damage;
        WeaponType = weaponType;
        transform.rotation = Quaternion.LookRotation(direction);
        _lifeTimer = 5f;
    }

    public void Shoot(Vector3 target, int damage)
    {
        Damage = damage;
        WeaponType = WeaponType.RocketAim;
        _target = target;
        _spawnPosition = transform.position;
        _bezierT = 0f;

        Vector3 toTargetXZ;
        if (target != null)
        {
            transform.rotation = Quaternion.LookRotation((target - transform.position).normalized);
            toTargetXZ = new Vector3(target.x - _spawnPosition.x, 0f, target.z - _spawnPosition.z);
        }
        else
        {
            toTargetXZ = new Vector3(transform.forward.x, 0f, transform.forward.z);
        }

        Vector3 controlDir = toTargetXZ.sqrMagnitude > 0.0001f ? toTargetXZ.normalized : Vector3.forward;
        _bezierControlPoint = _spawnPosition + controlDir * _bezierControlXZDistance + Vector3.up * _bezierControlHeight;
        _lifeTimer = 10f;
    }

    void Update()
    {
        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
        {
            ObjectPool.Instance.Return(gameObject);
            return;
        }

        if (WeaponType == WeaponType.Pistol)
            transform.position += Direction * Speed * Time.deltaTime;
        else if (WeaponType == WeaponType.RocketAim)
            UpdateRocketAimMove();
    }

    private void UpdateRocketAimMove()
    {
        // P2: target position (updates each frame if target moves)
        Vector3 p2 = _target != null ? _target : _spawnPosition + transform.forward * 100f;

        if (_bezierT < 1f)
        {
            // Advance t proportional to Speed, normalized by tangent magnitude for near-constant world speed
            Vector3 tangent = 2f * (1f - _bezierT) * (_bezierControlPoint - _spawnPosition)
                            + 2f * _bezierT * (p2 - _bezierControlPoint);
            float tangentMag = tangent.magnitude;
            _bezierT += tangentMag > 0.001f
                ? Speed * Time.deltaTime / tangentMag
                : Speed * Time.deltaTime * 0.01f;
            _bezierT = Mathf.Clamp01(_bezierT);
        }

        // Quadratic Bezier: B(t) = (1-t)^2 * P0 + 2(1-t)t * P1 + t^2 * P2
        float u = 1f - _bezierT;
        Vector3 newPos = u * u * _spawnPosition
                       + 2f * u * _bezierT * _bezierControlPoint
                       + _bezierT * _bezierT * p2;

        Vector3 moveDir = newPos - transform.position;
        if (moveDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(moveDir.normalized);

        transform.position = newPos;

        if (_bezierT >= 1f)
            Explode();
    }


    void OnTriggerEnter(Collider other)
    {
        if(impactEffectPrefab != null)
        {
            GameObject effect = ObjectPool.Instance.Get(impactEffectPrefab, transform.position, Quaternion.identity);
            ObjectPool.Instance.ReturnDelayed(effect, 2f);
        }
        if (WeaponType == WeaponType.RocketAim)
        {
            Explode();
            return;
        }

        IDamageable dmgTarget = other.GetComponentInParent<IDamageable>();
        if (dmgTarget != null)
        {
            dmgTarget.TakeDamage(Damage, WeaponType);
        }
        
        ObjectPool.Instance.Return(gameObject);
    }

    public void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, _enemyLayerMask);
        foreach (Collider hit in hits)
        {
            Debug.Log($"Explosion hit: {hit.name}");
            IDamageable dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg != null)
                dmg.TakeDamage(Damage, WeaponType);
        }
        ObjectPool.Instance.Return(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }

}
