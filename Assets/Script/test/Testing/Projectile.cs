using UnityEngine;
using StatsSystem.Interfaces;

namespace StatsSystem.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifeTime = 5f;

        private float damage;
        private GameObject owner;

        public void Setup(float damage, GameObject owner)
        {
            this.damage = damage;
            this.owner = owner;
            Destroy(gameObject, lifeTime);
        }

        private void Update()
        {
            // Trong 2D Top-Down, đạn thường bay theo hướng transform.up (hoặc transform.right)
            transform.Translate(Vector2.up * (speed * Time.deltaTime));
        }

        // DÙNG HÀM 2D
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == owner) return;

            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damage);
                Destroy(gameObject);
            }
            else if (!other.isTrigger)
            {
                // Trúng tường/vật cản 2D
                Destroy(gameObject);
            }
        }
    }
}