using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elite_HuyetPhu : MonoBehaviour
{
    public enum State { Idle, Kiting, Attacking, SixTalismans }

    [Header("--- THÔNG SỐ TẦM XA ---")]
    [SerializeField] private float keepDistance = 5f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private LayerMask playerLayer;

    [Header("--- KỸ NĂNG SIX TALISMANS ---")]
    [SerializeField] private float skillCooldown = 10f;
    [SerializeField] private SimpleObjectPool talismanPool;
    [SerializeField] private float orbitRadius = 1.5f;

    private State currentState = State.Idle;
    private Transform playerTransform;
    private Animator animator;
    private float skillTimer;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        skillTimer = skillCooldown;
    }

    private void Update()
    {
        if (skillTimer > 0) skillTimer -= Time.deltaTime;

        FindPlayer();
        if (playerTransform == null) return;

        if (currentState == State.Idle || currentState == State.Kiting)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (skillTimer <= 0)
            {
                StartCoroutine(Routine_SixTalismansArray());
            }
            else
            {
                if (distance < keepDistance - 1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, transform.position + (transform.position - playerTransform.position), moveSpeed * Time.deltaTime);
                }
                else if (distance > keepDistance + 1f)
                {
                    transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
                }
                else
                {
                    StartCoroutine(Routine_NormalShoot());
                }
            }
        }
    }

    private void FindPlayer()
    {
        if (playerTransform != null) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 15f, playerLayer);
        if (hit != null) playerTransform = hit.transform;
    }

    private IEnumerator Routine_NormalShoot()
    {
        currentState = State.Attacking;
        animator.SetTrigger("CastBasic");

        yield return new WaitForSeconds(0.3f);
        if (talismanPool != null)
        {
            GameObject t = talismanPool.GetFromPool(transform.position, Quaternion.identity);
            Vector2 dir = (playerTransform.position - transform.position).normalized;
            t.GetComponent<Rigidbody2D>().linearVelocity = dir * 7f;
        }

        yield return new WaitForSeconds(1.2f);
        currentState = State.Idle;
    }

    private IEnumerator Routine_SixTalismansArray()
    {
        currentState = State.SixTalismans;
        skillTimer = skillCooldown;
        animator.SetTrigger("CastArray");

        List<GameObject> spawnedTalismans = new List<GameObject>();

        for (int i = 0; i < 6; i++)
        {
            float angle = i * 60f * Mathf.Deg2Rad;
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
            GameObject talisman = talismanPool.GetFromPool(spawnPos, Quaternion.identity);
            spawnedTalismans.Add(talisman);
        }

        float elapsed = 0f;
        while (elapsed < 1.5f)
        {
            elapsed += Time.deltaTime;
            for (int i = 0; i < spawnedTalismans.Count; i++)
            {
                if (spawnedTalismans[i] == null) continue;
                float angle = (i * 60f + elapsed * 180f) * Mathf.Deg2Rad;
                spawnedTalismans[i].transform.position = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * orbitRadius;
            }
            yield return null;
        }

        foreach (var talisman in spawnedTalismans)
        {
            if (talisman != null && playerTransform != null)
            {
                Vector2 dir = (playerTransform.position - talisman.transform.position).normalized;
                talisman.GetComponent<Rigidbody2D>().linearVelocity = dir * 12f;
            }
            yield return new WaitForSeconds(0.2f);
        }

        currentState = State.Idle;
    }

    private void OnDrawGizmosSelected()
    {
        // 1. Khoảng cách duy trì Kiting
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, keepDistance);

        // 2. Vòng tròn 6 lá Phù Chú xoay quanh
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, orbitRadius);
    }
}