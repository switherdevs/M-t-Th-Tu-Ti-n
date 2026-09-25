using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [SerializeField, Tooltip("Tỷ lệ giảm sát thương (0.1 = 10%)")]
    private Stat defense = new Stat(0.1f);

    [Header("=== CURRENT STATE ===")]
    [SerializeField, ReadOnlyInspector]
    private float currentHealth;

    [SerializeField, ReadOnlyInspector]
    private bool isPoisoned = false;

    [SerializeField, ReadOnlyInspector]
    private bool isInvincible = false;

    [Header("=== PLAYER CONFIGURATION ===")]
    [SerializeField, Tooltip("Tick vào đây nếu GameObject này là Player")]
    private bool isPlayer = false;

    [SerializeField, Tooltip("Tên Trigger/Bool Animation Chết")]
    private string dieAnimName = "Die";

    [SerializeField, Tooltip("Thời gian chờ (giây) trước khi hiện UI GameOver")]
    private float deathDelay = 2.0f;

    [SerializeField, Tooltip("GameObject Canvas UI GameOver hiển thị khi người chơi chết")]
    private GameObject gameOverUI;

    [Header("=== BOSS CONFIGURATION ===")]
    [SerializeField, Tooltip("Tick vào đây nếu GameObject này là Boss")]
    private bool isBoss = false;

    [SerializeField, Tooltip("Hệ số nhân sát thương khi Boss bị mệt")]
    private float tiredDamageMultiplier = 3f;

    private BossDaSatMaQuan bossController;
    private BossPhaseSystem bossPhaseSystem;
    private Animator anim;
    private Coroutine poisonCoroutine;
    private Coroutine invincibleCoroutine;

    // Properties
    public Stat MaxHealth => maxHealth;
    public Stat Attack => attack;
    public Stat Defense => defense;

    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;
    public bool IsPlayer => isPlayer;
    public bool IsBoss => isBoss;
    public bool IsPoisoned => isPoisoned;
    public bool IsInvincible => isInvincible;

    // EVENTS
    public event Action<float, float> OnHealthChanged;
    public event Action<float> OnDamaged;
    public event Action<float> OnHealed;
    public event Action OnDeath;

    private void Awake()
    {
        anim = GetComponentInChildren<Animator>();

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }

        if (isBoss)
        {
            bossController = GetComponent<BossDaSatMaQuan>();
            bossPhaseSystem = GetComponent<BossPhaseSystem>();
        }
    }

    private void Start()
    {
        if (isPlayer)
        {
            TaiThongSoTuSaveFile();
        }
        else
        {
            currentHealth = MaxHealth.Value;
        }
    }

    public void TaiThongSoTuSaveFile()
    {
        if (QuestSaveSystem.Instance != null && QuestSaveSystem.Instance.duLieuSaveHienTai != null)
        {
            PlayerStatsSaveData statsSave = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats;

            maxHealth.Value = statsSave.maxHP;
            attack.Value = statsSave.damage;
            defense.Value = statsSave.armor;

            currentHealth = maxHealth.Value;

            OnHealthChanged?.Invoke(currentHealth, maxHealth.Value);
            Debug.Log($"<color=green>[CharacterStats]</color> Đã nạp thành công Stat từ Save: HP={MaxHealth.Value}, Atk={Attack.Value}, Def={Defense.Value}");
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
        if (!isBoss)
        {
            currentHealth = Mathf.Min(currentHealth, stat.Value);
        }

        OnHealthChanged?.Invoke(currentHealth, stat.Value);
    }

    public void SetCurrentHealth(float health)
    {
        currentHealth = Mathf.Clamp(health, 0f, MaxHealth.Value);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);
    }

    // 🎯 ĐÃ BỔ SUNG KIỂM TRA: CHỈ PLAYER MỚI ĐƯỢC BẤT TỬ
    public void SetInvincible(float duration)
    {
        // Kiểm tra an toàn: Phải tick isPlayer = true VÀ GameObject phải mang Tag "Player"
        if (!isPlayer || !CompareTag("Player")) return;

        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }
        invincibleCoroutine = StartCoroutine(Routine_InvincibleDuration(duration));
    }

    private IEnumerator Routine_InvincibleDuration(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        invincibleCoroutine = null;
    }

    public void TakeDamage(float rawDamage)
    {
        // Bị chặn ngay nếu đang chết, dame <= 0 HOẶC đang trong trạng thái bất tử
        if (IsDead || rawDamage <= 0 || isInvincible) return;

        if (isBoss && bossController != null)
        {
            if (bossController.IsTired)
            {
                rawDamage *= tiredDamageMultiplier;
            }
        }

        float finalDamage = rawDamage * (1f - Mathf.Clamp(Defense.Value, 0f, 0.95f));

        currentHealth -= finalDamage;
        currentHealth = Mathf.Max(0f, currentHealth);

        OnDamaged?.Invoke(finalDamage);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth.Value);

        if (IsDead)
        {
            if (isBoss)
            {
                if (bossPhaseSystem != null && bossPhaseSystem.IsCurrentPhaseLast)
                {
                    Die();
                }
            }
            else
            {
                Die();
            }
        }
    }

    public void ApplyPoison(float duration, float damagePerSecond)
    {
        if (!isPlayer || IsDead) return;

        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
        }

        poisonCoroutine = StartCoroutine(Routine_PoisonDamage(duration, damagePerSecond));
    }

    private IEnumerator Routine_PoisonDamage(float duration, float damagePerSecond)
    {
        isPoisoned = true;
        float timer = 0f;

        while (timer < duration && !IsDead)
        {
            yield return new WaitForSeconds(1f);
            TakeDamage(damagePerSecond);
            timer += 1f;
        }

        isPoisoned = false;
        poisonCoroutine = null;
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

        if (poisonCoroutine != null)
        {
            StopCoroutine(poisonCoroutine);
            isPoisoned = false;
        }

        if (isPlayer)
        {
            gameObject.tag = "Default";
            gameObject.layer = LayerMask.NameToLayer("Default");

            if (anim != null && !string.IsNullOrEmpty(dieAnimName))
            {
                anim.SetTrigger(dieAnimName);
            }

            StartCoroutine(Routine_PlayerDeathSequence());
        }
        else if (isBoss)
        {
            if (anim != null && !string.IsNullOrEmpty(dieAnimName))
            {
                anim.SetTrigger(dieAnimName);
            }
        }
    }

    private IEnumerator Routine_PlayerDeathSequence()
    {
        yield return new WaitForSecondsRealtime(deathDelay);

        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }
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

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyInspectorAttribute))]
public class ReadOnlyInspectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif