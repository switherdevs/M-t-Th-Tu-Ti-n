using System.Collections.Generic;
using UnityEngine;

public class ItemExchangeManager : MonoBehaviour
{
    public static ItemExchangeManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Kiểm tra xem trong Save File có đủ số lượng Item cần trao đổi hay không
    /// </summary>
    public bool KiemTraDuItemDoi(ItemData itemCanDoi, int soLuongCan)
    {
        if (itemCanDoi == null || QuestSaveSystem.Instance == null) return false;

        int soLuongDangCo = QuestSaveSystem.Instance.LaySoLuongItemTrongKho(itemCanDoi.idItem);
        return soLuongDangCo >= soLuongCan;
    }

    /// <summary>
    /// Thực hiện trừ Item cần đổi và cộng Item Final vào Save Game
    /// </summary>
    public bool ThucHienTraoDoi(ItemData itemCanDoi, int soLuongCan, ItemData itemFinal, int soLuongFinal)
    {
        if (!KiemTraDuItemDoi(itemCanDoi, soLuongCan))
        {
            Debug.LogWarning("[Exchange System] Không đủ item để thực hiện trao đổi!");
            return false;
        }

        // 1. Trừ item nguyên liệu trong Save Game
        QuestSaveSystem.Instance.LuuItemVaoSaveGame(itemCanDoi.idItem, -soLuongCan);

        // 2. Cộng item thành phẩm (Final) vào Save Game
        QuestSaveSystem.Instance.LuuItemVaoSaveGame(itemFinal.idItem, soLuongFinal);

        Debug.Log($"<color=green>[Exchange System]</color> Đã đổi thành công {soLuongCan}x {itemCanDoi.tenItem} lấy {soLuongFinal}x {itemFinal.tenItem}");
        return true;
    }
}