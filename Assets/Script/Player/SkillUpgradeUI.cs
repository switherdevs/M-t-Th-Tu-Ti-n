using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 🎯 STRUCT CẬP NHẬT: Thay thế cộng dồn bằng HỆ SỐ NHÂN DAMAGE và HỆ SỐ GIẢM COOLDOWN
/// </summary>
[System.Serializable]
public struct CauHinhNangCapSkill
{
    [Header("Hệ số nhân sát thương (VD: 1.15 = tăng 15% sát thương mỗi cấp)")]
    public float heSoNhanDamage;

    [Header("Hệ số giảm thời gian hồi (VD: 0.9 = giảm 10% thời gian hồi mỗi cấp)")]
    public float heSoGiamCooldown;
}

public class SkillUpgradeUI : MonoBehaviour
{
    [Header("--- CẤU HÌNH TEST GAME ---")]
    [Tooltip("Tích chọn để bật chế độ Test: Cho phép nâng cấp skill tự do không cần Skill Point")]
    public bool testGame = false;

    [Header("--- CẤU HÌNH DỮ LIỆU SKILL ---")]
    public List<SkillData> danhSachSkill = new List<SkillData>();

    [Header("--- CẤU HÌNH HỆ SỐ NÂNG CẤP TỪNG SKILL ---")]
    [Tooltip("Danh sách hệ số cấu hình nâng cấp theo đúng thứ tự Index tương ứng với danhSachSkill")]
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

                    if (savedData.currentDamage > 0)
                    {
                        skill.baseDamage = savedData.currentDamage;
                    }
                    if (savedData.currentCooldown > 0)
                    {
                        skill.cooldownTime = savedData.currentCooldown;
                    }
                }
                else
                {
                    skill.ResetVeChiSoGoc();
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
    }

    public void CapNhatGiaoDienVisual()
    {
        int pointKhaDung = LaySkillPointKhaDung();
        int playerLevel = QuestSaveSystem.Instance != null ? (int)QuestSaveSystem.Instance.duLieuSaveHienTai.playerStats.level : 1;

        if (textLevelHienTai != null)
        {
            textLevelHienTai.text = $"Level Hiện Tại: <color=#FFD700>{playerLevel}</color>";
        }

        if (textSkillPoint != null)
        {
            // 🎯 Nếu đang bật testGame, hiển thị chữ [TEST MODE] sáng lên
            string trangThaiTest = testGame ? " <color=cyan>[TEST ON]</color>" : "";
            textSkillPoint.text = $"Skill Point: <color=#00FF00>{pointKhaDung}</color>{trangThaiTest}";
        }

        for (int i = 0; i < anhHighlightNut.Length; i++)
        {
            if (anhHighlightNut[i] == null) continue;

            if (i == indexSkillDangChon)
            {
                anhHighlightNut[i].color = mauDaChon;
            }
            else
            {
                // Nếu bật testGame thì luôn sáng bình thường, không bị tối khi hết điểm
                bool duDieuKienSang = testGame || (pointKhaDung > 0);
                anhHighlightNut[i].color = duDieuKienSang ? mauBinhThuong : mauToiKhiChuaDuLevel;
            }
        }

        for (int i = 0; i < textThongTinSkill.Length; i++)
        {
            if (textThongTinSkill[i] != null && i < danhSachSkill.Count && danhSachSkill[i] != null)
            {
                SkillData data = danhSachSkill[i];
                string hienThiCap = data.currentLevel >= SkillData.MAX_LEVEL ? "Cấp: 10 (MAX)" : $"Cấp: {data.currentLevel}";
                textThongTinSkill[i].text = $"<b>{data.skillName}</b>\n<color=#FFD700>{hienThiCap}</color>\nST: {data.baseDamage:F1} | Hồi: {data.cooldownTime:F1}s";
            }
        }
    }

    public void XacNhanNangCapSkill()
    {
        if (indexSkillDangChon == -1)
        {
            PhatAmThanh(amThanhNangCapThatBai);
            ThongBaoPopup("Vui lòng chọn 1 Kỹ năng trước khi bấm Nâng cấp!");
            return;
        }

        SkillData skillDuocChon = danhSachSkill[indexSkillDangChon];
        if (skillDuocChon != null)
        {
            // 1. Kiểm tra giới hạn tối đa cấp 10
            if (skillDuocChon.currentLevel >= SkillData.MAX_LEVEL)
            {
                PhatAmThanh(amThanhNangCapThatBai);
                ThongBaoPopup($"Kỹ năng <color=yellow>{skillDuocChon.skillName}</color> đã đạt cấp tối đa (10)!");
                return;
            }

            // 2. 🎯 KIỂM TRA ĐIỀU KIỆN SKILL POINT (BỎ QUA NẾU BẬT TEST GAME)
            if (!testGame)
            {
                int pointKhaDung = LaySkillPointKhaDung();
                if (pointKhaDung <= 0)
                {
                    PhatAmThanh(amThanhNangCapThatBai);
                    ThongBaoPopup("Không đủ Skill Point! Cần đạt mốc Level 10, 20, 30... để nhận điểm.");
                    return;
                }
            }

            if (indexSkillDangChon >= danhSachCauHinhNangCap.Count)
            {
                Debug.LogError($"[LỖI UI SKILL] Chưa cấu hình hệ số nâng cấp cho Skill tại Index {indexSkillDangChon}!");
                return;
            }

            // LẤY HỆ SỐ CẤU HÌNH TƯƠNG ƯNG
            CauHinhNangCapSkill cauHinh = danhSachCauHinhNangCap[indexSkillDangChon];

            // 3. Gọi hàm nâng cấp dùng hệ số nhân
            skillDuocChon.UpgradeSkill(cauHinh.heSoNhanDamage, cauHinh.heSoGiamCooldown);

            // 4. Lưu vào file Save TXT
            if (QuestSaveSystem.Instance != null)
            {
                QuestSaveSystem.Instance.LuuSkillFullData(
                    skillDuocChon.skillName,
                    skillDuocChon.currentLevel,
                    skillDuocChon.baseDamage,
                    skillDuocChon.cooldownTime
                );
            }

            // 5. Hiệu ứng FX & Âm thanh
            PhatAmThanh(amThanhNangCapThanhCong);
            TaoHieuUngUpgrade();

            string thongBaoText = testGame ?
                $"[TEST MODE] Nâng cấp <color=yellow>{skillDuocChon.skillName}</color> lên Cấp {skillDuocChon.currentLevel}!" :
                $"Nâng cấp thành công <color=yellow>{skillDuocChon.skillName}</color> lên Cấp {skillDuocChon.currentLevel}!";

            ThongBaoPopup(thongBaoText);

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