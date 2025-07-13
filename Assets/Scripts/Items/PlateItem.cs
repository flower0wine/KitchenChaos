using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

/// <summary>
/// 表示一个可以盛放食物的盘子
/// </summary>
public class PlateItem : PickableItem
{
    [Header("槽位设置")]
    public PlateSlot[] itemSlots;  // 盘子上的所有槽位
    
    [Tooltip("放置物品的音效")]
    public AudioClip placeItemSound;
    
    // 音频源
    private AudioSource audioSource;
    
    // 已放置的物品字典 - 类型到GameObject的映射
    private Dictionary<FoodInPlateType, GameObject> placedItems = new Dictionary<FoodInPlateType, GameObject>();
    
    // 添加食物事件 - 参数为食物类型和食物对象
    [Serializable]
    public class FoodItemEvent : UnityEvent<FoodInPlateType, GameObject> { }
    
    // 事件声明
    public FoodItemEvent OnItemAdded = new FoodItemEvent();
    public FoodItemEvent OnItemRemoved = new FoodItemEvent();
    
    protected override void Awake()
    {
        base.Awake();
        
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && placeItemSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 确保所有槽位初始状态为空
        foreach (var slot in itemSlots)
        {
            slot.placedItem = null;
        }
    }
    
    /// <summary>
    /// 尝试将物品添加到盘子上
    /// </summary>
    public bool TryAddItemToPlate(GameObject item)
    {
        if (item == null) return false;
        
        // 获取物品类型
        PlatableItem platableItem = item.GetComponent<PlatableItem>();
        if (platableItem == null) return false;
        
        FoodInPlateType foodInPlateType = platableItem.GetFoodInPlateType();
        
        // 检查是否已经有同类物品
        if (placedItems.ContainsKey(foodInPlateType))
        {
            Debug.LogWarning($"盘子上已经有{foodInPlateType}了");
            return false;
        }
        
        // 查找对应的槽位
        PlateSlot targetSlot = FindSlotForItemType(foodInPlateType);
        if (targetSlot == null)
        {
            Debug.LogWarning($"盘子上没有适合{foodInPlateType}的槽位");
            return false;
        }
        
        // 放置物品到槽位
        MoveItemToSlot(item, targetSlot);
        
        // 更新引用
        placedItems[foodInPlateType] = item;
        targetSlot.placedItem = item;
        
        // 播放音效
        if (audioSource != null && placeItemSound != null)
        {
            audioSource.PlayOneShot(placeItemSound);
        }
        
        // 物品已放入盘子，通知物品
        platableItem.OnPlacedOnPlate(this, targetSlot.slotTransform);
        
        // 触发添加物品事件
        OnItemAdded.Invoke(foodInPlateType, item);
        
        return true;
    }
    
    /// <summary>
    /// 查找适合特定物品类型的槽位
    /// </summary>
    private PlateSlot FindSlotForItemType(FoodInPlateType foodInPlateType)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.foodInPlateType == foodInPlateType && slot.placedItem == null)
            {
                return slot;
            }
        }
        return null;
    }
    
    /// <summary>
    /// 将物品移动到指定槽位
    /// </summary>
    private void MoveItemToSlot(GameObject item, PlateSlot slot)
    {
        // 将物品设置为槽位的子物体
        item.transform.SetParent(slot.slotTransform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
        
        // 禁用物品的碰撞器和刚体（防止物理影响）
        Collider collider = item.GetComponent<Collider>();
        if (collider) collider.enabled = false;
        
        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }
    
    /// <summary>
    /// 从盘子上移除特定类型的物品
    /// </summary>
    public GameObject RemoveItemFromPlate(FoodInPlateType foodInPlateType)
    {
        if (!placedItems.ContainsKey(foodInPlateType))
            return null;
            
        GameObject item = placedItems[foodInPlateType];
        
        // 找到对应的槽位并清除引用
        foreach (var slot in itemSlots)
        {
            if (slot.foodInPlateType == foodInPlateType && slot.placedItem == item)
            {
                slot.placedItem = null;
                break;
            }
        }
        
        // 移除字典中的引用
        placedItems.Remove(foodInPlateType);
        
        // 触发移除物品事件
        OnItemRemoved.Invoke(foodInPlateType, item);
        
        return item;
    }
    
    /// <summary>
    /// 检查盘子是否含有特定类型的物品
    /// </summary>
    public bool HasItem(FoodInPlateType foodInPlateType)
    {
        return placedItems.ContainsKey(foodInPlateType);
    }
    
    /// <summary>
    /// 获取盘子上所有已放置的物品
    /// </summary>
    public Dictionary<FoodInPlateType, GameObject> GetAllPlacedItems()
    {
        return new Dictionary<FoodInPlateType, GameObject>(placedItems);
    }
    
    /// <summary>
    /// 清空盘子上的所有物品
    /// </summary>
    public void ClearPlate()
    {
        // 保存当前的食物项，以便触发事件
        Dictionary<FoodInPlateType, GameObject> itemsToRemove = new Dictionary<FoodInPlateType, GameObject>(placedItems);
        
        // 逐个处理每个放置的物品
        foreach (var item in placedItems.Values)
        {
            if (item != null)
            {
                // 可以选择销毁物品或者将其放回原始位置
                Destroy(item);
            }
        }
        
        // 清空所有引用
        placedItems.Clear();
        foreach (var slot in itemSlots)
        {
            slot.placedItem = null;
        }
        
        // 触发所有移除物品事件
        foreach (var pair in itemsToRemove)
        {
            OnItemRemoved.Invoke(pair.Key, pair.Value);
        }
    }
    
    // 重写交互方法
    public override bool HandleInteraction(InteractionContext context)
    {
        // 如果玩家手上拿着物品，尝试放入盘子
        if (context.HasItem)
        {
            GameObject heldItem = context.HeldItem.gameObject;
            
            // 尝试添加物品到盘子
            if (TryAddItemToPlate(heldItem))
            {
                // 物品已添加到盘子，从玩家手中移除引用
                context.Player.SetHeldItem(null);
                return true;
            }
            
            // 如果无法添加，执行基类交互
            return base.HandleInteraction(context);
        }
        else
        {
            // 如果玩家手上没有物品，让玩家拿起盘子
            context.Player.PickUpItem(this);
            return true;
        }
    }
} 