using UnityEngine;

/// <summary>
/// 表示一个可切割的物品
/// </summary>
public class CuttableItem : MonoBehaviour
{
    [Header("切割设置")]
    [Tooltip("切割后生成的物品预制体")]
    public GameObject cutResultPrefab;
    
    [Tooltip("切割所需的次数")]
    public int cutsNeeded = 1;
    
    [Tooltip("当前已切割次数")]
    private int currentCuts = 0;

    /// <summary>
    /// 切割物品
    /// </summary>
    public GameObject Cut()
    {
        currentCuts++;
        
        // 如果达到所需切割次数，则生成切割后的物品
        if (currentCuts >= cutsNeeded && cutResultPrefab != null)
        {
            return cutResultPrefab;
        }
        
        return null;
    }

    /// <summary>
    /// 检查物品是否可以被切割
    /// </summary>
    public bool CanBeCut()
    {
        return cutResultPrefab != null && currentCuts < cutsNeeded;
    }
    
    /// <summary>
    /// 获取当前切割进度
    /// </summary>
    public float GetCuttingProgress()
    {
        return (float)currentCuts / cutsNeeded;
    }
    
    /// <summary>
    /// 重置切割进度
    /// </summary>
    public void ResetCutting()
    {
        currentCuts = 0;
    }
} 