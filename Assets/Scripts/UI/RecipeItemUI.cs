using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 单个菜谱项UI，显示菜谱信息和所需食材
/// </summary>
public class RecipeItemUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("菜谱名称文本")]
    public TextMeshProUGUI recipeName;
    
    [Tooltip("菜谱描述文本")]
    public TextMeshProUGUI recipeDescription;
    
    [Tooltip("菜谱价格文本")]
    public TextMeshProUGUI priceText;
    
    [Tooltip("菜谱图标")]
    public Image recipeIcon;
    
    [Header("食材UI设置")]
    [Tooltip("食材图标模板")]
    public GameObject ingredientIconTemplate;
    
    [Tooltip("食材图标容器")]
    public Transform ingredientContainer;
    
    // 当前显示的食材图标
    private List<GameObject> ingredientIcons = new List<GameObject>();
    
    // 当前显示的菜谱
    private Recipe currentRecipe;

    public void Awake()
    {
        if (ingredientIconTemplate != null)
        {
            ingredientIconTemplate.SetActive(false);
        }
    }
    
    /// <summary>
    /// 设置并显示菜谱
    /// </summary>
    public void SetupRecipe(Recipe recipe)
    {
        currentRecipe = recipe;

        // 更新UI文本
        if (recipeName != null) recipeName.text = recipe.recipeName;
        if (recipeDescription != null) recipeDescription.text = recipe.description;
        if (priceText != null) priceText.text = recipe.price.ToString() + "元";
        
        // 更新菜谱图标
        if (recipeIcon != null)
        {
            if (recipe.recipeIcon != null)
            {
                recipeIcon.sprite = recipe.recipeIcon;
                recipeIcon.gameObject.SetActive(true);
            }
            else
            {
                recipeIcon.gameObject.SetActive(false);
            }
        }
        
        // 显示所需食材
        UpdateIngredientIcons();
    }
    
    /// <summary>
    /// 更新食材图标
    /// </summary>
    private void UpdateIngredientIcons()
    {
        // 清除现有食材图标
        ClearIngredientIcons();
        
        if (currentRecipe == null || ingredientIconTemplate == null || ingredientContainer == null)
        {
            return;
        }
        
        // 获取所有所需食材
        List<FoodInPlateType> ingredients = currentRecipe.GetRequiredIngredients();
        
        // 为每个食材创建图标
        foreach (FoodInPlateType ingredient in ingredients)
        {
            CreateIngredientIcon(ingredient);
        }
    }
    
    /// <summary>
    /// 创建食材图标
    /// </summary>
    private void CreateIngredientIcon(FoodInPlateType foodInPlateType)
    {
        // 获取食材图标
        Sprite foodIcon = FoodManager.Instance?.GetIconForFood(foodInPlateType.ToFoodType());
        if (foodIcon == null)
        {
            Debug.LogWarning($"未找到食材类型 {foodInPlateType} 的图标");
            return;
        }
        
        // 创建图标实例
        GameObject newIcon = Instantiate(ingredientIconTemplate, ingredientContainer);
        newIcon.SetActive(true);
        
        // 如果没有IconTemplateInfo组件，尝试直接找Image组件
        Image iconImage = newIcon.GetComponentInChildren<Image>();
        if (iconImage != null)
        {
            iconImage.sprite = foodIcon;
        }
        else
        {
            Debug.LogWarning("无法设置食材图标，缺少必要组件");
        }
        
        // 添加到列表
        ingredientIcons.Add(newIcon);
    }
    
    /// <summary>
    /// 清除所有食材图标
    /// </summary>
    private void ClearIngredientIcons()
    {
        foreach (var icon in ingredientIcons)
        {
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        
        ingredientIcons.Clear();
    }
    
    private void OnDestroy()
    {
        ClearIngredientIcons();
    }
} 