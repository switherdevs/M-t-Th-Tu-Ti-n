namespace StatsSystem.Core
{
    /// <summary>
    /// Định danh các loại Stat trong trò chơi.
    /// Dễ dàng mở rộng thêm Mana, CritRate, Armor, Status Effects...
    /// </summary>
    public enum StatType
    {
        MaxHealth,
        Attack,
        Defense,
        // Dễ dàng mở rộng trong tương lai:
        // CriticalRate,
        // CriticalDamage,
        // Shield,
        // Mana,
        // MovementSpeed
    }
}