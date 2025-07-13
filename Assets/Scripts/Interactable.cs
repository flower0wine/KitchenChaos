using UnityEngine;
using System.Collections.Generic;

public class Interactable : MonoBehaviour
{
    [Header("选择效果设置")]
    public GameObject selectObject; // 选择效果物体
    
    public bool isPickedUp = false;
    
    void Start()
    {
        // 确保选择效果物体默认是禁用的
        if (selectObject != null)
        {
            selectObject.SetActive(false);
        }
    }
    
    // 添加检查是否可拾取的方法
    public virtual bool IsPickable()
    {
        return GetComponent<PickableItem>() != null;
    }
    
    // 添加检查是否可放置物品的方法
    public virtual bool CanHoldItems()
    {
        return GetComponent<ItemContainer>() != null;
    }
    
    // 获取对象上的ItemContainer组件（如果有）
    public ItemContainer GetItemContainer()
    {
        return GetComponent<ItemContainer>();
    }

    // 使用Select物体的高亮方法
    public void SetHighlight(bool isHighlighted)
    {
        if (selectObject != null)
        {
            selectObject.SetActive(isHighlighted);
        }
    }

    public virtual bool CanHighlight(PlayerInteractor player)
    {
        return true;
    }

    /// <summary>
    /// 处理交互操作，返回是否已处理
    /// </summary>
    public virtual bool HandleInteraction(InteractionContext context)
    {
        // 默认实现根据持有物品状态选择不同的处理方式
        if (context.HasItem)
        {
            // 尝试接收物品
            return TryReceiveItem(context);
        }
        else
        {
            // 尝试拾取物品或执行交互
            PickableItem pickable = GetPickableItem();
            if (pickable != null)
            {
                context.Player.PickUpItem(pickable);
                return true;
            }
            
            // 如果没有可拾取物品，执行一般交互
            OnInteract(context.Player);
            return true;
        }
    }

    /// <summary>
    /// 执行一般交互操作
    /// </summary>
    protected virtual void OnInteract(PlayerInteractor player)
    {
        // 默认实现，子类可以重写
        Debug.Log($"与 {gameObject.name} 交互");
    }

    /// <summary>
    /// 尝试接收物品
    /// </summary>
    public virtual bool TryReceiveItem(InteractionContext context)
    {
        // 默认实现，无法接收物品
        return false;
    }

    /// <summary>
    /// 尝试拾取物品
    /// </summary>
    public virtual bool TryPickUp(InteractionContext context)
    {
        return false;
    }

    /// <summary>
    /// 处理切割操作
    /// </summary>
    public virtual void HandleCutting(PlayerInteractor player)
    {
        // 默认实现，不处理切割
    }

    /// <summary>
    /// 获取可拾取的物品
    /// </summary>
    /// <returns>可拾取的物品，如果不可拾取则返回null</returns>
    public virtual PickableItem GetPickableItem()
    {
        return null;
    }

    /// <summary>
    /// 处理与另一个交互物体的交互
    /// </summary>
    /// <param name="player">玩家引用</param>
    /// <param name="otherInteractable">另一个交互物体</param>
    /// <returns>是否成功处理交互</returns>
    public virtual bool HandleInteractionWithOther(PlayerInteractor player, Interactable otherInteractable)
    {
        // 基类默认实现，返回false表示未处理
        return false;
    }

    // 确保在禁用时关闭选择效果
    private void OnDisable()
    {
        if (selectObject != null)
        {
            selectObject.SetActive(false);
        }
    }
}