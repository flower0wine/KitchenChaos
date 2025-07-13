using System;
using UnityEngine;

[Serializable]
public class PlateSlot
{
    public FoodInPlateType foodInPlateType;  // 使用枚举替代字符串
    public Transform slotTransform;  // 物品放置的位置
    [HideInInspector] public GameObject placedItem;  // 放置在此槽位的物品引用
}