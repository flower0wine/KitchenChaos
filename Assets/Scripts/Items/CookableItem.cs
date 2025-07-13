using UnityEngine;

/// <summary>
/// 表示一个可烹饪的物品，继承自PickableItem
/// </summary>
public class CookableItem : PickableItem
{
    [Header("烹饪设置")]
    [Tooltip("烹饪后生成的物品预制体")]
    public GameObject cookedResultPrefab;
    
    [Tooltip("烹饪所需的时间(秒)")]
    public float cookTime = 5f;

    // 当前烹饪进度（0-1）
    private float cookingProgress = 0f;
    
    // 标记是否已烹饪完成
    private bool isCooked = false;

    /// <summary>
    /// 获取烹饪进度
    /// </summary>
    public float GetCookingProgress()
    {
        return cookingProgress;
    }
    
    /// <summary>
    /// 检查是否可以被烹饪
    /// </summary>
    public bool CanBeCookedFurther()
    {
        // 如果已经烹饪完成或者没有烹饪后的产物，则不能继续烹饪
        return !isCooked && cookedResultPrefab != null;
    }
    
    /// <summary>
    /// 更新烹饪进度
    /// </summary>
    /// <param name="deltaTime">烹饪时间增量</param>
    /// <returns>如果烹饪完成，则返回烹饪后的预制体；否则返回null</returns>
    public GameObject Cook(float deltaTime)
    {
        // 如果已经烹饪完成，不再继续烹饪
        if (isCooked)
        {
            return null;
        }
        
        // 更新烹饪进度
        cookingProgress += deltaTime / cookTime;
        
        // 检查是否烹饪完成
        if (cookingProgress >= 1f)
        {
            // 标记为已烹饪完成
            isCooked = true;
            cookingProgress = 1f;
            
            // 返回烹饪结果
            return cookedResultPrefab;
        }
        
        // 未烹饪完成，继续烹饪
        return null;
    }
    
    /// <summary>
    /// 重置烹饪进度
    /// </summary>
    public void ResetCooking()
    {
        cookingProgress = 0f;
        isCooked = false;
    }
    
    /// <summary>
    /// 检查是否已经烹饪完成
    /// </summary>
    public bool IsCooked()
    {
        return isCooked;
    }

    public bool StoveContainerCanHighlight(ItemContainer container, PlayerInteractor player)
    {
        // 玩家手上拿着可烹饪的物体，并且容器还没有物体
        if (container.GetHeldItem() == null)
        {
            return true;
        }

        if (player.GetHeldItem() != null)
        {
            PlateItem plate = player.GetHeldItem().GetComponent<PlateItem>();
            if (plate != null)
            {
                return true;
            }
        }
        return false;
    }
} 