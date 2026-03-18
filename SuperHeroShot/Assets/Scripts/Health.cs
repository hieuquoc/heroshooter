using UnityEngine;
using UnityEngine.Events;
using System;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;
    public UnityEvent onDeath;
    public UnityEvent<float> onDamaged;
    public Action<float, WeaponType> onDamagedWithType;
    public float MaxHealth => maxHealth;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void SetMaxHealth(float value)
    {
        maxHealth = value;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount, WeaponType weaponType = WeaponType.Pistol)
    {
        if (amount <= 0f) return;
        currentHealth -= amount;
        onDamaged?.Invoke(amount);
        onDamagedWithType?.Invoke(amount, weaponType);
        if (currentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    private void Die()
    {
        onDeath?.Invoke();
    }

    public float GetCurrentHealth() => currentHealth;
    
}
