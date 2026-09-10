using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// STRUCT CHỨA 2 SETTING CHỈ SỐ NÂNG CẤP DÀNH RIÊNG CHO TỪNG SKILL
/// </summary>
[System.Serializable]
public struct CauHinhNangCapSkill
{
    [Tooltip("Lượng sát thương cộng thêm riêng cho Skill này khi nâng cấp")]
    public float satThuongCongThem;

    [Tooltip("Thời gian hồi giảm riêng cho Skill này khi nâng cấp")]
    public float thoiGianHoiGiam;
}

public class SkillUpgradeUI : MonoBehaviour
{
    [Header("--- CẤU HÌNH DỮ LIỆU SKILL ---")]
    public List<SkillData> danhSachSkill = new List<SkillData>();

    [Header("--- CẤU HÌNH CHỈ SỐ NÂNG CẤP TỪNG SKILL ---")]
    [Tooltip("Danh sách cấu hình chỉ số nâng cấp theo đúng thứ tự Index tương ứng với danhSachSkill")]
    public List<CauHinhNangCapSkill> danhSachCauHinhNangCap = new List<CauHinhNangCapSkill>();

    [Header("--- CẤU HÌNH UI NÚT SELECT SKILL ---")]
    public Button[] nutChonSkill;
    public Image[] anhHighlightNut;

    [Tooltip("Gán 3 Text để hiển thị trực tiếp Level + Chỉ số riêng của từng Skill")]
    public TextMeshProUGUI[] textThongTinSkill;

    [Header("--- CẤU HÌNH NÚT NÂNG CẤP, LEVEL & SKILL POINT ---")]
    public Button nutNangCap;
    public TextMeshProUGUI textLevelHienTai;
    public TextMeshProUGUI textSkillPoint;

    [Header("--- CẤU HÌNH TEXT THÔNG BÁO POPUP (FADE IN/OUT) ---")]
    public TextMeshProUGUI textThongBaoPopup;
    public float thoiGianHienPopup = 1.5f;
    public float tocDoFade = 2f;

    [Header("--- CẤU HÌNH AUDIO & HIỆU ỨNG (FX) ---")]
    public AudioSource audioSource;
    public AudioClip amThanhNangCapThanhCong;
    public AudioClip amThanhNangCapThatBai;
    public GameObject hieuUngNangCapPrefab;
    public Transform viTriXuatHienHieuUng;

    [Header("--- CẤU HÌNH MÀU SẮC VISUAL ---")]
    public Color mauBinhThuong = Color.white;
    public Color mauDaChon = Color.yellow;
    public Color mauToiKhiChuaDuLevel = new Color(0.4f, 0.4f, 0.4f, 1f);

    private int indexSkillDangChon = -1;
    private Coroutine popupCoroutine;

    private void Start()
    {
        LoadSkillDataFromSave();
        KhoiTaoGiaoDienUI();
    }

    private void OnEnable()
    {
        LoadSkillDataFromSave();
        KhoiTaoGiaoDienUI();
    }

    /// <summary>
    /// Đọc dữ liệu từ Save System và GHI ĐÈ chỉ số (Level, Damage, Cooldown) vào ScriptableObject SkillData
    /// </summary>
    private void LoadSkillDataFromSave()
    {
        if (QuestSaveSystem.Instance == null) return;

        foreach (var skill in danhSachSkill)
        {
            if (skill != null)
            {
                SaveSkillData savedData = QuestSaveSystem.Instance.LayDuLieuSkill(skill.skillName);
                if (savedData != null && savedData.skillLevel > 0)
                {
                    skill.currentLevel = savedData.skillLevel;

                    // Ghi đè chỉ số đã được nâng cấp từ save vào ScriptableObject
                    if (savedData.currentDamage > 0)
                    {
                        skill.baseDamage = savedData.currentDamage;
                    }
                    if (savedData.currentCooldown > 0)
                    {
                        skill.cooldownTime = savedData.currentCooldown;
                    }
                }
            }
        }
    }

    public void KhoiTaoGiaoDienUI()
    {
        indexSkillDangChon = -1;

        if (textThongBaoPopup != null)
        {
            Color c = textThongBaoPopup.color;
            c.a = 0f;
            textThongBaoPopup.color = c;
        }

        for (int i = 0; i < nutChonSkill.Length; i++)
        {
            int index = i;
            if (nutChonSkill[i] != null)
            {
                nutChonSkill[i].onClick.RemoveAllListeners();
                nutChonSkill[i].interactable = true;
                nutChonSkill[i].onClick.AddListener(() => ChonSkill(index));
            }
        }

        if (nutNangCap != null)
        {
            nutNangCap.onClick.RemoveAllListeners();
            nutNangCap.onClick.AddListener(XacNhanNangCapSkill);
        }

        CapNhatGiaoDienVisual();
    }

    public int TinhTongSkillPointTheoLevel()
    {
        if (QuestSaveSystem.Instance == null || QuestSaveSystem.Instance.duLieuSaveHienTai == null)
            return 0;

        int playerLevel = (int)QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.level;
        return playerLevel / 10;
    }

    public int LaySkillPointKhaDung()
    {
        int tongPoint = TinhTongSkillPointTheoLevel();
        int tongDiemDaDung = 0;

        foreach (var skill in danhSachSkill)
        {
            if (skill != null)
            {
                tongDiemDaDung += (skill.currentLevel - 1);
            }
        }

        int pointConLai = tongPoint - tongDiemDaDung;
        return Mathf.Max(0, pointConLai);
    }

    public void ChonSkill(int index)
    {
        if (index < 0 || index >= danhSachSkill.Count) return;

        indexSkillDangChon = index;
        CapNhatGiaoDienVisual();

        Debug.Log($"<color=cyan>[UI Skill]</color> Đã Chọn Skill: {danhSachSkill[index].skillName}");
    }

    public void CapNhatGiaoDienVisual()
    {
        int pointKhaDung = LaySkillPointKhaDung();
        int playerLevel = QuestSaveSystem.Instance != null ? (int)QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.level : 1;

        // 1. Cập nhật Text Level Player & Skill Point Còn Lại
        if (textLevelHienTai != null)
        {
            textLevelHienTai.text = $"Level Hiện Tại: <color=#FFD700>{playerLevel}</color>";
        }

        if (textSkillPoint != null)
        {
            textSkillPoint.text = $"Skill Point: <color=#00FF00>{pointKhaDung}</color>";
        }

        // 2. Cập nhật Visual Nút Bấm
        for (int i = 0; i < anhHighlightNut.Length; i++)
        {
            if (anhHighlightNut[i] == null) continue;

            if (i == indexSkillDangChon)
            {
                anhHighlightNut[i].color = mauDaChon;
            }
            else
            {
                anhHighlightNut[i].color = (pointKhaDung > 0) ? mauBinhThuong : mauToiKhiChuaDuLevel;
            }
        }

        // 3. HIỂN THỊ CHI TIẾT TỪNG LEVEL VÀ SÁT THƯƠNG ĐÃ NÂNG CẤP LÊN UI
        for (int i = 0; i < textThongTinSkill.Length; i++)
        {
            if (textThongTinSkill[i] != null && i < danhSachSkill.Count && danhSachSkill[i] != null)
            {
                SkillData data = danhSachSkill[i];
                textThongTinSkill[i].text = $"<b>{data.skillName}</b>\n<color=#FFD700>Cấp: {data.currentLevel}</color>\nST: {data.baseDamage} | Hồi: {data.cooldownTime:F1}s";
            }
        }
    }

    public void XacNhanNangCapSkill()
    {
        int pointKhaDung = LaySkillPointKhaDung();

        if (pointKhaDung <= 0)
        {
            PhatAmThanh(amThanhNangCapThatBai);
            ThongBaoPopup("Không đủ Skill Point! Cần đạt mốc Level 10, 20, 30... để nhận điểm.");
            return;
        }

        if (indexSkillDangChon == -1)
        {
            PhatAmThanh(amThanhNangCapThatBai);
            ThongBaoPopup("Vui lòng chọn 1 Kỹ năng trước khi bấm Nâng cấp!");
            return;
        }

        if (indexSkillDangChon >= danhSachCauHinhNangCap.Count)
        {
            Debug.LogError($"[LỖI UI SKILL] Chưa cấu hình chỉ số nâng cấp cho Skill tại Index {indexSkillDangChon} trong danhSachCauHinhNangCap!");
            return;
        }

        SkillData skillDuocChon = danhSachSkill[indexSkillDangChon];
        if (skillDuocChon != null)
        {
            // LẤY ĐÚNG STRUCT CẤU HÌNH CÙNG INDEX VỚI SKILL ĐƯỢC CHỌN
            CauHinhNangCapSkill cauHinh = danhSachCauHinhNangCap[indexSkillDangChon];

            // 1. Nâng chỉ số Kỹ năng bằng thông số riêng của Skill đó
            skillDuocChon.UpgradeSkill(cauHinh.satThuongCongThem, cauHinh.thoiGianHoiGiam);

            // 2. LƯU CẤP ĐỘ + SÁT THƯƠNG + COOLDOWN MỚI VÀO FILE TXT SAVE GAME
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.LuuSkillFullData(
                    skillDuocChon.skillName,
                    skillDuocChon.currentLevel,
                    skillDuocChon.baseDamage,
                    skillDuocChon.cooldownTime
                );
            }

            // 3. Hiệu ứng FX & Âm thanh
            PhatAmThanh(amThanhNangCapThanhCong);
            TaoHieuUngUpgrade();

            ThongBaoPopup($"Nâng cấp thành công <color=yellow>{skillDuocChon.skillName}</color> lên Cấp {skillDuocChon.currentLevel}!");

            indexSkillDangChon = -1;
            CapNhatGiaoDienVisual();
        }
    }

    private void PhatAmThanh(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void TaoHieuUngUpgrade()
    {
        if (hieuUngNangCapPrefab != null)
        {
            Transform pos = viTriXuatHienHieuUng != null ? viTriXuatHienHieuUng : transform;
            GameObject fx = Instantiate(hieuUngNangCapPrefab, pos.position, Quaternion.identity, pos);
            Destroy(fx, 2f);
        }
    }

    public void ThongBaoPopup(string noiDung)
    {
        if (textThongBaoPopup == null) return;

        if (popupCoroutine != null)
        {
            StopCoroutine(popupCoroutine);
        }

        popupCoroutine = StartCoroutine(CoRoutineFadePopup(noiDung));
    }

    private IEnumerator CoRoutineFadePopup(string noiDung)
    {
        textThongBaoPopup.text = noiDung;

        Color c = textThongBaoPopup.color;
        c.a = 1f;
        textThongBaoPopup.color = c;

        yield return new WaitForSeconds(thoiGianHienPopup);

        while (c.a > 0f)
        {
            c.a -= Time.deltaTime * tocDoFade;
            textThongBaoPopup.color = c;
            yield return null;
        }

        c.a = 0f;
        textThongBaoPopup.color = c;
    }
}