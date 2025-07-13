using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 定义一道菜的配方和属性
/// </summary>
[System.Serializable]
public class Recipe
{
    [Tooltip("菜品名称")]
    public string recipeName;
    
    [Tooltip("菜品描述")]
    [TextArea(2, 5)]
    public string description;
    
    [Tooltip("菜品图标")]
    public Sprite recipeIcon;
    
    [Tooltip("售价")]
    public int price = 10;
    
    [Tooltip("组成材料")]
    public List<FoodInPlateType> ingredients = new List<FoodInPlateType>();
    
    [HideInInspector]
    public string id;

    // 构造函数，自动生成ID
    public Recipe()
    {
        id = Guid.NewGuid().ToString();
    }
    
    /// <summary>
    /// 检查提供的食物类型列表是否匹配此菜谱
    /// </summary>
    public bool CheckRecipe(List<FoodInPlateType> providedIngredients)
    {
        // 数量必须一致
        if (providedIngredients.Count != ingredients.Count)
        {
            return false;
        }
        
        // 创建食材副本进行排序比较
        List<FoodInPlateType> sortedProvided = new List<FoodInPlateType>(providedIngredients);
        List<FoodInPlateType> sortedRecipe = new List<FoodInPlateType>(ingredients);
        
        // 对两个列表进行排序
        sortedProvided.Sort();
        sortedRecipe.Sort();
        
        // 比较每一个原料是否匹配
        for (int i = 0; i < sortedRecipe.Count; i++)
        {
            if (sortedProvided[i] != sortedRecipe[i])
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 获取菜谱所需的所有原料种类
    /// </summary>
    public List<FoodInPlateType> GetRequiredIngredients()
    {
        return new List<FoodInPlateType>(ingredients);
    }
    
    /// <summary>
    /// 生成此菜谱的描述文本（包含所需原料）
    /// </summary>
    public string GetRecipeDescription()
    {
        string desc = description + "\n\n需要的原料：\n";
        
        foreach (FoodInPlateType ingredient in ingredients)
        {
            desc += $"- {ingredient}\n";
        }
        
        return desc;
    }
} 