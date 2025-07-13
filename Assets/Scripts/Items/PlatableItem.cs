using UnityEngine;

/// <summary>
/// 标识一个可以放入盘子的物品
/// </summary>
public class PlatableItem : PickableItem
{
    [Header("盘子相关设置")]
    public FoodInPlateType foodInPlateType;  // 使用枚举替代字符串
    
    // 当前放置此物品的盘子
    private PlateItem currentPlate;
    
    /// <summary>
    /// 获取物品类型标识符
    /// </summary>
    public FoodInPlateType GetFoodInPlateType()
    {
        return foodInPlateType;
    }
    
    /// <summary>
    /// 当物品被放置到盘子上时调用
    /// </summary>
    public virtual void OnPlacedOnPlate(PlateItem plate, Transform slotTransform)
    {
        currentPlate = plate;
        
        // 标记物品状态 - 可以基于需要添加更多操作
        Debug.Log($"{foodInPlateType}被放到了盘子上");
    }
    
    /// <summary>
    /// 当物品从盘子上移除时调用
    /// </summary>
    public virtual void OnRemovedFromPlate(PlateItem plate)
    {
        currentPlate = null;
        
        // 重新启用物理组件
        Collider collider = GetComponent<Collider>();
        if (collider) collider.enabled = true;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;
    }
    
    // 重写交互方法，实现与盘子的交互
    public override bool HandleInteraction(InteractionContext context)
    {
        // 如果玩家手上拿的是盘子，尝试将自己放入盘子
        if (context.HasItem)
        {
            PlateItem plate = context.HeldItem.GetComponent<PlateItem>();
            if (plate != null)
            {
                // 尝试添加到盘子
                if (plate.TryAddItemToPlate(gameObject))
                {
                    if (GetHoldItemContainer() != null)
                    {
                        // 清除台面上的物体引用
                        GetHoldItemContainer().SetHeldItem(null);
                        // 清除物体的台面引用·
                        SetHoldItemContainer(null);
                    }
                    return true;
                }
            }
        }
        
        // 如果不是与盘子的交互，执行默认行为
        return base.HandleInteraction(context);
    }
}