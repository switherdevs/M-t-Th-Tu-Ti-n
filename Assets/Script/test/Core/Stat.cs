using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace StatsSystem.Core
{
    [Serializable]
    public class Stat
    {
        [SerializeField] private float baseValue;
        
        private bool isDirty = true;
        private float lastBaseValue;
        private float calculatedValue;

        private readonly List<StatModifier> statModifiers;
        public ReadOnlyCollection<StatModifier> StatModifiers { get; } 

        public event Action<Stat> OnValueChanged;

        public float BaseValue
        {
            get => baseValue;
            set
            {
                if (Mathf.Approximately(baseValue, value)) return;
                baseValue = value;
                isDirty = true;
                OnValueChanged?.Invoke(this);
            }
        }

        public float Value
        {
            get
            {
                if (isDirty || !Mathf.Approximately(baseValue, lastBaseValue))
                {
                    lastBaseValue = baseValue;
                    calculatedValue = CalculateFinalValue();
                    isDirty = false;
                }
                return calculatedValue;
            }
        }

        public Stat()
        {
            statModifiers = new List<StatModifier>();
            StatModifiers = statModifiers.AsReadOnly();
        }

        public Stat(float baseValue) : this()
        {
            this.baseValue = baseValue;
        }

        public void AddModifier(StatModifier mod)
        {
            isDirty = true;
            statModifiers.Add(mod);
            statModifiers.Sort(CompareModifierOrder);
            OnValueChanged?.Invoke(this);
        }

        public bool RemoveModifier(StatModifier mod)
        {
            if (statModifiers.Remove(mod))
            {
                isDirty = true;
                OnValueChanged?.Invoke(this);
                return true;
            }
            return false;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            bool removed = false;
            for (int i = statModifiers.Count - 1; i >= 0; i--)
            {
                if (statModifiers[i].Source == source)
                {
                    isDirty = true;
                    statModifiers.RemoveAt(i);
                    removed = true;
                }
            }
            if (removed) OnValueChanged?.Invoke(this);
            return removed;
        }

        private int CompareModifierOrder(StatModifier a, StatModifier b)
        {
            if (a.Order < b.Order) return -1;
            if (a.Order > b.Order) return 1;
            return 0;
        }

        private float CalculateFinalValue()
        {
            float finalValue = baseValue;
            float sumPercentAdd = 0;

            for (int i = 0; i < statModifiers.Count; i++)
            {
                StatModifier mod = statModifiers[i];

                if (mod.Type == StatModType.Flat)
                {
                    finalValue += mod.Value;
                }
                else if (mod.Type == StatModType.PercentAdd)
                {
                    sumPercentAdd += mod.Value;
                    if (i + 1 >= statModifiers.Count || statModifiers[i + 1].Type != StatModType.PercentAdd)
                    {
                        finalValue *= (1 + sumPercentAdd);
                        sumPercentAdd = 0;
                    }
                }
                else if (mod.Type == StatModType.PercentMult)
                {
                    finalValue *= (1 + mod.Value);
                }
            }

            return (float)Math.Round(finalValue, 4);
        }
    }
}