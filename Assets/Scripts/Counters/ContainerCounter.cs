using UnityEngine;

/// <summary>
/// 容器台面，用于在玩家交互时生成并给予指定物品
/// </summary>
public class ContainerCounter : Interactable
{
    [Header("物品配置")]
    [Tooltip("此容器提供的食物类型")]
    public FoodType foodType;
    
    [Tooltip("图标精灵渲染器组件")]
    public SpriteRenderer iconRenderer;
    
    [Tooltip("打开动画的触发器名称")]
    public string openAnimTriggerName = "OpenClose";
    
    // 内部变量
    private bool isOpen = false;
    private float lastOpenTime = -10f;
    private Animator animator;
    private GameObject itemPrefab;
    
    public void Awake()
    {
        // 获取动画控制器
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            // 如果本身没有，尝试查找子物体上的Animator
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"容器 {gameObject.name} 没有找到动画控制器");
            }
        }
        
        // 初始化食物预制体和图标
        InitializeFoodData();
    }
    
    /// <summary>
    /// 从FoodManager初始化食物数据
    /// </summary>
    private void InitializeFoodData()
    {
        if (FoodManager.Instance == null)
        {
            Debug.LogError("FoodManager实例未找到！请确保场景中存在FoodManager");
            return;
        }
        
        // 获取食物预制体
        itemPrefab = FoodManager.Instance.GetPrefabForFood(foodType);
        if (itemPrefab == null)
        {
            Debug.LogError($"未找到食物类型 {foodType} 的预制体！");
        }
        
        // 设置图标
        if (iconRenderer != null)
        {
            Sprite foodIcon = FoodManager.Instance.GetIconForFood(foodType);
            if (foodIcon != null)
            {
                iconRenderer.sprite = foodIcon;
            }
            else
            {
                Debug.LogWarning($"未找到食物类型 {foodType} 的图标！");
            }
        }
        else
        {
            Debug.LogWarning("未设置图标渲染组件！");
        }
    }
    
    // 重写交互方法
    public override bool HandleInteraction(InteractionContext context)
    {
        // 检查玩家是否有空手且容器未处于冷却状态
        if (context.Player != null && context.HeldItem == null && !isOpen && Time.time > lastOpenTime + 2f)
        {
            // 检查是否有物品预制体
            if (itemPrefab == null)
            {
                Debug.LogError("没有可用的物品预制体！");
                return true;
            }
            
            // 标记容器为打开状态
            isOpen = true;
            lastOpenTime = Time.time;
            
            // 播放动画
            PlayOpenAnimation();
            
            // 生成物品
            Invoke(nameof(SpawnItem), 0.3f);
            return true;
        }
        else if (context.Player != null && context.HeldItem != null)
        {
            return false; // 未处理交互，可能会丢弃物品
        }
        
        return true; // 默认返回已处理
    }
    
    // 播放打开动画
    private void PlayOpenAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(openAnimTriggerName);
        }
    }
    
    // 生成物品并给予玩家
    private void SpawnItem()
    {
        // 获取玩家
        PlayerInteractor player = FindObjectOfType<PlayerInteractor>();
        if (player == null)
        {
            Debug.LogWarning("无法找到玩家来给予物品！");
            isOpen = false;
            return;
        }
        
        // 实例化物品（直接在容器位置生成，因为会立即给到玩家）
        GameObject itemObj = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        
        // 获取物品组件
        PickableItem item = itemObj.GetComponent<PickableItem>();
        if (item == null)
        {
            Debug.LogError("生成的物品没有PickableItem组件！");
            Destroy(itemObj);
            isOpen = false;
            return;
        }
        
        // 让玩家拾取物品
        player.PickUpItem(item);
        
        // 重置状态
        isOpen = false;
    }
    
    // 由动画事件调用的方法
    public void AnimationEventSpawnItem()
    {
        // 这个方法可以在动画中的特定帧通过动画事件调用
        // 如果想要在动画中间生成物品，可以调用此方法而不是使用Invoke延迟
        CancelInvoke(nameof(SpawnItem));
        SpawnItem();
    }

    public override bool CanHighlight(PlayerInteractor player)
    {
        if (player.GetHeldItem() == null)
        {
            return true;
        }
        return false;
    }
} 