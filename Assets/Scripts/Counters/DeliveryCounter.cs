using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// 送餐台面，用于提交完成的菜肴
/// </summary>
public class DeliveryCounter : Interactable
{
    [Header("送餐设置")]
    [Tooltip("成功送餐音效")]
    public AudioClip successSound;
    
    [Tooltip("失败送餐音效")]
    public AudioClip failSound;
    
    [Tooltip("成功送餐特效预制体")]
    public GameObject successEffectPrefab;
    
    [Tooltip("失败送餐特效预制体")]
    public GameObject failEffectPrefab;
    
    [Tooltip("特效生成位置偏移")]
    public Vector3 effectOffset = new Vector3(0, 0.5f, 0);
    
    [Tooltip("递送成功后是否从菜单中移除该菜谱")]
    public bool removeRecipeAfterDelivery = true;
    
    // 音频源
    private AudioSource audioSource;

    private RecipeMenuUI recipeMenuUI;
    
    // 事件
    public event Action<Recipe, int> OnRecipeSuccess;
    public event Action OnRecipeFailed;
    
    private void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (successSound != null || failSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 初始化菜单UI
        recipeMenuUI = FindObjectOfType<RecipeMenuUI>();
        if (recipeMenuUI == null)
        {
            Debug.LogError("未找到RecipeMenuUI脚本");
        }
    }
    
    // 重写交互方法
    public override bool HandleInteraction(InteractionContext context)
    {
        // 只处理玩家手持物品的情况
        if (context.HasItem)
        {
            PlateItem plate = context.HeldItem.GetComponent<PlateItem>();
            if (plate != null)
            {
                // 接收盘子并处理
                bool success = DeliverPlate(plate);
                
                if (success)
                {
                    Destroy(context.HeldItem.gameObject);
                    context.Player.SetHeldItem(null);
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 处理送餐逻辑
    /// </summary>
    /// <param name="plate">玩家递交的盘子</param>
    /// <returns>是否成功匹配菜谱</returns>
    private bool DeliverPlate(PlateItem plate)
    {
        if (plate == null) return false;
        
        // 检查盘子是否匹配任何菜谱
        Recipe matchedRecipe = RecipeManager.Instance.CheckPlateForRecipe(plate);
        
        if (matchedRecipe != null)
        {
            // 成功匹配菜谱
            HandleSuccessfulDelivery(matchedRecipe);
            
            return true;
        }
        else
        {
            // 未匹配任何菜谱
            HandleFailedDelivery(plate);
            return false;
        }
    }
    
    /// <summary>
    /// 处理成功送餐
    /// </summary>
    private void HandleSuccessfulDelivery(Recipe recipe)
    {
        Debug.Log($"成功递送: {recipe.recipeName} (ID: {recipe.id}), 获得 {recipe.price} 分");

        // 从菜单中移除已递送的菜谱
        if (removeRecipeAfterDelivery && RecipeManager.Instance != null)
        {
            // 保存菜谱ID以便在事件中使用
            string deliveredRecipeId = recipe.id;
            
            RecipeManager.Instance.RemoveRecipe(recipe);
            
            recipeMenuUI.RemoveObsoleteRecipeItems(new HashSet<string> { deliveredRecipeId });
        }
        
        // 播放成功音效
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }
        
        // 生成成功特效
        SpawnEffect(successEffectPrefab);
        
        // 触发成功事件
        OnRecipeSuccess?.Invoke(recipe, recipe.price);
    }
    
    /// <summary>
    /// 处理失败送餐
    /// </summary>
    private void HandleFailedDelivery(PlateItem plate)
    {
        // 获取盘子上的所有食材，生成描述
        List<FoodType> ingredients = new List<FoodType>();
        foreach (var item in plate.GetAllPlacedItems())
        {
            ingredients.Add(item.Key.ToFoodType());
        }
        
        string itemsDesc = string.Join(", ", ingredients);
        Debug.Log($"无效的菜肴组合: {itemsDesc}");
        
        // 播放失败音效
        if (audioSource != null && failSound != null)
        {
            audioSource.PlayOneShot(failSound);
        }
        
        // 生成失败特效
        SpawnEffect(failEffectPrefab);
        
        // 触发失败事件
        OnRecipeFailed?.Invoke();
    }
    
    /// <summary>
    /// 生成特效
    /// </summary>
    private void SpawnEffect(GameObject effectPrefab)
    {
        if (effectPrefab != null)
        {
            Vector3 spawnPos = transform.position + effectOffset;
            GameObject effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
            
            // 自动销毁特效（假设特效会自动结束）
            Destroy(effect, 2f);
        }
    }
    
    // 重写高亮方法
    public override bool CanHighlight(PlayerInteractor player)
    {
        // 只有当玩家手持盘子时才高亮
        if (player.GetHeldItem() != null)
        {
            return player.GetHeldItem().GetComponent<PlateItem>() != null;
        }
        return false;
    }
} 