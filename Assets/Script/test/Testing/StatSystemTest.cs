using UnityEngine;
using StatsSystem.Components;
using StatsSystem.Core;

namespace StatsSystem.Testing
{
    public class StatSystemTest : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterStats playerStats;
        [SerializeField] private CharacterStats enemyStats;

        private StatModifier testSwordBuff;

        private void Start()
        {
            if (playerStats == null || enemyStats == null)
            {
                Debug.LogError("Chưa gán PlayerStats hoặc EnemyStats vào StatSystemTest!");
                return;
            }

            // Đăng ký nghe Event thử nghiệm
            playerStats.OnDamaged += (damage) => Debug.Log($"<color=cyan>[PLAYER EVENT]</color> Bị đánh mất {damage} HP. HP còn lại: {playerStats.CurrentHealth}/{playerStats.MaxHealth.Value}");
            playerStats.OnDeath += () => Debug.Log("<color=red>[PLAYER EVENT] Player đã chết!</color>");

            enemyStats.OnDamaged += (damage) => Debug.Log($"<color=yellow>[ENEMY EVENT]</color> Bị đánh mất {damage} HP. HP còn lại: {enemyStats.CurrentHealth}/{enemyStats.MaxHealth.Value}");
            enemyStats.OnDeath += () => Debug.Log("<color=red>[ENEMY EVENT] Enemy đã chết!</color>");
        }

        private void Update()
        {
            // Phím 1: Player đánh Enemy
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log($"\n--- PLAYER TẤN CÔNG ENEMY ---");
                Debug.Log($"[Trước] Player ATK: {playerStats.Attack.Value} | Enemy DEF: {enemyStats.Defense.Value * 100}% | Enemy HP: {enemyStats.CurrentHealth}/{enemyStats.MaxHealth.Value}");

                enemyStats.TakeDamage(playerStats.Attack.Value);
            }

            // Phím 2: Enemy đánh Player
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log($"\n--- ENEMY TẤN CÔNG PLAYER ---");
                Debug.Log($"[Trước] Enemy ATK: {enemyStats.Attack.Value} | Player DEF: {playerStats.Defense.Value * 100}% | Player HP: {playerStats.CurrentHealth}/{playerStats.MaxHealth.Value}");

                playerStats.TakeDamage(enemyStats.Attack.Value);
            }

            // Phím 3: Trang bị kiếm +20 ATK cho Player (Demo Modifier)
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (testSwordBuff == null)
                {
                    testSwordBuff = new StatModifier(20f, StatModType.Flat, this);
                    playerStats.Attack.AddModifier(testSwordBuff);
                    Debug.Log($"<color=green>[EQUIP]</color> Đã trang bị Kiếm (+20 ATK). ATK hiện tại: {playerStats.Attack.Value}");
                }
                else
                {
                    playerStats.Attack.RemoveModifier(testSwordBuff);
                    testSwordBuff = null;
                    Debug.Log($"<color=orange>[UNEQUIP]</color> Đã tháo Kiếm. ATK hiện tại: {playerStats.Attack.Value}");
                }
            }

            // Phím 4: Hồi 25 HP cho Player
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                playerStats.Heal(25f);
                Debug.Log($"<color=green>[HEAL]</color> Player được hồi 25 HP. HP hiện tại: {playerStats.CurrentHealth}");
            }
        }
    }
}