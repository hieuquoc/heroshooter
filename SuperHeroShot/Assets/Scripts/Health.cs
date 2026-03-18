using UnityEngine;
using UnityEngine.Events;
using System;

namespace rescueforce
{
    public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    public UnityEvent onDeath;
    public UnityEvent<float> onDamaged;
    public Action<float, WeaponType> onDamagedWithType;
    public float MaxHealth => maxHealth;
    private bool isDead = false;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = maxHealth;
        isDead = false;
    }

    public void TakeDamage(float amount, WeaponType weaponType = WeaponType.Pistol)
    {
        if (amount <= 0f || isDead) return;
        currentHealth -= amount;
        onDamaged?.Invoke(amount);
        onDamagedWithType?.Invoke(amount, weaponType);
        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void Die()
    {
        isDead = true;
        onDeath?.Invoke();
    }

    public float GetCurrentHealth() => currentHealth;
    
}

}

