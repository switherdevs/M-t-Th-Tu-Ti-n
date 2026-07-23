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

    [Header("Thông Số Vũ Khí")]
    [SerializeField] private float bulletSpeed = 10f;     // Tốc độ bay của kiếm
    [SerializeField] private float fireRate = 0.2f;       // Khoảng cách giây giữa 2 lần bắn (Tốc độ sấy đạn)

    [Header("Hệ Thống Quản Lý Kỹ Năng")]
    [SerializeField] private PlayerSkillManager skillManager;

    private Vector2 mousePosition;  // Vị trí chuột trong thế giới 2D
    private float nextFireTime = 0f; // Biến tạm để tính toán thời gian được bắn phát tiếp theo
    private Camera mainCam;         // Biến lưu trữ Camera để tối ưu hiệu năng
    private CharacterStats myStats;

    void Awake()
    {
        // Tối ưu: Lưu Camera lại một lần duy nhất lúc khởi tạo để tránh dùng Camera.main gây lag
        mainCam = Camera.main;
        myStats = GetComponent<CharacterStats>();
    }

    void Update()
    {
        // 1. LẤY VỊ TRÍ CHUỘT KIỂU LEGACY: Đọc tọa độ chuột và chuyển sang thế giới 2D
        if (mainCam != null)
        {
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mousePosition = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
        }

        // 2. KIỂM TRA CLICK VÀ ĐÈ CHUỘT KIỂU LEGACY
        // Input.GetMouseButton(0): Trả về true LIÊN TỤC khi người dùng đang đè giữ chuột trái (Nút 0)
        // Time.time >= nextFireTime: Đảm bảo đã hết thời gian hồi chiêu mới cho bắn phát tiếp theo
        if (Input.GetMouseButton(0) && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate; // Cập nhật thời gian cho phát bắn kế tiếp
        }
    }

    private void Shoot()
    {
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
            scriptKiem.Setup(direction, myStats); // Gọi hàm xoay mũi kiếm trúng hướng chuột
        }

        Rigidbody2D rbKiem = kiem.GetComponent<Rigidbody2D>();
        if (rbKiem != null)
        {
            rbKiem.linearVelocity = direction * bulletSpeed; // Truyền lực đẩy kiếm bay đi
        }
        if (skillManager != null)
        {
            skillManager.TriggerAllSkills(shootPoint, direction);
        }

        // Tự động hủy viên đạn sau 3 giây để tránh tràn bộ nhớ
        Destroy(kiem, 3f);

        // HIỆU ỨNG VFX (Kiểm tra nếu người dùng ko bỏ vào = null thì bỏ qua)
        if (vfxMuzzlePrefab != null)
        {
            GameObject vfx = Instantiate(vfxMuzzlePrefab, shootPoint.position, Quaternion.identity);
            Destroy(vfx, 1f);
        }

        // ÂM THANH SFX (Kiểm tra nếu người dùng ko bỏ vào = null thì bỏ qua)
        if (sfxShootPrefab != null)
        {
            GameObject sfx = Instantiate(sfxShootPrefab, shootPoint.position, Quaternion.identity);
            Destroy(sfx, 2f);
        }
    }
}