using UnityEngine;
using UnityEngine.InputSystem; // 添加新输入系统命名空间
using System.Collections.Generic;

public class PlayerInteractor : MonoBehaviour
{
    [Header("检测设置")]
    public float interactRange = 1f;    // 检测半径
    public LayerMask interactableLayer; // 可交互物品的层级

    [Header("拾取设置")]
    public Transform holdPoint; // 物品拿取点

    [Header("音效设置")]
    [Tooltip("物品放置音效")]
    public AudioClip placeItemSound;
    
    [Tooltip("物品拾取音效")]
    public AudioClip pickupItemSound;
    
    private Interactable closestInteractable; // 当前选中的物品
    private PickableItem heldItem; // 当前拿着的物品

    // 音频源
    private AudioSource audioSource;
    
    void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (placeItemSound != null || pickupItemSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 确保有拿取点
        if (holdPoint == null)
        {
            // 如果未指定，创建一个拿取点
            GameObject holdPointObj = new GameObject("HoldPoint");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = new Vector3(0, 1.5f, 1f); // 在玩家前方和头部位置
            holdPoint = holdPointObj.transform;
        }
        
        // 订阅交互事件
        InputManager.Instance.OnInteractPerformed += HandleInteraction;
        InputManager.Instance.OnCutPerformed += HandleCutting;
    }

    void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnInteractPerformed -= HandleInteraction;
            InputManager.Instance.OnCutPerformed -= HandleCutting; // 取消订阅切菜事件
        }
    }

    void Update()
    {
        if (!GameManager.Instance.CanRunning())
        {
            closestInteractable = null;
            return;
        }

        // 检测附近可交互物品
        FindClosestInteractable();
    }
    
    // 处理交互输入 - 简化版
    void HandleInteraction()
    {
        if (closestInteractable == null) return;
        
        // 简单地将交互委托给物体，传递所需的上下文
        InteractionContext context = new InteractionContext(
            this,               // 交互的玩家
            heldItem,           // 玩家持有的物品
            closestInteractable // 被交互的物体
        );
        
        // 让交互对象负责处理交互逻辑
        bool interactionHandled = closestInteractable.HandleInteraction(context);
        
        // 如果玩家手上有物品但交互未被处理，丢弃物品
        if (heldItem != null && !interactionHandled)
        {
            Debug.Log("交互未处理");
            // DropItem();
        }
    }
    
    // 处理切割输入 - 简化版
    void HandleCutting()
    {
        if (closestInteractable == null) return;

        CuttingContainer cuttingContainer = closestInteractable.GetComponent<CuttingContainer>();
        if (cuttingContainer != null)
        {
            cuttingContainer.HandleCutting(this);
        }
    }
    
    // 简化版的 FindClosestInteractable 方法
    void FindClosestInteractable()
    {
        // 获取范围内的所有可交互物品
        Collider[] colliders = Physics.OverlapSphere(transform.position, interactRange, interactableLayer);
        
        // 先重置上一个选中的物品的高亮
        if (closestInteractable != null)
        {
            closestInteractable.SetHighlight(false);
            closestInteractable = null;
        }
        
        // 查找最近的可交互物体
        float closestDistance = Mathf.Infinity;
        
        foreach (Collider col in colliders)
        {
            Interactable interactable = col.GetComponent<Interactable>();
            
            // 跳过无效物体
            if (interactable == null) continue;
            
            // 跳过当前手持的物品
            PickableItem pickable = interactable.GetComponent<PickableItem>();
            if (pickable != null && pickable == heldItem) continue;
            
            // 计算距离
            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            
            // 如果是最近的，更新引用
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }
        
        // 高亮选中的物体
        if (closestInteractable != null && closestInteractable.CanHighlight(this))
        {
            closestInteractable.SetHighlight(true);
        }
    }

    // 可视化检测范围（编辑器调试）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
        
        // 如果有拿取点，显示它的位置
        if (holdPoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(holdPoint.position, 0.1f);
        }
    }

    // 获取当前持有的物品
    public PickableItem GetHeldItem() 
    {
        return heldItem;
    }

    public void SetHeldItem(PickableItem item)
    {
        heldItem = item;
    }

    // 判断是否能拿起新物品
    public bool CanPickUpItem()
    {
        return heldItem == null;
    }

    // 玩家拿起物品
    public void PickUpItem(PickableItem item)
    {
        if (item == null || heldItem != null) return;
        
        SetHeldItem(item);
        item.PickUp(holdPoint);

        PlayPickupSound();
    }

    public void PlaceItem()
    {
        SetHeldItem(null);

        PlayPlaceSound();
    }

    /// <summary>
    /// 播放物品放置音效
    /// </summary>
    private void PlayPlaceSound()
    {
        if (audioSource != null && placeItemSound != null)
        {
            audioSource.PlayOneShot(placeItemSound);
        }
    }
    
    /// <summary>
    /// 播放物品拾取音效
    /// </summary>
    private void PlayPickupSound()
    {
        if (audioSource != null && pickupItemSound != null)
        {
            audioSource.PlayOneShot(pickupItemSound);
        }
    }

    // 移除物品引用
    public void DestroyHeldItem()
    {
        if (heldItem == null) return;
        
        Destroy(heldItem.gameObject);
        
        heldItem = null;
    }
}

