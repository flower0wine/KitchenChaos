using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 管理盘子UI，显示盘子中包含的食物图标
/// </summary>
public class PlateUI : MonoBehaviour
{
    [Header("UI设置")]
    [Tooltip("图标模板对象")]
    public GameObject iconTemplate;
    
    [Tooltip("图标的父物体容器")]
    public Transform iconContainer;
    
    // 当前显示的图标字典
    private Dictionary<FoodInPlateType, GameObject> foodIcons = new Dictionary<FoodInPlateType, GameObject>();
    
    // 关联的盘子
    private PlateItem plateItem;
    private Camera mainCamera;
    
    private void Awake()
    {
        plateItem = GetComponentInParent<PlateItem>();
        mainCamera = Camera.main;

        if (plateItem == null)
        {
            Debug.LogError("PlateUI无法找到关联的PlateItem组件");
            return;
        }
        
        // 确保图标模板初始状态是隐藏的
        if (iconTemplate != null)
        {
            iconTemplate.SetActive(false);
            
            // 验证模板中是否有IconTemplateInfo组件
            if (iconTemplate.GetComponent<IconTemplateInfo>() == null)
            {
                Debug.LogWarning("图标模板上未找到IconTemplateInfo组件");
            }
        }
        else
        {
            Debug.LogError("未设置图标模板");
        }
        
        // 注册盘子添加/移除物品的事件
        plateItem.OnItemAdded.AddListener(AddFoodIcon);
        plateItem.OnItemRemoved.AddListener(RemoveFoodIcon);
    }

    private void LateUpdate()
    {
        // 让UI始终面向相机
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
    
    /// <summary>
    /// 为添加到盘子的食物创建图标
    /// </summary>
    private void AddFoodIcon(FoodInPlateType foodInPlateType, GameObject foodItem)
    {
        // 如果该类型食物图标已存在，先移除
        RemoveFoodIcon(foodInPlateType, foodItem);
        
        // 获取食物类型
        FoodType foodType = foodInPlateType.ToFoodType();
        // 获取食物图标信息
        Sprite foodIcon = FoodManager.Instance.GetIconForFood(foodType);
        if (foodIcon == null)
        {
            Debug.LogWarning($"未找到食物类型 {foodInPlateType} 的图标");
            return;
        }
        
        // 创建新图标
        GameObject newIcon = Instantiate(iconTemplate, iconContainer);
        newIcon.SetActive(true);
        
        // 设置图标图像
        IconTemplateInfo iconInfo = newIcon.GetComponent<IconTemplateInfo>();
        if (iconInfo != null)
        {
            iconInfo.SetIcon(foodIcon);
        }
        else
        {
            Debug.LogWarning($"在克隆的图标对象中未找到IconTemplateInfo组件，无法显示 {foodInPlateType} 的图标");
        }
        
        // 添加到字典
        foodIcons[foodInPlateType] = newIcon;
        
        // 更新布局
        UpdateIconLayout();
    }
    
    /// <summary>
    /// 移除食物图标
    /// </summary>
    private void RemoveFoodIcon(FoodInPlateType foodInPlateType, GameObject foodItem)
    {
        if (foodIcons.TryGetValue(foodInPlateType, out GameObject icon))
        {
            Destroy(icon);
            foodIcons.Remove(foodInPlateType);
            
            // 更新布局
            UpdateIconLayout();
        }
    }
    
    /// <summary>
    /// 更新图标布局
    /// </summary>
    private void UpdateIconLayout()
    {
        int index = 0;
        foreach (var icon in foodIcons.Values)
        {
            RectTransform rt = icon.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 简单的水平排列示例，根据实际需要调整
                rt.anchoredPosition = new Vector2(index * 50, 0);
                index++;
            }
        }
    }
} 