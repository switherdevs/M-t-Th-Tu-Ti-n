using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite_HuyetPhu : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TẦM BẮN ---")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 7f;
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.4f;

    [Header("--- KỸ NĂNG THƯỜNG (BASIC SKILL) ---")]
    [SerializeField] private SimpleObjectPool basicTalismanPool;
    [SerializeField] private float normalShootSpeed = 7f;
    [SerializeField] private int attacksToSpecial = 5;

    [Header("--- KỸ NĂNG ĐẶC BIỆT (SIX TALISMANS) ---")]
    [SerializeField] private SimpleObjectPool specialTalismanPool;
    [SerializeField] private float windupTime = 2f;
    [SerializeField] private float slowMultiplier = 0.3f;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float orbitRotationSpeed = 180f;
    [SerializeField] private float specialShootSpeed = 12f;

    [Header("--- ÂM THANH (AUDIO) ---")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sfxPrepareAttack;
    [SerializeField] private AudioClip sfxAttack;
    [SerializeField] private AudioClip sfxDeath;
    [SerializeField] private AudioClip sfxStun;

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCastBasic = "CastBasic";
    [SerializeField] private string animCastArray = "CastArray";
    [SerializeField] private string animIsStunned = "isStunned";

    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;
    private Collider2D mainCollider;

    private int basicAttackCount = 0;
    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isStunned = false;
    private bool isBeingExecuted = false;
    private bool isDeadHandled = false;

    private WaitForSeconds waitBasicShootDelay = new WaitForSeconds(0.3f);
    private WaitForSeconds waitBasicShootEnd = new WaitForSeconds(1.2f);
    private WaitForSeconds waitTalismanLaunchDelay = new WaitForSeconds(0.2f);

    private Coroutine currentBehaviorCoroutine;
    private List<GameObject> activeTalismans = new List<GameObject>();

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        stats = GetComponent<CharacterStats>();
        mainCollider = GetComponent<Collider2D>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        basicAttackCount = 0;
    }

    private void Update()
    {
        if (CheckAndHandleDeath()) return;

        // KIỂM TRA ĐẦU TIÊN: Khóa hoàn toàn nếu Choáng, Bận hoặc đang bị Kết liễu
        if (isStunned || isBeingExecuted || isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        Vector3 attackCenter = GetAttackCenter();
        FlipTowards(playerTransform.position);

        if (isWindingUp)
        {
            MoveSmoothly(playerTransform.position, moveSpeed);
            return;
        }

        bool isPlayerInAttackRange = Physics2D.OverlapCircle(attackCenter, attackRange, playerLayer) != null;

        if (isPlayerInAttackRange)
        {
            if (basicAttackCount >= attacksToSpecial)
            {
                currentBehaviorCoroutine = StartCoroutine(Routine_SixTalismansArray());
            }
            else
            {
                currentBehaviorCoroutine = StartCoroutine(Routine_NormalShoot());
            }
        }
        else
        {
            MoveSmoothly(playerTransform.position, moveSpeed);
        }
    }

    private bool CheckAndHandleDeath()
    {
        if (stats != null && stats.IsDead)
        {
            if (!isDeadHandled)
            {
                isDeadHandled = true;
                InterruptAllActions();
                if (mainCollider != null) mainCollider.enabled = false;
                PlaySFX(sfxDeath);
            }
            return true;
        }
        return false;
    }

    // NÂNG CẤP: Dừng triệt để mọi hành động & Reset Animation Speed
    private void InterruptAllActions()
    {
        StopAllCoroutines();
        currentBehaviorCoroutine = null;

        // Reset lại tốc độ Animator nếu đang bị slow do gồng chiêu
        if (animator != null)
        {
            animator.speed = 1f;
        }

        isBusy = true;
        isWindingUp = false;

        ClearActiveTalismans();
    }

    public void ApplyStun(float duration)
    {
        if (isDeadHandled || isBeingExecuted) return;
        InterruptAllActions();
        currentBehaviorCoroutine = StartCoroutine(Routine_GetStunned(duration));
    }

    private IEnumerator Routine_GetStunned(float duration)
    {
        isStunned = true;
        isBusy = true;

        if (animator != null && !string.IsNullOrEmpty(animIsStunned))
        {
            animator.SetBool(animIsStunned, true);
        }
        PlaySFX(sfxStun);

        yield return new WaitForSeconds(duration);

        if (animator != null && !string.IsNullOrEmpty(animIsStunned))
        {
            animator.SetBool(animIsStunned, false);
        }
        isStunned = false;
        isBusy = false;
    }

    public void OnStartExecution()
    {
        InterruptAllActions();
        isBeingExecuted = true;
        isStunned = false;
        isBusy = true;
    }

    private void FindPlayer()
    {
        if (playerTransform != null)
        {
            if (Vector2.Distance(transform.position, playerTransform.position) > detectionRange * 1.5f)
            {
                playerTransform = null;
            }
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRange, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private void MoveSmoothly(Vector3 targetPosition, float speed)
    {
        Vector2 currentPos = transform.position;
        Vector2 dirToTarget = ((Vector2)targetPosition - currentPos).normalized;
        Vector2 moveDir = dirToTarget;

        RaycastHit2D hit = Physics2D.CircleCast(currentPos, avoidRadius, dirToTarget, 1f, obstacleLayer);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            Vector2 slideDir = Vector2.Perpendicular(hit.normal).normalized;
            if (Vector2.Dot(dirToTarget, slideDir) < 0) slideDir = -slideDir;
            moveDir = (dirToTarget + slideDir * 1.5f).normalized;
        }

        transform.position += (Vector3)(moveDir * (speed * Time.deltaTime));
    }

    private IEnumerator Routine_NormalShoot()
    {
        isBusy = true;
        PlaySFX(sfxPrepareAttack);
        if (animator != null) animator.SetTrigger(animCastBasic);

        yield return waitBasicShootDelay;

        if (isStunned || isBeingExecuted || isDeadHandled) yield break;

        PlaySFX(sfxAttack);
        if (basicTalismanPool != null && playerTransform != null)
        {
            Vector3 spawnPos = GetAttackCenter();
            GameObject t = basicTalismanPool.GetFromPool(spawnPos, Quaternion.identity);

            if (t != null && t.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
            {
                Vector2 dir = (playerTransform.position - spawnPos).normalized;
                rb.linearVelocity = dir * normalShootSpeed;
            }
        }

        basicAttackCount++;
        yield return waitBasicShootEnd;
        isBusy = false;
    }

    private IEnumerator Routine_SixTalismansArray()
    {
        isWindingUp = true;
        PlaySFX(sfxPrepareAttack);

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator != null ? animator.speed : 1f;
        moveSpeed *= slowMultiplier;
        if (animator != null) animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        if (isStunned || isBeingExecuted || isDeadHandled)
        {
            moveSpeed = originalSpeed;
            if (animator != null) animator.speed = originalAnimSpeed;
            yield break;
        }

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        if (animator != null) animator.speed = originalAnimSpeed;

        if (animator != null) animator.SetTrigger(animCastArray);
        ClearActiveTalismans();

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;

            if (specialTalismanPool != null)
            {
                GameObject talisman = specialTalismanPool.GetFromPool(spawnPos, Quaternion.identity);
                if (talisman != null) activeTalismans.Add(talisman);
            }
        }

        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            if (isStunned || isBeingExecuted || isDeadHandled)
            {
                ClearActiveTalismans();
                yield break;
            }

            elapsed += Time.deltaTime;
            for (int i = 0; i < activeTalismans.Count; i++)
            {
                if (activeTalismans[i] == null) continue;
                float angle = (i * 60f + elapsed * orbitRotationSpeed) * Mathf.Deg2Rad;
                activeTalismans[i].transform.position = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
            }
            yield return null;
        }

        foreach (var talisman in activeTalismans)
        {
            if (isStunned || isBeingExecuted || isDeadHandled)
            {
                ClearActiveTalismans();
                yield break;
            }

            if (talisman != null && playerTransform != null)
            {
                PlaySFX(sfxAttack);
                if (talisman.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    Vector2 dir = (playerTransform.position - talisman.transform.position).normalized;
                    rb.linearVelocity = dir * specialShootSpeed;
                }
            }
            yield return waitTalismanLaunchDelay;
        }

        activeTalismans.Clear();
        basicAttackCount = 0;
        isBusy = false;
    }

    private void ClearActiveTalismans()
    {
        foreach (var talisman in activeTalismans)
        {
            if (talisman != null)
            {
                talisman.SetActive(false);
            }
        }
        activeTalismans.Clear();
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void FlipTowards(Vector3 target)
    {
        if (isStunned || isBeingExecuted) return;
        Vector3 scale = transform.localScale;
        scale.x = target.x > transform.position.x ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public Vector3 GetAttackCenter()
    {
        float direction = transform.localScale.x >= 0 ? 1f : -1f;
        return transform.position + new Vector3(attackOffset.x * direction, attackOffset.y, 0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
    }
}