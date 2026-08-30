using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite_HuyetPhu : MonoBehaviour
{
    [Header("--- TẦM NHÌN & TẦM BẮN ---")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float attackRange = 7f; // Vòng tròn tầm bắn
    [SerializeField] private Vector2 attackOffset = Vector2.zero;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- TÌM ĐƯỜNG & NÉ VẬT CẢN ---")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidRadius = 0.4f;

    [Header("--- KỸ NĂNG THƯỜNG (BASIC SKILL) ---")]
    [SerializeField] private SimpleObjectPool basicTalismanPool; // Pool đạn bắn thường
    [SerializeField] private float normalShootSpeed = 7f;        // Tốc độ đạn bắn thường
    [SerializeField] private int attacksToSpecial = 5;           // Số lần bắn thường để xả 1 lần chiêu đặc biệt

    [Header("--- KỸ NĂNG ĐẶC BIỆT (SIX TALISMANS) ---")]
    [SerializeField] private SimpleObjectPool specialTalismanPool; // Pool bùa tuyệt chiêu
    [SerializeField] private float windupTime = 2f;
    [SerializeField] private float slowMultiplier = 0.3f;
    [SerializeField] private float orbitRadius = 1.5f;
    [SerializeField] private float orbitRotationSpeed = 180f;      // Tốc độ xoay bùa (độ/giây)
    [SerializeField] private float specialShootSpeed = 12f;         // Tốc độ bùa tuyệt chiêu khi phóng đi

    [Header("--- ANIMATION STRINGS ---")]
    [SerializeField] private string animCastBasic = "CastBasic";
    [SerializeField] private string animCastArray = "CastArray";

    private Transform playerTransform;
    private Animator animator;

    private int basicAttackCount = 0; // Bộ đếm số lần bắn thường hiện tại
    private bool isBusy = false;
    private bool isWindingUp = false;

    // Cache các YieldInstruction để tránh tạo rác GC
    private WaitForSeconds waitBasicShootDelay = new WaitForSeconds(0.3f);
    private WaitForSeconds waitBasicShootEnd = new WaitForSeconds(1.2f);
    private WaitForSeconds waitTalismanLaunchDelay = new WaitForSeconds(0.2f);

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        basicAttackCount = 0;
    }

    private void Update()
    {
        if (isBusy) return;

        FindPlayer();
        if (playerTransform == null) return;

        Vector3 attackCenter = GetAttackCenter();
        FlipTowards(playerTransform.position);

        // Trạng thái gồng chiêu: Di chuyển chậm tới Player
        if (isWindingUp)
        {
            MoveSmoothly(playerTransform.position, moveSpeed);
            return;
        }

        // Kiểm tra Player có nằm trong tầm bắn không
        bool isPlayerInAttackRange = Physics2D.OverlapCircle(attackCenter, attackRange, playerLayer) != null;

        if (isPlayerInAttackRange)
        {
            // Nếu đã tích đủ số lần bắn thường -> Dùng kỹ năng đặc biệt
            if (basicAttackCount >= attacksToSpecial)
            {
                StartCoroutine(Routine_SixTalismansArray());
            }
            else
            {
                // Ngược lại -> Bắn thường
                StartCoroutine(Routine_NormalShoot());
            }
        }
        else
        {
            // Player ngoài tầm bắn -> Rượt theo Player
            MoveSmoothly(playerTransform.position, moveSpeed);
        }
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
        animator.SetTrigger(animCastBasic);

        yield return waitBasicShootDelay;

        // Bắn đạn thường
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

        // Tăng bộ đếm số lần bắn thường
        basicAttackCount++;

        yield return waitBasicShootEnd;
        isBusy = false;
    }

    private IEnumerator Routine_SixTalismansArray()
    {
        isWindingUp = true;

        float originalSpeed = moveSpeed;
        float originalAnimSpeed = animator.speed;
        moveSpeed *= slowMultiplier;
        animator.speed *= slowMultiplier;

        // Giai đoạn 1: Vận công (Windup)
        yield return new WaitForSeconds(windupTime);

        isWindingUp = false;
        isBusy = true;
        moveSpeed = originalSpeed;
        animator.speed = originalAnimSpeed;

        animator.SetTrigger(animCastArray);

        List<GameObject> spawnedTalismans = new List<GameObject>();

        // Giai đoạn 2: Tạo trận bùa 6 lá hình lục giác
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

        // Giai đoạn 3: Xoay trận bùa quanh quái
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

        // Giai đoạn 4: Lần lượt phóng từng lá bùa tuyệt chiêu
        foreach (var talisman in spawnedTalismans)
        {
            if (talisman != null && playerTransform != null)
            {
                if (talisman.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    Vector2 dir = (playerTransform.position - talisman.transform.position).normalized;
                    rb.linearVelocity = dir * specialShootSpeed;
                }
            }
            yield return waitTalismanLaunchDelay;
        }

        // Reset bộ đếm số lần bắn thường sau khi dùng xong tuyệt chiêu
        basicAttackCount = 0;

        isBusy = false;
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
        // Tầm phát hiện (Vàng)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Vòng tròn tầm bắn (Đỏ)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), attackRange);

        // Bán kính né vật cản (Xanh dương)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, avoidRadius);
    }
}