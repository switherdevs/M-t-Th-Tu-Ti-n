using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [Tooltip("Năng lượng tối đa của nhân vật (Mặc định sẽ tự đồng bộ với Save)")]
    [SerializeField] private float nangLuongToiDa = 100f;

    [Tooltip("Tốc độ hồi năng lượng mỗi giây (Hồi mượt theo thời gian)")]
    [SerializeField] private float tocDoHoiNangLuong = 10f;

    [Tooltip("Text hiển thị chỉ số năng lượng dạng số (Ví dụ: 80/100)")]
    [SerializeField] private TextMeshProUGUI textNangLuong;

    [Header("--- UI HIỂN THỊ THỜI GIAN HỒI (TEXT MESH PRO) ---")]
    [SerializeField] private TextMeshProUGUI[] skillCooldownTexts;

    [Header("--- VỊ TRÍ BẮN & CHỈ SỐ ---")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private CharacterStats skillStat;

    private float nangLuongHienTai;
    private float[] cooldownTimers;

    private void Start()
    {
        // 🎯 Đồng bộ Energy Max từ Save System
        CapNhatMaxEnergyTuSave();

        nangLuongHienTai = nangLuongToiDa;

        if (equippedSkills != null)
        {
            cooldownTimers = new float[equippedSkills.Count];
        }
    }

    /// <summary>
    /// 🎯 Hàm load năng lượng tối đa từ QuestSaveSystem
    /// </summary>
    public void CapNhatMaxEnergyTuSave()
    {
        if (QuestSaveSystem.Instance != null && QuestSaveSystem.Instance.duLieuSaveHienTai != null)
        {
            nangLuongToiDa = QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.maxEnergy;
        }

        if (sliderNangLuong != null)
        {
            sliderNangLuong.maxValue = nangLuongToiDa;
        }
    }

    private void Update()
    {
        HoiNangLuongLienTuc();
        CapNhatCooldownVaGiaoDien();
        XuLyNhapPhimKichHoatSkill();
    }

    private void HoiNangLuongLienTuc()
    {
        if (nangLuongHienTai < nangLuongToiDa)
        {
            nangLuongHienTai += tocDoHoiNangLuong * Time.deltaTime;

            if (nangLuongHienTai > nangLuongToiDa)
            {
                nangLuongHienTai = nangLuongToiDa;
            }
        }

        if (sliderNangLuong != null)
        {
            sliderNangLuong.value = nangLuongHienTai;
        }

        // 🎯 Cập nhật Text display năng lượng (VD: 100/110)
        if (textNangLuong != null)
        {
            textNangLuong.text = $"{(int)nangLuongHienTai}/{(int)nangLuongToiDa}";
        }
    }

    private void CapNhatCooldownVaGiaoDien()
    {
        if (equippedSkills == null) return;

        for (int i = 0; i < equippedSkills.Count; i++)
        {
            if (cooldownTimers[i] > 0f)
            {
                cooldownTimers[i] -= Time.deltaTime;
            }
            else
            {
                cooldownTimers[i] = 0f;
            }

            if (skillCooldownTexts != null && i < skillCooldownTexts.Length && skillCooldownTexts[i] != null)
            {
                if (cooldownTimers[i] > 0f)
                {
                    skillCooldownTexts[i].text = $"{cooldownTimers[i]:F1}s";
                }
                else
                {
                    skillCooldownTexts[i].text = "";
                }
            }
        }
    }

    private void XuLyNhapPhimKichHoatSkill()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ThucThiSkillTheoIndex(0);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            ThucThiSkillTheoIndex(1);
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            ThucThiSkillTheoIndex(2);
        }
    }

    private void ThucThiSkillTheoIndex(int index)
    {
        if (index < 0 || index >= equippedSkills.Count) return;

        SkillData skill = equippedSkills[index];

        if (skill == null) return;

        if (cooldownTimers[index] > 0f)
        {
            Debug.Log($"<color=yellow>[PLAYER SKILL]</color> {skill.skillName} đang hồi chiêu ({cooldownTimers[index]:F1}s)!");
            return;
        }

        if (nangLuongHienTai < skill.manaCost)
        {
            Debug.LogWarning($"<color=red>[PLAYER SKILL]</color> Không đủ năng lượng dùng {skill.skillName}! Cần: {skill.manaCost} | Hiện có: {(int)nangLuongHienTai}");
            return;
        }

        Vector2 direction = LayHuongTheoConChuot();
        Transform pointToFire = firePoint != null ? firePoint : transform;

        nangLuongHienTai -= skill.manaCost;
        skill.UseSkill(pointToFire, direction);

        cooldownTimers[index] = skill.cooldownTime;

        Debug.Log($"<color=green>[PLAYER SKILL]</color> Kích hoạt {skill.skillName}! Tốn {skill.manaCost} Mana.");
    }

    private Vector2 LayHuongTheoConChuot()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector2 direction = (mouseWorldPos - transform.position).normalized;

        return direction == Vector2.zero ? Vector2.right : direction;
    }
}