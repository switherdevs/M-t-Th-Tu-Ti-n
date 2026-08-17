namespace StatsSystem.Interfaces
{
    public interface IDamageable
    {
        bool IsDead { get; }

        /// <summary>
        /// Xử lý nhận sát thương thô từ bên ngoài
        /// </summary>
        /// <param name="rawDamage">Sát thương đầu vào trước khi tính DEF</param>
        void TakeDamage(float rawDamage);

        /// <summary>
        /// Xử lý hồi máu
        /// </summary>
        void Heal(float amount);
    }
}