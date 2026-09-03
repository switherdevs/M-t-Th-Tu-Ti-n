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

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCastBasic = "CastBasic";
    [SerializeField] private string animCastArray = "CastArray";

    private Transform playerTransform;
    private Animator animator;
    private CharacterStats stats;
    private Collider2D mainCollider;

    private int basicAttackCount = 0;
    private bool isBusy = false;
    private bool isWindingUp = false;
    private bool isDeadHandled = false;

    private WaitForSeconds waitBasicShootDelay = new WaitForSeconds(0.3f);
    private WaitForSeconds waitBasicShootEnd = new WaitForSeconds(1.2f);
    private WaitForSeconds waitTalismanLaunchDelay = new WaitForSeconds(0.2f);

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
        if (isBusy) return;

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
                StartCoroutine(Routine_SixTalismansArray());
            }
            else
            {
                StartCoroutine(Routine_NormalShoot());
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
                isBusy = true;
                if (mainCollider != null) mainCollider.enabled = false;
                PlaySFX(sfxDeath);
            }
            return true;
        }
        return false;
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
        animator.SetTrigger(animCastBasic);

        yield return waitBasicShootDelay;

        PlaySFX(sfxAttack);
        if (basicTalismanPool != null && playerTransform != null)
        {
            Vector3 spawnPos = GetAttackCenter();
            GameObject t = basicTalismanPool.GetFromPool(spawnPos, Quaternion.identity);

            if (t.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
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
        float originalAnimSpeed = animator.speed;
        moveSpeed *= slowMultiplier;
        animator.speed *= slowMultiplier;

        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        animator.speed = originalAnimSpeed;

        animator.SetTrigger(animCastArray);

        List<GameObject> spawnedTalismans = new List<GameObject>();

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;

            if (specialTalismanPool != null)
            {
                GameObject talisman = specialTalismanPool.GetFromPool(spawnPos, Quaternion.identity);
                spawnedTalismans.Add(talisman);
            }
        }

        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < spawnedTalismans.Count; i++)
            {
                if (spawnedTalismans[i] == null) continue;
                float angle = (i * 60f + elapsed * orbitRotationSpeed) * Mathf.Deg2Rad;
                spawnedTalismans[i].transform.position = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
            }
            yield return null;
        }

        foreach (var talisman in spawnedTalismans)
        {
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

        basicAttackCount = 0;
        isBusy = false;
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