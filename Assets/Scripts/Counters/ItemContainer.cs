using UnityEngine;

/// <summary>
/// 标识一个可以放置物品的表面
/// 继承Interactable使其具有交互能力
/// </summary>
public class ItemContainer : Interactable
{
    [Tooltip("物品放置点")]
    public Transform itemHoldPoint;
    
    [Tooltip("放置点高度")]
    public float holdPointHeight = 0.5f;
    
    // 记录物体上当前放置的物品
    protected Interactable heldItem = null;
    
    public void Awake()
    {
        // 如果没有指定放置点，创建一个
        if (itemHoldPoint == null)
        {
            GameObject holdPointObj = new GameObject("ItemHoldPoint");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = new Vector3(0, holdPointHeight, 0); // 默认放在中央上方
            itemHoldPoint = holdPointObj.transform;
        }
    }

    // 重写交互方法
    public override bool HandleInteraction(InteractionContext context)
    {
        Interactable heldItem = GetHeldItem();
        if (heldItem != null && context.HasItem)
        {
            heldItem.HandleInteraction(context);
            return true;
        }
        
        // 如果有物品要放置，尝试接收
        if (context.HasItem)
        {
            return TryReceiveItem(context);
        }
        
        TryPickUp(context);
        
        return true;
    }

    // 当被放到可放置物品的表面上
    public virtual void PlaceOnCounter(PickableItem item)
    {
        if (item == null) return;
        
        // 重置物体的父级，并设置位置
        transform.SetParent(itemHoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        // 启用物理组件（如果有）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    public override PickableItem GetPickableItem()
    {
        return heldItem != null ? heldItem.GetComponent<PickableItem>() : null;
    }

    // 返回物品放置点，覆盖Interactable中的方法
    public Transform GetItemHoldPoint()
    {
        return itemHoldPoint;
    }

    // 获取放置在此对象上的物品（如果对象是ItemContainer）
    public Interactable GetHeldItem()
    {
        return heldItem;
    }

    // 设置放置在此对象上的物品（如果对象是ItemContainer）
    public void SetHeldItem(Interactable item)
    {
        heldItem = item;
    }

    // 重写接收物品方法
    public override bool TryReceiveItem(InteractionContext context)
    {
        // 检查容器是否已有物品
        if (heldItem != null)
        {
            return false; // 已有物品，不能接收
        }
        
        // 子类可以在这里进行额外验证
        if (!CanAcceptItem(context.HeldItem))
        {
            return false;
        }
        
        // 放置物品
        context.HeldItem.PlaceOn(this);
        context.Player.PlaceItem();
        OnPlace(context);
        return true;
    }

    public override bool TryPickUp(InteractionContext context)
    {
        if (!CanPickUp(context))
        {
            return false;
        }

        PickableItem pickableItem = GetPickableItem();
        if (pickableItem == null)
        {
            return false;
        }

        context.Player.PickUpItem(pickableItem);
        OnPickUp(context);
        return true;
    }

    // 检查是否可以接受特定物品
    public virtual bool CanAcceptItem(PickableItem item)
    {
        // 基类默认接受所有物品，子类可以重写
        return true;
    }

    public virtual bool CanPickUp(InteractionContext context)
    {
        return true;
    }

    public virtual void OnPickUp(InteractionContext context)
    {
        // 默认实现，子类可以重写
    }

    public virtual void OnPlace(InteractionContext context)
    {
        // 默认实现，子类可以重写
    }

    

} 