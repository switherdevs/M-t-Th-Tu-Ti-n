using UnityEngine;
using StatsSystem.Components;

namespace StatsSystem.Combat
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint; // Vị trí nòng súng
        [SerializeField] private CharacterStats stats; // Để lấy chỉ số ATK hiện tại

        private void Awake()
        {
            if (stats == null) stats = GetComponent<CharacterStats>();
        }

        private void Update()
        {
            // Nhấn chuột trái (hoặc phím Space) để bắn
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }

        public void Shoot()
        {
            if (bulletPrefab == null || firePoint == null) return;

            // 1. Sinh ra viên đạn tại vị trí firePoint
            GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

            // 2. Lấy component Projectile và thiết lập Sát thương dựa theo ATK của nhân vật
            if (bulletObj.TryGetComponent<Projectile>(out var projectile))
            {
                // Truyền sát thương (ATK) + Người bắn (gameObject)
                projectile.Setup(stats.Attack.Value, gameObject);
            }
        }
    }
}