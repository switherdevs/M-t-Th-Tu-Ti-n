using System;

namespace StatsSystem.Core
{
    public enum StatModType
    {
        Flat = 100,         // Cộng thẳng (vd: +10 ATK)
        PercentAdd = 200,   // Cộng % tích lũy (vd: +10% ATK)
        PercentMult = 300   // Nhân % độc lập (vd: x1.2 ATK)
    }

    [Serializable]
    public class StatModifier
    {
        public float Value { get; }
        public StatModType Type { get; }
        public int Order { get; }      // Thứ tự ưu tiên tính toán
        public object Source { get; }   // Nguồn gốc modifier (Item, Skill, Buff...)

        public StatModifier(float value, StatModType type, int order, object source)
        {
            Value = value;
            Type = type;
            Order = order;
            Source = source;
        }

        public StatModifier(float value, StatModType type, object source)
            : this(value, type, (int)type, source) { }

        public StatModifier(float value, StatModType type)
            : this(value, type, (int)type, null) { }
    }
}