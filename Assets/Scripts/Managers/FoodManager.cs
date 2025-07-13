using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 管理食物与其图标之间的映射关系以及食物属性
/// </summary>
public class FoodManager : MonoBehaviour
{
    // 单例实例
    public static FoodManager Instance { get; private set; }
    
    [System.Serializable]
    public class FoodMapping
    {
        [Tooltip("食物类型")]
        public FoodType foodType;
        
        [Tooltip("食物图标")]
        public Sprite iconSprite;
        
        [Tooltip("食物预制体")]
        public GameObject foodPrefab;
    }
    
    [Tooltip("食物类型到图标的映射")]
    public List<FoodMapping> foodIcons = new List<FoodMapping>();
    
    // 缓存映射关系，提高查询效率
    private Dictionary<FoodType, Sprite> iconDictionary = new Dictionary<FoodType, Sprite>();
    private Dictionary<FoodType, GameObject> prefabDictionary = new Dictionary<FoodType, GameObject>();
    
    private void Awake()
    {
        // 单例模式实现
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // 初始化映射字典
        InitializeMapping();
    }
    
    private void InitializeMapping()
    {
        iconDictionary.Clear();
        prefabDictionary.Clear();
        
        foreach (var mapping in foodIcons)
        {
            if (mapping.iconSprite != null)
            {
                iconDictionary[mapping.foodType] = mapping.iconSprite;
            }
            
            if (mapping.foodPrefab != null)
            {
                prefabDictionary[mapping.foodType] = mapping.foodPrefab;
            }
            
        }
    }
    
    /// <summary>
    /// 获取指定食物类型的图标
    /// </summary>
    public Sprite GetIconForFood(FoodType foodType)
    {
        if (iconDictionary.TryGetValue(foodType, out Sprite icon))
        {
            return icon;
        }
        
        Debug.LogWarning($"未找到食物类型 {foodType} 的图标");
        return null;
    }
    
    /// <summary>
    /// 获取指定食物类型的预制体
    /// </summary>
    public GameObject GetPrefabForFood(FoodType foodType)
    {
        if (prefabDictionary.TryGetValue(foodType, out GameObject prefab))
        {
            return prefab;
        }
        
        Debug.LogWarning($"未找到食物类型 {foodType} 的预制体");
        return null;
    }
    
    /// <summary>
    /// 动态添加映射关系
    /// </summary>
    public void AddMapping(FoodType foodType, Sprite icon, GameObject prefab = null)
    {
        iconDictionary[foodType] = icon;
        
        if (prefab != null)
        {
            prefabDictionary[foodType] = prefab;
        }
        
        // 可选：更新Inspector中的列表，方便编辑时查看
        bool found = false;
        foreach (var mapping in foodIcons)
        {
            if (mapping.foodType == foodType)
            {
                mapping.iconSprite = icon;
                if (prefab != null) mapping.foodPrefab = prefab;
                found = true;
                break;
            }
        }
        
        if (!found)
        {
            foodIcons.Add(new FoodMapping
            {
                foodType = foodType,
                iconSprite = icon,
                foodPrefab = prefab,
            });
        }
    }
}