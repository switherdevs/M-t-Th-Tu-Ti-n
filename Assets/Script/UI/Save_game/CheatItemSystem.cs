using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StatsSystem.Components;

public class CheatItemSystem : MonoBehaviour
{
    [Header("--- 1. CẤU HÌNH NÚT CHEAT ITEM ---")]
    [SerializeField] private Button nutCheatItem;
    [SerializeField] private List<ItemData> danhSachItemCheat = new List<ItemData>();
    [SerializeField] private int soLuongItemAdd = 999;

    [Header("--- 2. CẤU HÌNH NÚT CHEAT DAMAGE ---")]
    [SerializeField] private Button nutCheatDamage;
    [SerializeField] private float damageHeSoNhan = 10f;

    [Header("--- 3. CẤU HÌNH NÚT CHEAT SPEED ---")]
    [SerializeField] private Button nutCheatSpeed;
    [SerializeField] private float moveSpeedCheat = 15f;
    [SerializeField] private float sprintSpeedCheat = 25f;

    [Header("--- CẤU HÌNH MÀU NÚT (TOGGLE VISUAL) ---")]
    [Tooltip("Màu khi nút ở trạng thái TẮT (Bình thường)")]
    [SerializeField] private Color mauNutThuong = Color.white;
    [Tooltip("Màu khi nút ở trạng thái BẬT (Đậm / Nổi bật)")]
    [SerializeField] private Color mauNutKichHoat = new Color(0.4f, 0.4f, 0.4f, 1f);

    // Biến lưu trạng thái Bật/Tắt riêng biệt cho từng tính năng
    private bool isItemCheatActive = false;
    private bool isDamageCheatActive = false;
    private bool isSpeedCheatActive = false;

    // Lưu thông số gốc
    private float moveSpeedGoc = 5f;
    private float sprintSpeedGoc = 9f;
    private float damageGoc = 20f;

    private PlayerController playerController;
    private CharacterStats playerStats;

    private void Start()
    {
        // Gán sự kiện Click cho 3 Button độc lập
        if (nutCheatItem != null) nutCheatItem.onClick.AddListener(ToggleCheatItem);
        if (nutCheatDamage != null) nutCheatDamage.onClick.AddListener(ToggleCheatDamage);
        if (nutCheatSpeed != null) nutCheatSpeed.onClick.AddListener(ToggleCheatSpeed);

        TimVaLuuThongSoGoc();
        CapNhatGiaoDienToanBoNut();
    }

    private void TimVaLuuThongSoGoc()
    {
        if (playerController == null) playerController = FindFirstObjectByType<PlayerController>();
        if (playerStats == null && playerController != null) playerStats = playerController.GetComponent<CharacterStats>();

        if (playerController != null)
        {
            moveSpeedGoc = (float)LayGiaTriPrivate(playerController, "moveSpeed", 5f);
            sprintSpeedGoc = (float)LayGiaTriPrivate(playerController, "sprintSpeed", 9f);
        }

        if (playerStats != null)
        {
            damageGoc = playerStats.Attack.Value;
        }
    }

    // ==========================================
    // 1. TÍNH NĂNG CHEAT ITEM (NÚT 1)
    // ==========================================
    public void ToggleCheatItem()
    {
        isItemCheatActive = !isItemCheatActive;

        if (isItemCheatActive)
        {
            if (QuestSaveSystem.Instance != null && danhSachItemCheat != null)
            {
                foreach (ItemData item in danhSachItemCheat)
                {
                    if (item != null && !string.IsNullOrEmpty(item.idItem))
                    {
                        QuestSaveSystem.Instance.LuuItemVaoSaveGame(item.idItem, soLuongItemAdd);
                    }
                }
                Debug.Log($"<color=green>[Cheat Item]</color> Đã dồn {soLuongItemAdd} item vào Save!");
            }
        }

        DoiMauButton(nutCheatItem, isItemCheatActive);
    }

    // ==========================================
    // 2. TÍNH NĂNG CHEAT DAMAGE (NÚT 2)
    // ==========================================
    public void ToggleCheatDamage()
    {
        isDamageCheatActive = !isDamageCheatActive;
        TimVaLuuThongSoGoc();

        if (playerStats != null)
        {
            if (isDamageCheatActive)
            {
                // Bật Cheat Damage: Nhân Sát thương
                playerStats.Attack.Value = damageGoc * damageHeSoNhan;
                Debug.Log("<color=red>[Cheat Damage]</color> Đã kích hoạt x10 Damage!");
            }
            else
            {
                // Tắt Cheat Damage: Đọc lại File Save để trả lại Damage gốc
                if (QuestSaveSystem.Instance != null && QuestSaveSystem.Instance.duLieuSaveHienTai != null)
                {
                    playerStats.TaiThongSoTuSaveFile();
                }
                else
                {
                    playerStats.Attack.Value = damageGoc;
                }
                Debug.Log("<color=yellow>[Cheat Damage]</color> Đã tắt Cheat Damage, trả về thông số gốc!");
            }
        }

        DoiMauButton(nutCheatDamage, isDamageCheatActive);
    }

    // ==========================================
    // 3. TÍNH NĂNG CHEAT SPEED (NÚT 3)
    // ==========================================
    public void ToggleCheatSpeed()
    {
        isSpeedCheatActive = !isSpeedCheatActive;
        TimVaLuuThongSoGoc();

        if (playerController != null)
        {
            if (isSpeedCheatActive)
            {
                // Bật Cheat Speed
                GanGiaTriPrivate(playerController, "moveSpeed", moveSpeedCheat);
                GanGiaTriPrivate(playerController, "sprintSpeed", sprintSpeedCheat);
                Debug.Log("<color=cyan>[Cheat Speed]</color> Đã kích hoạt Siêu Tốc Độ!");
            }
            else
            {
                // Tắt Cheat Speed: Trả về tốc độ ban đầu
                GanGiaTriPrivate(playerController, "moveSpeed", moveSpeedGoc);
                GanGiaTriPrivate(playerController, "sprintSpeed", sprintSpeedGoc);
                Debug.Log("<color=yellow>[Cheat Speed]</color> Đã trả Tốc Độ về bình thường!");
            }
        }

        DoiMauButton(nutCheatSpeed, isSpeedCheatActive);
    }

    // ==========================================
    // XỬ LÝ ĐỔI MÀU GIAO DIỆN NÚT (VISUAL)
    // ==========================================
    private void DoiMauButton(Button btn, bool isActive)
    {
        if (btn == null) return;

        // Đổi màu trực tiếp trên Target Graphic (Image) của Button
        if (btn.targetGraphic != null)
        {
            btn.targetGraphic.color = isActive ? mauNutKichHoat : mauNutThuong;
        }
    }

    private void CapNhatGiaoDienToanBoNut()
    {
        DoiMauButton(nutCheatItem, isItemCheatActive);
        DoiMauButton(nutCheatDamage, isDamageCheatActive);
        DoiMauButton(nutCheatSpeed, isSpeedCheatActive);
    }

    // REFLECTION CAN THIỆP BIẾN PRIVATE
    private object LayGiaTriPrivate(object obj, string fieldName, object defaultValue)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? field.GetValue(obj) : defaultValue;
    }

    private void GanGiaTriPrivate(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) field.SetValue(obj, value);
    }
}