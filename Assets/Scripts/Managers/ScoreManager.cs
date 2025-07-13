using UnityEngine;
using System;

/// <summary>
/// 分数管理器，负责游戏中的分数统计和更新
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // 单例实例
    public static ScoreManager Instance { get; private set; }
    
    // 分数数据
    private int deliveredCount = 0;
    private int totalScore = 0;
    
    // 事件
    public event Action<int, int> OnScoreChanged; // 参数：递送数量，总分数
    
    private void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // 确保不会被销毁
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // 使用延迟初始化，确保在所有物体都已实例化后再订阅事件
        Invoke(nameof(InitializeEvents), 0.1f);
    }
    
    /// <summary>
    /// 延迟初始化事件订阅
    /// </summary>
    private void InitializeEvents()
    {
        // 查找所有DeliveryCounter
        DeliveryCounter[] deliveryCounters = FindObjectsOfType<DeliveryCounter>();
        
        foreach (var counter in deliveryCounters)
        {
            counter.OnRecipeSuccess += HandleRecipeDelivered;
        }
        
    }
    
    private void OnDestroy()
    {
        // 取消所有事件订阅
        DeliveryCounter[] deliveryCounters = FindObjectsOfType<DeliveryCounter>();
        foreach (var counter in deliveryCounters)
        {
            counter.OnRecipeSuccess -= HandleRecipeDelivered;
        }
    }
    
    /// <summary>
    /// 处理菜谱递送成功事件
    /// </summary>
    private void HandleRecipeDelivered(Recipe recipe, int score)
    {
        
        // 只在游戏进行中计分
        if (GameManager.Instance != null && 
            GameManager.Instance.GetCurrentState() != GameManager.GameState.Playing)
        {
            Debug.LogWarning("游戏不在进行状态，不计分");
            return;
        }
        
        // 更新递送数据
        deliveredCount++;
        totalScore += score;
        
        // 触发分数变化事件
        OnScoreChanged?.Invoke(deliveredCount, totalScore);
    }
    
    /// <summary>
    /// 重置分数
    /// </summary>
    public void ResetScore()
    {
        deliveredCount = 0;
        totalScore = 0;
        
        // 通知UI更新
        OnScoreChanged?.Invoke(deliveredCount, totalScore);
    }
    
    /// <summary>
    /// 获取当前分数
    /// </summary>
    public (int deliveredCount, int totalScore) GetScore()
    {
        return (deliveredCount, totalScore);
    }
} 