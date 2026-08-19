using System;
using System.Collections;
using UnityEngine;

// --- DỮ LIỆU STAT CƠ BẢN GIÚP BẠN KHÔNG CẦN SCRIPT CŨ NỮA ---
[System.Serializable]
public class Stat
{
    [SerializeField] private float baseValue;
    public event Action<Stat> OnValueChanged;

    public float Value
    {
        get => baseValue;
        set
        {
            baseValue = value;
            OnValueChanged?.Invoke(this);
        }
    }

    public Stat(float defaultValue)
    {
        baseValue = defaultValue;
    }
}

public enum StatType
{
    MaxHealth,
    Attack,
    Defense
}

public interface IDamageable
{
    void TakeDamage(float rawDamage);
}

[DisallowMultipleComponent]
public class CharacterStats : MonoBehaviour, IDamageable
{
    [Header("=== BASE STATS ===")]
    [SerializeField, Tooltip("Máu tối đa ban đầu")]
    private Stat maxHealth = new Stat(100f);

    [SerializeField, Tooltip("Sát thương gây ra ban đầu")]
    private Stat attack = new Stat(20f);

    [SerializeField, Tooltip("Tỷ lệ giảm sát thương (0.2 = 20%, tối đa 0.95)")]
    private Stat defense = new Stat(0.1f);

    [Header("=== CURRENT STATE ===")]
    [SerializeField, ReadOnlyInspector]
    private float currentHealth;

    [Header("=== PLAYER CONFIGURATION ===")]
    [SerializeField, Tooltip("Tick vào đây nếu GameObject này là Player")]
    private bool isPlayer = false;

    [SerializeField, Tooltip("Tên Trigger/Bool Animation Chết")]
    private string dieAnimName = "Die";

    [SerializeField, Tooltip("Thời gian chờ (giây) trước khi dừng game và hiện UI")]
    private float deathDelay = 2.0f;

    [SerializeField, Tooltip("GameObject Canvas UI GameOver hiển thị khi người chơi chết")]
    private GameObject gameOverUI;

    [Header("=== BOSS CONFIGURATION ===")]
    [SerializeField, Tooltip("Tick vào đây nếu GameObject này là Boss để kích hoạt cơ chế nhận X3 sát thương khi mệt")]
    private bool isBoss = false;

    [SerializeField, Tooltip("Hệ số nhân sát thương khi Boss bị mệt (Mặc định x3)")]
    private float tiredDamageMultiplier = 3f;

    // Tham chiếu tự động tới script Boss & Animator
    private BossDaSatMaQuan bossController;
    private Animator anim;

    // Properties
    public Stat MaxHealth => maxHealth;
    public Stat Attack => attack;
    public Stat Defense => defense;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    // EVENTS
    public event Action<float, float> OnHealthChanged; // (currentHP, maxHP)
    public event Action<float> OnDamaged;              // (damageTaken)
    public event Action<float> OnHealed;               // (healAmount)
    public event Action OnDeath;

    private void Awake()
    {
        currentHealth = MaxHealth.Value;
        anim = GetComponentInChildren<Animator>();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        if (isBoss)
        {
            bossController = GetComponent<BossDaSatMaQuan>();
            if (bossController == null)
            {
                Debug.LogWarning($"⚠️ [CharacterStats] {gameObject.name} được tick là Boss nhưng không tìm thấy script BossDaSatMaQuan!");
            }
        }
    }

    private void OnEnable()
    {
        MaxHealth.OnValueChanged += HandleMaxHealthChanged;
    }

    private void OnDisable()
    {
        MaxHealth.OnValueChanged -= HandleMaxHealthChanged;
    }

    private void HandleMaxHealthChanged(Stat stat)
    {
        currentHealth = Mathf.Min(currentHealth, stat.Value);
        OnHealthChanged?.Invoke(currentHealth, stat.Value);
    }

    public void TakeDamage(float rawDamage)
    {
        if (IsDead || rawDamage <= 0) return;

        if (isBoss && bossController != null)
        {
            if (bossController.IsTired)
            {
                rawDamage *= tiredDamageMultiplier;
                Debug.Log($"💥 BOSS ĐANG MỆT! Sát thương nhận vào bị nhân {tiredDamageMultiplier} lần: {rawDamage}");
            }
        }

        // TÍNH SÁT THƯƠNG ĐƠN GIẢN NỘI BỘ (Không cần DamageCalculator cũ nữa)
        float finalDamage = rawDamage * (1f - Mathf.Clamp(Defense.Value, 0f, 0.95f));

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0f, currentHealth);

        OnDamaged?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);

        if (IsDead)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (IsDead || amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, MaxHealth.Value);

        OnHealed?.Invoke(amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);
    }

    protected virtual void Die()
    {
        OnDeath?.Invoke();

        if (isPlayer)
        {
            gameObject.tag = "Untagged";
            gameObject.layer = LayerMask.NameToLayer("Default");

            if (anim != null && !string.IsNullOrEmpty(dieAnimName))
            {
                anim.SetTrigger(dieAnimName);
            }

            StartCoroutine(Routine_PlayerDeathSequence());
        }
    }

    private IEnumerator Routine_PlayerDeathSequence()
    {
        yield return new WaitForSeconds(deathDelay);

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public Stat GetStat(StatType type)
    {
        return type switch
        {
            StatType.MaxHealth => maxHealth,
            StatType.Attack => attack,
            StatType.Defense => defense,
            _ => null
        };
    }
}

public class ReadOnlyInspectorAttribute : PropertyAttribute { }