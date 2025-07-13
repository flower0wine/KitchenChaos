using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 管理所有可用的菜谱
/// </summary>
public class RecipeManager : MonoBehaviour
{
    // 单例实例
    public static RecipeManager Instance { get; private set; }
    
    [Header("菜谱设置")]
    [Tooltip("所有可用的菜谱列表")]
    public List<Recipe> availableRecipes = new List<Recipe>();
    
    [Tooltip("菜单中最大菜谱数量")]
    public int maxMenuSize = 6;
    
    [Tooltip("添加新菜谱的最短时间(秒)")]
    public float minAddTime = 3f;
    
    [Tooltip("添加新菜谱的最长时间(秒)")]
    public float maxAddTime = 12f;
    
    // 使用字典来缓存ID到菜谱的映射
    private Dictionary<string, Recipe> recipeIdMap = new Dictionary<string, Recipe>();
    
    // 当前活跃的菜单菜谱
    private List<Recipe> activeMenuRecipes = new List<Recipe>();
    
    // 可用但不在当前菜单中的菜谱
    private List<Recipe> inactiveRecipes = new List<Recipe>();
    
    // 计时器
    private float timer = 0f;
    
    // 下次添加菜谱的时间
    private float nextAddTime = 0f;
    
    // 菜单更新事件
    public event Action<List<Recipe>> OnMenuUpdated;
    
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
        
        // 初始化一个包含所有可能菜谱的主列表
        InitializeAllPossibleRecipes();
        
        // 更新ID映射
        UpdateRecipeIdMap();
        
        // 开始时所有菜谱都是非活跃的
        inactiveRecipes = new List<Recipe>(availableRecipes);
        activeMenuRecipes.Clear();
        
        // 清空可用菜谱列表，将通过动态系统填充
        availableRecipes.Clear();
        
        // 重置计时器
        ResetAddTimer();
    }
    
    private void Start()
    {
        AddRandomRecipeToMenu();
    }
    
    private void Update()
    {
        if (!GameManager.Instance.CanRunning()) return;
        
        // 更新计时器
        timer += Time.deltaTime;
        
        // 检查是否应该添加新菜谱
        if (timer >= nextAddTime && availableRecipes.Count < maxMenuSize)
        {
            AddRandomRecipeToMenu();
            ResetAddTimer();
        }
    }
    
    /// <summary>
    /// 添加随机菜谱到菜单
    /// </summary>
    private void AddRandomRecipeToMenu()
    {
        // 如果没有非活跃菜谱或菜单已满，则返回
        if (inactiveRecipes.Count == 0 || availableRecipes.Count >= maxMenuSize)
            return;
            
        // 随机选择一个菜谱
        int randomIndex = UnityEngine.Random.Range(0, inactiveRecipes.Count);
        Recipe selectedRecipe = inactiveRecipes[randomIndex];
        
        // 从非活跃列表中移除
        inactiveRecipes.RemoveAt(randomIndex);
        
        // 添加到活跃菜单和可用菜谱列表
        activeMenuRecipes.Add(selectedRecipe);
        availableRecipes.Add(selectedRecipe);
        
        // 确保菜谱ID已映射
        if (!string.IsNullOrEmpty(selectedRecipe.id))
        {
            recipeIdMap[selectedRecipe.id] = selectedRecipe;
        }
        
        // 触发菜单更新事件
        OnMenuUpdated?.Invoke(new List<Recipe>(availableRecipes));
    }
    
    /// <summary>
    /// 重置添加计时器
    /// </summary>
    private void ResetAddTimer()
    {
        // 根据菜单大小计算下次添加时间
        // 菜单越小，添加时间越短
        float menuFillRatio = (float)availableRecipes.Count / maxMenuSize;
        nextAddTime = Mathf.Lerp(minAddTime, maxAddTime, menuFillRatio);
        
        // 重置计时器
        timer = 0f;
    }
    
    /// <summary>
    /// 根据提供的食材列表查找匹配的菜谱
    /// </summary>
    public Recipe FindMatchingRecipe(List<FoodInPlateType> ingredients)
    {
        foreach (Recipe recipe in availableRecipes)
        {
            if (recipe.CheckRecipe(ingredients))
            {
                return recipe;
            }
        }
        
        return null; // 没有找到匹配的菜谱
    }
    
    /// <summary>
    /// 获取所有可用的菜谱
    /// </summary>
    public List<Recipe> GetAllRecipes()
    {
        return new List<Recipe>(availableRecipes);
    }
    
    /// <summary>
    /// 在运行时添加新菜谱
    /// </summary>
    public Recipe AddRecipe(Recipe recipe)
    {
        // 确保菜谱有唯一ID
        if (string.IsNullOrEmpty(recipe.id))
        {
            recipe.id = Guid.NewGuid().ToString();
        }
        
        if (!activeMenuRecipes.Contains(recipe))
        {
            activeMenuRecipes.Add(recipe);
        }
        
        // 添加到可用菜谱列表
        availableRecipes.Add(recipe);
        
        // 更新ID映射
        recipeIdMap[recipe.id] = recipe;
        
        return recipe;
    }
    
    /// <summary>
    /// 检查盘子上的食物是否符合任何菜谱
    /// </summary>
    public Recipe CheckPlateForRecipe(PlateItem plate)
    {
        if (plate == null) return null;
        
        // 收集盘子上的所有食材
        List<FoodInPlateType> plateIngredients = new List<FoodInPlateType>();
        foreach (var item in plate.GetAllPlacedItems())
        {
            plateIngredients.Add(item.Key);
        }
        
        // 查找匹配的菜谱
        return FindMatchingRecipe(plateIngredients);
    }
    
    /// <summary>
    /// 根据ID查找菜谱
    /// </summary>
    public Recipe GetRecipeById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;
        
        // 确保ID映射是最新的
        UpdateRecipeIdMap();
        
        if (recipeIdMap.TryGetValue(id, out Recipe recipe))
        {
            return recipe;
        }
        
        return null;
    }
    
    /// <summary>
    /// 从可用菜谱列表中移除特定菜谱
    /// </summary>
    /// <param name="recipe">要移除的菜谱</param>
    /// <returns>是否成功移除</returns>
    public bool RemoveRecipe(Recipe recipe)
    {
        if (recipe == null)
        {
            return false;
        }
        
        // 从列表中移除菜谱
        bool result = availableRecipes.Remove(recipe);
        
        if (result)
        {
            // 确保从ID映射中也移除
            recipeIdMap.Remove(recipe.id);
            
            activeMenuRecipes.Remove(recipe);
            
            // 只有在非活跃列表中不包含此菜谱时才添加
            if (!inactiveRecipes.Contains(recipe))
            {
                inactiveRecipes.Add(recipe);
            }
            
            // 触发菜单更新事件
            OnMenuUpdated?.Invoke(new List<Recipe>(availableRecipes));
        }
        
        return result;
    }
    
    /// <summary>
    /// 根据ID移除菜谱
    /// </summary>
    public bool RemoveRecipeById(string id)
    {
        Recipe recipe = GetRecipeById(id);
        if (recipe != null)
        {
            return RemoveRecipe(recipe);
        }
        return false;
    }
    
    /// <summary>
    /// 重置所有菜谱（用于游戏重新开始）
    /// </summary>
    public void ResetRecipes()
    {
        // 清空活跃菜谱和可用菜谱列表
        activeMenuRecipes.Clear();
        availableRecipes.Clear();
        
        // 重新初始化非活跃菜谱列表
        InitializeAllPossibleRecipes();
        inactiveRecipes = new List<Recipe>(availableRecipes);
        availableRecipes.Clear();
        
        // 更新ID映射
        UpdateRecipeIdMap();
        
        // 重置计时器
        ResetAddTimer();
        
        // 添加随机菜谱开始
        AddRandomRecipeToMenu();
        
        // 触发菜单更新事件
        OnMenuUpdated?.Invoke(new List<Recipe>(availableRecipes));
    }
    
    /// <summary>
    /// 更新ID到菜谱的映射
    /// </summary>
    private void UpdateRecipeIdMap()
    {
        recipeIdMap.Clear();
        
        // 映射活跃菜谱
        foreach (var recipe in activeMenuRecipes)
        {
            if (!string.IsNullOrEmpty(recipe.id))
            {
                recipeIdMap[recipe.id] = recipe;
            }
        }
        
        // 映射非活跃菜谱
        foreach (var recipe in inactiveRecipes)
        {
            if (!string.IsNullOrEmpty(recipe.id))
            {
                recipeIdMap[recipe.id] = recipe;
            }
        }
    }
    
    /// <summary>
    /// 初始化所有可能的菜谱
    /// </summary>
    private void InitializeAllPossibleRecipes()
    {
        // 清空现有菜谱
        availableRecipes.Clear();
        
        #region 基础三明治类
        // 1. 奶酪三明治 - 简单
        Recipe cheeseSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "经典奶酪三明治",
            description = "松软的面包配上浓郁的奶酪，简单却美味。",
            price = 8
        };
        cheeseSandwich.ingredients.Add(FoodInPlateType.Bread);
        cheeseSandwich.ingredients.Add(FoodInPlateType.CheeseSlice);
        availableRecipes.Add(cheeseSandwich);
        
        // 2. 番茄三明治 - 简单
        Recipe tomatoSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "清新番茄三明治",
            description = "新鲜的番茄与面包的搭配，口感清爽。",
            price = 7
        };
        tomatoSandwich.ingredients.Add(FoodInPlateType.Bread);
        tomatoSandwich.ingredients.Add(FoodInPlateType.TomatoSlice);
        availableRecipes.Add(tomatoSandwich);
        
        // 3. 蔬菜三明治 - 简单
        Recipe veggieSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "绿色蔬菜三明治",
            description = "新鲜卷心菜与面包，健康又美味。",
            price = 7
        };
        veggieSandwich.ingredients.Add(FoodInPlateType.Bread);
        veggieSandwich.ingredients.Add(FoodInPlateType.CabbageSlice);
        availableRecipes.Add(veggieSandwich);
        
        // 4. 熟肉三明治 - 简单
        Recipe meatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "香烤肉片三明治",
            description = "多汁的肉片与面包，让你满足感倍增。",
            price = 10
        };
        meatSandwich.ingredients.Add(FoodInPlateType.Bread);
        meatSandwich.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(meatSandwich);
        
        // 5. 焦肉三明治 - 简单
        Recipe burntMeatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "炭烧风味三明治",
            description = "略带焦香的肉片与面包，独特的风味。",
            price = 9
        };
        burntMeatSandwich.ingredients.Add(FoodInPlateType.Bread);
        burntMeatSandwich.ingredients.Add(FoodInPlateType.BurnedMeat);
        availableRecipes.Add(burntMeatSandwich);
        #endregion
        
        #region 双料三明治类
        // 6. 奶酪番茄三明治 - 中等
        Recipe cheeseTomatoSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "奶酪番茄三明治",
            description = "奶酪的香浓与番茄的清新，经典搭配。",
            price = 12
        };
        cheeseTomatoSandwich.ingredients.Add(FoodInPlateType.Bread);
        cheeseTomatoSandwich.ingredients.Add(FoodInPlateType.CheeseSlice);
        cheeseTomatoSandwich.ingredients.Add(FoodInPlateType.TomatoSlice);
        availableRecipes.Add(cheeseTomatoSandwich);
        
        // 7. 奶酪蔬菜三明治 - 中等
        Recipe cheeseVeggieSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "奶酪蔬菜三明治",
            description = "奶酪与卷心菜的组合，营养又美味。",
            price = 11
        };
        cheeseVeggieSandwich.ingredients.Add(FoodInPlateType.Bread);
        cheeseVeggieSandwich.ingredients.Add(FoodInPlateType.CheeseSlice);
        cheeseVeggieSandwich.ingredients.Add(FoodInPlateType.CabbageSlice);
        availableRecipes.Add(cheeseVeggieSandwich);
        
        // 8. 奶酪肉三明治 - 中等
        Recipe cheeseMeatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "奶酪烤肉三明治",
            description = "浓郁奶酪搭配多汁肉片，满足你的味蕾。",
            price = 15
        };
        cheeseMeatSandwich.ingredients.Add(FoodInPlateType.Bread);
        cheeseMeatSandwich.ingredients.Add(FoodInPlateType.CheeseSlice);
        cheeseMeatSandwich.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(cheeseMeatSandwich);
        
        // 9. 番茄蔬菜三明治 - 中等
        Recipe tomatoVeggieSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "田园蔬菜三明治",
            description = "番茄与卷心菜的清新组合，健康首选。",
            price = 10
        };
        tomatoVeggieSandwich.ingredients.Add(FoodInPlateType.Bread);
        tomatoVeggieSandwich.ingredients.Add(FoodInPlateType.TomatoSlice);
        tomatoVeggieSandwich.ingredients.Add(FoodInPlateType.CabbageSlice);
        availableRecipes.Add(tomatoVeggieSandwich);
        
        // 10. 番茄肉三明治 - 中等
        Recipe tomatoMeatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "番茄烤肉三明治",
            description = "番茄的酸甜与肉的咸香，层次丰富。",
            price = 14
        };
        tomatoMeatSandwich.ingredients.Add(FoodInPlateType.Bread);
        tomatoMeatSandwich.ingredients.Add(FoodInPlateType.TomatoSlice);
        tomatoMeatSandwich.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(tomatoMeatSandwich);
        
        // 11. 蔬菜肉三明治 - 中等
        Recipe veggieMeatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "蔬菜烤肉三明治",
            description = "蔬菜的清脆与肉的嫩滑，口感丰富。",
            price = 13
        };
        veggieMeatSandwich.ingredients.Add(FoodInPlateType.Bread);
        veggieMeatSandwich.ingredients.Add(FoodInPlateType.CabbageSlice);
        veggieMeatSandwich.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(veggieMeatSandwich);
        #endregion
        
        #region 三料汉堡/三明治类
        // 12. 奶酪番茄肉三明治 - 高级
        Recipe cheeseTomatoMeatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "超级奶酪肉三明治",
            description = "奶酪、番茄与肉的奢华组合，满足感十足。",
            price = 18
        };
        cheeseTomatoMeatSandwich.ingredients.Add(FoodInPlateType.Bread);
        cheeseTomatoMeatSandwich.ingredients.Add(FoodInPlateType.CheeseSlice);
        cheeseTomatoMeatSandwich.ingredients.Add(FoodInPlateType.TomatoSlice);
        cheeseTomatoMeatSandwich.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(cheeseTomatoMeatSandwich);
        
        // 13. 奶酪蔬菜肉三明治 - 高级
        Recipe cheeseVeggieMeatSandwich = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "农场风味全料三明治",
            description = "奶酪、蔬菜与肉的完美平衡，丰富又营养。",
            price = 17
        };
        cheeseVeggieMeatSandwich.ingredients.Add(FoodInPlateType.Bread);
        cheeseVeggieMeatSandwich.ingredients.Add(FoodInPlateType.CheeseSlice);
        cheeseVeggieMeatSandwich.ingredients.Add(FoodInPlateType.CabbageSlice);
        cheeseVeggieMeatSandwich.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(cheeseVeggieMeatSandwich);
        
        // 14. 经典汉堡 - 高级
        Recipe classicBurger = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "经典全配汉堡",
            description = "番茄、卷心菜与肉饼的经典组合，永不过时。",
            price = 16
        };
        classicBurger.ingredients.Add(FoodInPlateType.Bread);
        classicBurger.ingredients.Add(FoodInPlateType.TomatoSlice);
        classicBurger.ingredients.Add(FoodInPlateType.CabbageSlice);
        classicBurger.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(classicBurger);
        
        // 15. 豪华芝士汉堡 - 豪华
        Recipe deluxeCheeseBurger = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "豪华芝士皇堡",
            description = "奶酪、番茄、卷心菜与肉饼，汉堡中的极致体验。",
            price = 20
        };
        deluxeCheeseBurger.ingredients.Add(FoodInPlateType.Bread);
        deluxeCheeseBurger.ingredients.Add(FoodInPlateType.CheeseSlice);
        deluxeCheeseBurger.ingredients.Add(FoodInPlateType.TomatoSlice);
        deluxeCheeseBurger.ingredients.Add(FoodInPlateType.CabbageSlice);
        deluxeCheeseBurger.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(deluxeCheeseBurger);
        #endregion
        
        #region 特殊组合
        // 16. 双层肉汉堡 - 豪华
        Recipe doubleMeatBurger = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "双层肉汉堡巨无霸",
            description = "两层肉饼搭配蔬菜，专为饥饿的你准备。",
            price = 22
        };
        doubleMeatBurger.ingredients.Add(FoodInPlateType.Bread);
        doubleMeatBurger.ingredients.Add(FoodInPlateType.CookedMeat);
        doubleMeatBurger.ingredients.Add(FoodInPlateType.CabbageSlice);
        availableRecipes.Add(doubleMeatBurger);
        
        // 17. 双层芝士汉堡 - 豪华
        Recipe doubleCheeseburger = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "双层芝士汉堡",
            description = "双倍奶酪的浓郁口感，让你回味无穷。",
            price = 16
        };
        doubleCheeseburger.ingredients.Add(FoodInPlateType.Bread);
        doubleCheeseburger.ingredients.Add(FoodInPlateType.CheeseSlice);
        doubleCheeseburger.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(doubleCheeseburger);
        
        // 18. 素食者的梦想 - 特殊
        Recipe vegetarianDelight = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "素食者的梦想",
            description = "完美的素食组合，营养均衡且美味。",
            price = 14
        };
        vegetarianDelight.ingredients.Add(FoodInPlateType.Bread);
        vegetarianDelight.ingredients.Add(FoodInPlateType.CheeseSlice);
        vegetarianDelight.ingredients.Add(FoodInPlateType.TomatoSlice);
        vegetarianDelight.ingredients.Add(FoodInPlateType.CabbageSlice);
        availableRecipes.Add(vegetarianDelight);
        
        // 19. 烤火慢煮 - 特殊
        Recipe burntSpecial = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "炭烧风味特餐",
            description = "焦香的肉片与各种配料的组合，独特的风味体验。",
            price = 16
        };
        burntSpecial.ingredients.Add(FoodInPlateType.Bread);
        burntSpecial.ingredients.Add(FoodInPlateType.BurnedMeat);
        burntSpecial.ingredients.Add(FoodInPlateType.CheeseSlice);
        burntSpecial.ingredients.Add(FoodInPlateType.TomatoSlice);
        availableRecipes.Add(burntSpecial);
        
        // 20. 超级豪华满汉全席 - 终极
        Recipe ultimateBurger = new Recipe
        {
            id = Guid.NewGuid().ToString(),
            recipeName = "厨师长特制全配汉堡",
            description = "我们能提供的所有食材，堆成的一座美食高塔。",
            price = 25
        };
        ultimateBurger.ingredients.Add(FoodInPlateType.Bread);
        ultimateBurger.ingredients.Add(FoodInPlateType.CheeseSlice);
        ultimateBurger.ingredients.Add(FoodInPlateType.TomatoSlice);
        ultimateBurger.ingredients.Add(FoodInPlateType.CabbageSlice);
        ultimateBurger.ingredients.Add(FoodInPlateType.CookedMeat);
        availableRecipes.Add(ultimateBurger);
        #endregion
    }
    
}