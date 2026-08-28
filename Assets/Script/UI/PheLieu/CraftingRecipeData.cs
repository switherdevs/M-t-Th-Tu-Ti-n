using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemRequirement
{
    public ItemData itemData;
    public int soLuongYeuCau = 1;
}

[CreateAssetMenu(fileName = "NewCraftingRecipe", menuName = "Tu Tien/Crafting Recipe")]
public class CraftingRecipeData : ScriptableObject
{
    [Header("--- SẢN PHẨM NHẬN ĐƯỢC ---")]
    public ItemData itemKetQua;
    public int soLuongKetQua = 1; // Mặc định là 1

    [Header("--- NGUYÊN LIỆU YÊU CẦU ---")]
    public List<ItemRequirement> danhSachNguyenLieu = new List<ItemRequirement>();
}