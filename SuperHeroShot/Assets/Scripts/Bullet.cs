using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PooledObject
{
    public int Damage { get; set; }
    public Vector3 Direction { get; set; }
    public float Speed { get; set; }

    public void Shoot(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
    }

    void Update()
    {
        transform.position += Direction * Speed * Time.deltaTime;
    }


    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)        {
            enemy.TakeDamage(Damage);
            ObjectPool.Instance.Return(gameObject);
        }
    }

}
