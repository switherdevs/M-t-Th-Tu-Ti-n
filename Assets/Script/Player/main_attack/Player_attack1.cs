using StatsSystem.Components;
using UnityEngine;

public class TanCong : MonoBehaviour
{
    [Header("Gắn Các Prefab Vào Đây")]
    [SerializeField] private Transform shootPoint;       // Điểm sinh ra đạn trên người Tiêu Phong
    [SerializeField] private GameObject bulletPrefab;     // Prefab viên đạn (Kiếm)
    [SerializeField] private GameObject vfxMuzzlePrefab;  // Prefab hiệu ứng khói/tóe lửa khi vừa bấm bắn

    [Header("Thông Số Vũ Khí")]
    [SerializeField] private float bulletSpeed = 10f;     // Tốc độ bay của kiếm
    [SerializeField] private float fireRate = 0.2f;       // Khoảng cách giây giữa 2 lần bắn

    [Header("Cấu Hình Âm Thanh Tấn Công")]
    [SerializeField] private AudioClip attackSound;      // Âm thanh phát ra khi bắn
    [SerializeField][Range(0f, 1f)] private float attackSoundVolume = 0.7f;

    [Header("Cấu Hình Animation Bắn")]
    [SerializeField] private string shootAnimName = "Attack"; // Tên Trigger Animation bắn

    [Header("Hệ Thống Quản Lý Kỹ Năng")]
    [SerializeField] private PlayerSkillManager skillManager;

    private Vector2 mousePosition;  // Vị trí chuột trong thế giới 2D
    private float nextFireTime = 0f; // Biến tạm để tính toán thời gian được bắn phát tiếp theo
    private Camera mainCam;         // Biến lưu trữ Camera để tối ưu hiệu năng
    private CharacterStats myStats;
    private Animator anim;
    private AudioSource audioSource; // AudioSource tối ưu cho tốc độ bắn nhanh
    private int shootAnimHash;

    void Awake()
    {
        // Tối ưu: Lưu Camera lại một lần duy nhất lúc khởi tạo
        mainCam = Camera.main;
        myStats = GetComponent<CharacterStats>();

        // Tự động tìm Animator trên chính nó hoặc trên GameObject con
        anim = GetComponentInChildren<Animator>();

        // Tự động tìm hoặc thêm mới AudioSource trên Player
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Khởi tạo Mã Hash cho Animation bắn
        if (!string.IsNullOrEmpty(shootAnimName))
        {
            shootAnimHash = Animator.StringToHash(shootAnimName);
        }
    }

    // Thuật toán kiểm tra an toàn xem Animator có Parameter đó không
    private bool HasParameter(int paramHash)
    {
        if (anim == null) return false;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.nameHash == paramHash) return true;
        }
        return false;
    }

    void Update()
    {
        // 1. LẤY VỊ TRÍ CHUỘT KIỂU LEGACY DÙNG CHUẨN ĐỘ SÂU CAMERA
        if (mainCam != null)
        {
            Vector3 screenMousePos = Input.mousePosition;
            screenMousePos.z = -mainCam.transform.position.z;
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(screenMousePos);
            mousePosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        }

        // 2. KIỂM TRA CLICK VÀ ĐÈ CHUỘT LEGACY (TỐI ƯU SPEED VỚI NEXTFIRETIME)
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate; // Cập nhật thời điểm bắn tiếp theo
        }
    }

    private void Shoot()
    {
        // KÍCH HOẠT ANIMATION BẮN
        if (HasParameter(shootAnimHash))
        {
            anim.SetTrigger(shootAnimHash);
        }

        // TỐI ƯU ÂM THANH BẮN: Phát âm thanh bằng PlayOneShot để hỗ trợ sấy tốc độ cao không bị lặp rác memory
        if (audioSource != null && attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, attackSoundVolume);
        }

        // ĐIỀU KIỆN BẮT BUỘC
        if (shootPoint == null || bulletPrefab == null) return;

        // Tính toán hướng bay từ Điểm bắn đến Vị trí chuột
        Vector2 direction = (mousePosition - (Vector2)shootPoint.position).normalized;

        // Sinh ra viên Đạn (Kiếm) tại đúng vị trí shootPoint
        GameObject kiem = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        // Điều khiển hướng xoay của mũi kiếm và vận tốc bay
        PhiKiem scriptKiem = kiem.GetComponent<PhiKiem>();
        if (scriptKiem != null)
        {
            scriptKiem.Setup(direction, myStats);
        }

        Rigidbody2D rbKiem = kiem.GetComponent<Rigidbody2D>();
        if (rbKiem != null)
        {
            rbKiem.linearVelocity = direction * bulletSpeed;
        }

        // Tự động hủy viên đạn sau 3 giây
        Destroy(kiem, 3f);

        // HIỆU ỨNG VFX
        if (vfxMuzzlePrefab != null)
        {
            GameObject vfx = Instantiate(vfxMuzzlePrefab, shootPoint.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }
    }
}