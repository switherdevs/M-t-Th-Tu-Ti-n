using StatsSystem.Components;
using StatsSystem.Core;
using UnityEngine;

public class TanCong : MonoBehaviour
{
    [Header("Gắn Các Prefab Vào Đây")]
    [SerializeField] private Transform shootPoint;       // Điểm sinh ra đạn trên người Tiêu Phong
    [SerializeField] private GameObject bulletPrefab;     // Prefab viên đạn (Kiếm)
    [SerializeField] private GameObject vfxMuzzlePrefab;  // Prefab hiệu ứng khói/tóe lửa khi vừa bấm bắn (Tùy chọn)
    [SerializeField] private GameObject sfxShootPrefab;   // Prefab chứa Audio Source âm thanh tiếng kiếm khí (Tùy chọn)

    [Header("Âm Thanh Bắn (Mảng 3 Audio Clip)")]
    [SerializeField] private AudioClip[] shootClips;      // Chứa 3 file âm thanh tiếng chém/bắn

    [Header("Thông Số Vũ Khí")]
    [SerializeField] private float bulletSpeed = 10f;     // Tốc độ bay của kiếm
    [SerializeField] private float fireRate = 0.2f;       // Khoảng cách giây giữa 2 lần bắn (Tốc độ sấy đạn)

    [Header("Cấu Hình Animation Bắn")]
    [SerializeField] private string shootAnimName = "Attack"; // Tên Trigger Animation bắn

    [Header("Hệ Thống Quản Lý Kỹ Năng")]
    [SerializeField] private PlayerSkillManager skillManager;

    private Vector2 mousePosition;  // Vị trí chuột trong thế giới 2D
    private float nextFireTime = 0f; // Biến tạm để tính toán thời gian được bắn phát tiếp theo
    private Camera mainCam;         // Biến lưu trữ Camera để tối ưu hiệu năng
    private CharacterStats myStats;
    private Animator anim;
    private int shootAnimHash;

    // Biến phụ trợ cho thuật toán chống lặp âm thanh 2 lần liên tiếp
    private int lastSoundIndex = -1;

    void Awake()
    {
        // Tối ưu: Lưu Camera lại một lần duy nhất lúc khởi tạo
        mainCam = Camera.main;
        myStats = GetComponent<CharacterStats>();

        // Tự động tìm Animator trên chính nó hoặc trên GameObject con
        anim = GetComponentInChildren<Animator>();

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
            screenMousePos.z = -mainCam.transform.position.z; // Gán khoảng cách Z từ Camera đến Plane 2D
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(screenMousePos);
            mousePosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        }

        // 2. KIỂM TRA CLICK VÀ ĐÈ CHUỘT KIỂU LEGACY
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate; // Cập nhật thời gian cho phát bắn kế tiếp
        }
    }

    private void Shoot()
    {
        // KÍCH HOẠT ANIMATION BẮN (Không đụng vào anim.speed nữa để tránh làm nhanh chân chạy)
        if (HasParameter(shootAnimHash))
        {
            anim.SetTrigger(shootAnimHash);
        }

        // ĐIỀU KIỆN BẮT BUỘC: Nếu không có điểm bắn hoặc đạn thì dừng ngay để tránh lỗi crash game
        if (shootPoint == null || bulletPrefab == null) return;

        // Tính toán hướng bay từ Điểm bắn đến Vị trí chuột
        Vector2 direction = (mousePosition - (Vector2)shootPoint.position).normalized;

        // Sinh ra viên Đạn (Kiếm) tại đúng vị trí shootPoint
        GameObject kiem = Instantiate(bulletPrefab, shootPoint.position, Quaternion.identity);

        // Điều khiển hướng xoay của mũi kiếm và vận tốc bay thông qua Script phụ bổ trợ
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

        if (skillManager != null)
        {
            skillManager.TriggerAllSkills(shootPoint, direction);
        }

        // Tự động hủy viên đạn sau 3 giây để tránh tràn bộ nhớ
        Destroy(kiem, 3f);

        // HIỆU ỨNG VFX
        if (vfxMuzzlePrefab != null)
        {
            GameObject vfx = Instantiate(vfxMuzzlePrefab, shootPoint.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        // ÂM THANH SFX (Đã nâng cấp thuật toán Random chống lặp)
        PhatAmThanhRandom();
    }

    /// <summary>
    /// Thuật toán Random âm thanh không bị trùng lặp với âm thanh vừa phát liền trước
    /// </summary>
    private void PhatAmThanhRandom()
    {
        if (shootClips == null || shootClips.Length == 0) return;

        int randomIndex = 0;

        // Nếu mảng chỉ có 1 âm thanh thì chọn luôn index 0
        if (shootClips.Length == 1)
        {
            randomIndex = 0;
        }
        else
        {
            // Vòng lặp do-while: Liên tục lấy ngẫu nhiên cho đến khi randomIndex KHÁC lastSoundIndex
            do
            {
                randomIndex = Random.Range(0, shootClips.Length);
            }
            while (randomIndex == lastSoundIndex);
        }

        // Cập nhật lại chỉ số âm thanh vừa chọn
        lastSoundIndex = randomIndex;

        AudioClip clipDuocChon = shootClips[randomIndex];

        if (clipDuocChon == null) return;

        // Trường hợp 1: Nếu có sfxShootPrefab thì gán AudioClip được chọn vào AudioSource của Prefab đó
        if (sfxShootPrefab != null)
        {
            GameObject sfx = Instantiate(sfxShootPrefab, shootPoint.position, Quaternion.identity);
            AudioSource audioSource = sfx.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.clip = clipDuocChon;
                audioSource.Play();
            }
            Destroy(sfx, 2f);
        }
        // Trường hợp 2: Phát trực tiếp thông qua AudioSource.PlayClipAtPoint tiện lợi
        else
        {
            AudioSource.PlayClipAtPoint(clipDuocChon, shootPoint.position);
        }
    }
}