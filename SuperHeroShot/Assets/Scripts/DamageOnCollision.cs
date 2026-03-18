using UnityEngine;

public class DamageOnCollision : MonoBehaviour
{
    public float damage = 25f;
    public bool destroyOnHit = true;

    private void OnCollisionEnter(Collision collision)
    {
        TryDamage(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other.gameObject);
    }

    private void TryDamage(GameObject target)
    {
        if (target.TryGetComponent<IDamageable>(out var dmg))
        {
            dmg.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
