using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Khai báo thêm UI để dùng Slider
using TMPro;
using StatsSystem.Components;

public class PlayerSkillManager : MonoBehaviour
{
    [Header("--- DANH SÁCH KỸ NĂNG ĐANG TRANG BỊ ---")]
    [Tooltip("Ô 0 = Phím R (Skill 1), Ô 1 = Phím T (Skill 2), Ô 2 = Phím F (Skill 3)")]
    [SerializeField] private List<SkillData> equippedSkills = new List<SkillData>();

    [Header("--- CẤU HÌNH THANH NĂNG LƯỢNG (SLIDER UI) ---")]
    [Tooltip("Kéo Slider hiển thị thanh năng lượng trên Canvas vào đây")]
    [SerializeField] private Slider sliderNangLuong;

    [Tooltip("Năng lượng tối đa của nhân vật")]
    [SerializeField] private float nangLuongToiDa = 100f;

    [Tooltip("Tốc độ hồi năng lượng mỗi giây (Hồi mượt theo thời gian)")]
    [SerializeField] private float tocDoHoiNangLuong = 10f;

    [Tooltip("Text hiển thị chỉ số năng lượng dạng số (Ví dụ: 80/100) - Tùy chọn")]
    [SerializeField] private TextMeshProUGUI textNangLuong;

    [Header("--- UI HIỂN THỊ THỜI GIAN HỒI (TEXT MESH PRO) ---")]
    [Tooltip("Kéo các Text TMP hiển thị Cooldown tương ứng với Skill 1 (R), Skill 2 (T), Skill 3 (F) vào đây")]
    [SerializeField] private TextMeshProUGUI[] skillCooldownTexts;

    [Header("--- VỊ TRÍ BẮN & CHỈ SỐ ---")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private CharacterStats skillStat;

    // Biến lưu trữ năng lượng hiện tại
    private float nangLuongHienTai;

    // Mảng lưu thời gian hồi đếm ngược thực tế của từng skill
    private float[] cooldownTimers;

    private void Start()
    {
        // Khởi tạo năng lượng đầy ban đầu
        nangLuongHienTai = nangLuongToiDa;

        if (sliderNangLuong != null)
        {
            sliderNangLuong.maxValue = nangLuongToiDa;
            sliderNangLuong.value = nangLuongHienTai;
        }

        // Khởi tạo mảng đếm ngược thời gian hồi chiêu theo số lượng skill đang trang bị
        if (equippedSkills != null)
        {
            cooldownTimers = new float[equippedSkills.Count];
        }
    }

    private void Update()
    {
        // 1. Tự động hồi năng lượng mượt mà theo thời gian thực (Time.deltaTime)
        HoiNangLuongLienTuc();

        // 2. Cập nhật đếm ngược Cooldown và hiển thị chữ lên TextMeshPro UI
        CapNhatCooldownVaGiaoDien();

        // 3. Lắng nghe thao tác phím bấm từ người chơi
        XuLyNhapPhimKichHoatSkill();
    }

    /// <summary>
    /// Hàm tự động hồi năng lượng mượt mà và cập nhật thanh Slider UI
    /// </summary>
    private void HoiNangLuongLienTuc()
    {
        if (nangLuongHienTai < nangLuongToiDa)
        {
            // Cộng dần năng lượng theo thời gian
            nangLuongHienTai += tocDoHoiNangLuong * Time.deltaTime;

            // Khóa không cho vượt quá tối đa
            if (nangLuongHienTai > nangLuongToiDa)
            {
                nangLuongHienTai = nangLuongToiDa;
            }
        }

        // Cập nhật Slider UI
        if (sliderNangLuong != null)
        {
            sliderNangLuong.value = nangLuongHienTai;
        }

        // Cập nhật Text dạng số nếu có
        if (textNangLuong != null)
        {
            textNangLuong.text = $"{(int)nangLuongHienTai}/{(int)nangLuongToiDa}";
        }
    }

    /// <summary>
    /// Hàm tính toán đếm lùi thời gian hồi và cập nhật UI TextMeshPro
    /// </summary>
    private void CapNhatCooldownVaGiaoDien()
    {
        if (equippedSkills == null) return;

        for (int i = 0; i < equippedSkills.Count; i++)
        {
            // Nếu skill đang trong thời gian hồi -> Giảm dần thời gian theo từng giây
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
            else
            {
                cooldownTimers[i] = 0f; // Khóa không cho âm thời gian
            }

            // Cập nhật TextMeshPro nếu có kéo thả UI vào Inspector
            if (skillCooldownTexts != null && i < skillCooldownTexts.Length && skillCooldownTexts[i] != null)
            {
                if (cooldownTimers[i] > 0f)
                {
                    // Hiển thị số giây còn lại (lấy 1 chữ số thập phân, ví dụ: 3.5s)
                    skillCooldownTexts[i].text = $"{cooldownTimers[i]:F1}s";
                }
                else
                {
                    // Khi skill sẵn sàng dùng thì xóa chữ Cooldown
                    skillCooldownTexts[i].text = "";
                }
            }
        }
    }

    /// <summary>
    /// Hàm kiểm tra phím bấm R, T, F để kích hoạt Kỹ Năng tương ứng
    /// </summary>
    private void XuLyNhapPhimKichHoatSkill()
    {
        // Phím R -> Kích hoạt Skill ở vị trí 0 (Skill 1)
        if (Input.GetKeyDown(KeyCode.R))
        {
            ThucThiSkillTheoIndex(0);
        }
        // Phím T -> Kích hoạt Skill ở vị trí 1 (Skill 2)
        else if (Input.GetKeyDown(KeyCode.T))
        {
            ThucThiSkillTheoIndex(1);
        }
        // Phím F -> Kích hoạt Skill ở vị trí 2 (Skill 3)
        else if (Input.GetKeyDown(KeyCode.F))
        {
            ThucThiSkillTheoIndex(2);
        }
    }

    /// <summary>
    /// Hàm kiểm tra điều kiện (Cooldown + Năng lượng) và thi triển skill
    /// </summary>
    /// <param name="index">Chỉ số vị trí Skill (0 = R, 1 = T, 2 = F)</param>
    private void ThucThiSkillTheoIndex(int index)
    {
        // Kiểm tra an toàn: index phải hợp lệ và nằm trong danh sách
        if (index < 0 || index >= equippedSkills.Count) return;

        SkillData skill = equippedSkills[index];

        if (skill == null) return;

        // 1. KIỂM TRA ĐIỀU KIỆN 1: Thời gian hồi chiêu
        if (cooldownTimers[index] > 0f)
        {
            Debug.Log($"<color=yellow>[PLAYER SKILL]</color> {skill.skillName} đang trong thời gian hồi ({cooldownTimers[index]:F1}s còn lại)!");
            return;
        }

        // 2. KIỂM TRA ĐIỀU KIỆN 2: Năng lượng có đủ để dùng Skill không
        if (nangLuongHienTai < skill.manaCost)
        {
            Debug.LogWarning($"<color=red>[PLAYER SKILL]</color> Không đủ năng lượng dùng {skill.skillName}! Cần: {skill.manaCost} | Hiện có: {(int)nangLuongHienTai}");
            return;
        }

        // THỎA MÃN CẢ 2 ĐIỀU KIỆN -> THI TRIỂN SKILL
        Vector2 direction = LayHuongTheoConChuot();
        Transform pointToFire = firePoint != null ? firePoint : transform;

        // Trừ năng lượng của nhân vật
        nangLuongHienTai -= skill.manaCost;

        // Thực thi hàm tung skill
        skill.UseSkill(pointToFire, direction);

        // Gán lại thời gian hồi chiêu ban đầu
        cooldownTimers[index] = skill.cooldownTime;

        Debug.Log($"<color=green>[PLAYER SKILL]</color> Đã kích hoạt {skill.skillName}! Tốn {skill.manaCost} Mana. Cooldown: {skill.cooldownTime}s");
    }

    /// <summary>
    /// Hàm bổ trợ tính toán hướng từ Player đến vị trí con chuột trên màn hình
    /// </summary>
    private Vector2 LayHuongTheoConChuot()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        return direction == Vector2.zero ? Vector2.right : direction;
    }
}