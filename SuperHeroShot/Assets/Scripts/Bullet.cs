using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledObject
{
    public int Damage { get; set; }
    public Vector3 Direction { get; set; }
    public float Speed = 20f;
    public WeaponType WeaponType { get; set; }

    [SerializeField] private Transform _target;
    [SerializeField] private float _rocketTurnSpeed = 180f;

    private float _lifeTimer;

    public void Shoot(Vector3 direction, WeaponType weaponType)
    {
        Direction = direction;
        WeaponType = weaponType;
        transform.rotation = Quaternion.LookRotation(direction);
        _lifeTimer = 5f;
    }

    public void Shoot(Transform target, WeaponType weaponType)
    {
        WeaponType = weaponType;
        _target = target;
        if (target != null)
            transform.rotation = Quaternion.LookRotation((target.position - transform.position).normalized);
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
        if (_target != null)
        {
            Vector3 dirToTarget = (_target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(dirToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rocketTurnSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * Speed * Time.deltaTime;
    }


    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)        {
            enemy.TakeDamage(Damage, WeaponType);            
        }
        ObjectPool.Instance.Return(gameObject);
    }

}
