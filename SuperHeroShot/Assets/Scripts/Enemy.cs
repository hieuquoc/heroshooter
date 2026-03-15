using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : PooledObject
{
    public int MaxHealth = 1;
    public int Health = 1;
    public int Damage = 1;
    private float rayHitTime;
    private float rayHitInterval = 0.5f;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
