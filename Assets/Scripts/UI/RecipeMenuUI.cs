using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 菜单UI，显示所有可用的菜谱
/// </summary>
public class RecipeMenuUI : MonoBehaviour
{
    [Header("UI设置")]
    [Tooltip("菜谱项模板")]
    public GameObject recipeItemTemplate;
    
    [Tooltip("菜谱项的父容器")]
    public Transform recipeContainer;
    
    // 当前显示的菜谱项字典，使用ID作为键
    private Dictionary<string, GameObject> recipeItemsById = new Dictionary<string, GameObject>();
    
    // 当前菜谱顺序，用于保持显示顺序一致
    private List<string> currentRecipeOrder = new List<string>();
    
    // 是否已初始化
    private bool initialized = false;

    public void Awake()
    {
        if (recipeItemTemplate != null)
        {
            recipeItemTemplate.SetActive(false);
        }
    }
    
    private void Start()
    {
        // 订阅菜单更新事件
        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.OnMenuUpdated += UpdateMenuDisplay;
        }
        
        // 初始化菜单
        InitializeMenu();
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件，防止内存泄漏
        if (RecipeManager.Instance != null)
        {
            RecipeManager.Instance.OnMenuUpdated -= UpdateMenuDisplay;
        }
    }
    
    /// <summary>
    /// 初始化菜单
    /// </summary>
    public void InitializeMenu()
    {
        if (RecipeManager.Instance == null)
            return;
            
        // 获取所有菜谱
        List<Recipe> allRecipes = RecipeManager.Instance.GetAllRecipes();
        
        // 更新菜单显示
        UpdateMenuDisplay(allRecipes);
        
        initialized = true;
    }
    
    /// <summary>
    /// 响应菜单更新事件，更新整个菜单显示
    /// </summary>
    private void UpdateMenuDisplay(List<Recipe> currentRecipes)
    {
        if (currentRecipes == null)
            return;
            
        // 获取当前菜谱的ID集合，用于比对
        HashSet<string> currentRecipeIds = new HashSet<string>();
        Dictionary<string, Recipe> recipeById = new Dictionary<string, Recipe>();
        
        // 更新菜谱顺序
        List<string> newRecipeOrder = new List<string>();
        
        foreach (Recipe recipe in currentRecipes)
        {
            if (!string.IsNullOrEmpty(recipe.id))
            {
                currentRecipeIds.Add(recipe.id);
                recipeById[recipe.id] = recipe;
                newRecipeOrder.Add(recipe.id);
            }
        }
        
        // 确定需要删除的项
        List<string> idsToRemove = new List<string>();
        foreach (var id in recipeItemsById.Keys)
        {
            if (!currentRecipeIds.Contains(id))
            {
                idsToRemove.Add(id);
            }
        }
        
        // 删除不再存在的菜谱项
        foreach (string id in idsToRemove)
        {
            if (recipeItemsById.TryGetValue(id, out GameObject item))
            {
                Destroy(item);
                recipeItemsById.Remove(id);
            }
            currentRecipeOrder.Remove(id);
        }
        
        // 更新现有项和添加新项
        foreach (string recipeId in newRecipeOrder)
        {
            Recipe recipe = recipeById[recipeId];
            
            // 检查是否需要重新排序
            int newIndex = newRecipeOrder.IndexOf(recipeId);
            int currentIndex = currentRecipeOrder.IndexOf(recipeId);
            
            // 更新或添加菜谱项
            GameObject itemObj;
            bool isNewItem = false;
            
            if (recipeItemsById.TryGetValue(recipeId, out itemObj))
            {
                // 更新现有项
                RecipeItemUI itemUI = itemObj.GetComponent<RecipeItemUI>();
                if (itemUI != null)
                {
                    itemUI.SetupRecipe(recipe);
                }
            }
            else
            {
                // 创建新项
                itemObj = CreateRecipeItem(recipe);
                isNewItem = true;
            }
            
            // 重新排序（只有当顺序发生变化或是新项时）
            if ((currentIndex != newIndex && currentIndex != -1) || isNewItem)
            {
                // 设置在层级中的位置以保持顺序
                itemObj.transform.SetSiblingIndex(newIndex);
            }
        }
        
        // 更新当前顺序记录
        currentRecipeOrder = new List<string>(newRecipeOrder);
        
        // 记录已初始化
        initialized = true;
    }
    
    /// <summary>
    /// 创建新的菜谱项
    /// </summary>
    private GameObject CreateRecipeItem(Recipe recipe)
    {
        if (recipeItemTemplate == null || recipeContainer == null)
        {
            Debug.LogError("菜谱项模板或容器未设置");
            return null;
        }
        
        // 创建菜谱项
        GameObject newItem = Instantiate(recipeItemTemplate, recipeContainer);
        newItem.SetActive(true);
        
        // 配置菜谱项
        RecipeItemUI itemUI = newItem.GetComponent<RecipeItemUI>();
        if (itemUI != null)
        {
            itemUI.SetupRecipe(recipe);
        }
        else
        {
            Debug.LogWarning($"菜谱项模板上未找到RecipeItemUI组件");
        }
        
        // 添加到字典
        recipeItemsById[recipe.id] = newItem;
        
        return newItem;
    }
    
    /// <summary>
    /// 移除不再存在于当前菜谱列表中的项
    /// </summary>
    public void RemoveObsoleteRecipeItems(HashSet<string> removeRecipeIds)
    {
        if (removeRecipeIds == null || removeRecipeIds.Count == 0)
            return;
            
        // 创建待移除项的列表
        List<string> idsToRemove = new List<string>();
        
        // 查找所有需要移除的项
        foreach (string id in removeRecipeIds)
        {
            if (recipeItemsById.ContainsKey(id))
            {
                idsToRemove.Add(id);
            }
        }
        
        // 移除这些项
        foreach (string id in idsToRemove)
        {
            if (recipeItemsById.TryGetValue(id, out GameObject item))
            {
                Destroy(item);
                recipeItemsById.Remove(id);
                currentRecipeOrder.Remove(id);
            }
        }
    }
    
    /// <summary>
    /// 清除所有菜谱项
    /// </summary>
    public void ClearMenu()
    {
        foreach (var item in recipeItemsById.Values)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        
        recipeItemsById.Clear();
        currentRecipeOrder.Clear();
        initialized = false;
    }
    
    /// <summary>
    /// 显示或隐藏菜单
    /// </summary>
    public void SetMenuActive(bool active)
    {
        gameObject.SetActive(active);
        
        // 如果显示并且尚未初始化，则初始化菜单
        if (active && !initialized)
        {
            InitializeMenu();
        }
    }
    
    /// <summary>
    /// 切换菜单显示状态
    /// </summary>
    public void ToggleMenu()
    {
        SetMenuActive(!gameObject.activeSelf);
    }
} 