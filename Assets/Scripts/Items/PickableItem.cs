using UnityEngine;

/// <summary>
/// 可拾取物品的基类，处理所有拾取和放置相关的逻辑
/// </summary>
public class PickableItem : TrashableItem
{
    [Header("物品设置")]
    public float throwForce = 2f; // 丢弃时的力度
    
    [HideInInspector] public ItemContainer currentHolder = null; // 当前放置物品的容器
    
    private Rigidbody rb;
    private Collider itemCollider;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
    }

    public ItemContainer GetHoldItemContainer()
    {
        return currentHolder;
    }

    public void SetHoldItemContainer(ItemContainer itemContainer)
    {
        currentHolder = itemContainer;
    }
    
    /// <summary>
    /// 被玩家拿起时调用
    /// </summary>
    public void PickUp(Transform holdPoint)
    {
        // 设置父级为玩家的拿取点，并调整位置
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        // 禁用物理和碰撞
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        // 禁用碰撞器防止与玩家碰撞
        if (itemCollider != null)
        {
            itemCollider.enabled = false;
        }
        
        ItemContainer itemContainer = GetHoldItemContainer();
        // 清除当前持有者引用
        if (itemContainer != null)
        {
            itemContainer.SetHeldItem(null);
            SetHoldItemContainer(null);
        }
        
        isPickedUp = true;
        
        OnPickedUp();
    }

    /// <summary>
    /// 当物品被放置在容器上时调用
    /// </summary>
    public void PlaceOn(ItemContainer container)
    {
        if (container == null) return;

        // 获取放置点
        Transform placementPoint = container.GetItemHoldPoint();
        
        // 重置物体的父级，并设置位置
        transform.SetParent(placementPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        
        // 保持物理组件禁用
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // 启用碰撞器
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
        
        // 设置引用关系
        container.SetHeldItem(this);
        SetHoldItemContainer(container);

        isPickedUp = false;
        
        OnPlaced();
    }
    
    /// <summary>
    /// 当物品被丢弃时调用
    /// </summary>
    public virtual void Drop(Vector3 direction)
    {
        // 重置物品的父级
        transform.SetParent(null);
        
        // 启用物理和碰撞
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = direction * throwForce;
            rb.angularVelocity = Random.insideUnitSphere * 1f; // 一点随机旋转
        }
        
        // 启用碰撞器
        if (itemCollider != null)
        {
            itemCollider.enabled = true;
        }
        
        isPickedUp = false;
        
        OnDropped();
    }

    public override PickableItem GetPickableItem()
    {
        return this;
    }
    
    // 以下方法可由子类重写，用于添加特定物品的行为
    
    /// <summary>
    /// 当物品被拿起时触发
    /// </summary>
    protected virtual void OnPickedUp()
    {
    }
    
    /// <summary>
    /// 当物品被放置时触发
    /// </summary>
    protected virtual void OnPlaced()
    {
    }
    
    /// <summary>
    /// 当物品被丢弃时触发
    /// </summary>
    protected virtual void OnDropped()
    {
    }
} 