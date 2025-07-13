using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 盘子台面，用于自动生成盘子并支持玩家取用
/// </summary>
public class PlatesCounter : ItemContainer
{
    [Header("盘子设置")]
    [Tooltip("盘子预制件")]
    public GameObject platePrefab;
    
    [Tooltip("盘子生成间隔(秒)")]
    public float plateSpawnTime = 4f;
    
    [Tooltip("盘子层叠间隔高度")]
    public float plateStackHeight = 0.1f;
    
    [Tooltip("最大盘子数量")]
    public int maxPlateCount = 8;
    
    [Header("效果")]
    [Tooltip("生成盘子的音效")]
    public AudioClip plateSpawnSound;
    
    // 内部变量
    private float spawnTimer = 0f;
    private AudioSource audioSource;
    private List<GameObject> spawnedPlates = new List<GameObject>();
    
    private void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (plateSpawnSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Update()
    {
        if (!GameManager.Instance.CanRunning()) return;

        // 如果盘子数量未达到最大值，增加生成计时器
        if (spawnedPlates.Count < maxPlateCount)
        {
            spawnTimer += Time.deltaTime;
            
            // 检查是否达到生成时间
            if (spawnTimer >= plateSpawnTime)
            {
                // 重置计时器
                spawnTimer = 0f;
                
                // 生成新盘子
                SpawnPlate();
            }
        }
    }
    
    // 生成一个新盘子
    private void SpawnPlate()
    {
        if (platePrefab == null) return;
        
        // 计算新盘子的位置（基于当前盘子堆的高度）
        Vector3 spawnPosition = itemHoldPoint.position + new Vector3(0, plateStackHeight * spawnedPlates.Count, 0);
        
        // 实例化盘子
        GameObject plateObj = Instantiate(platePrefab, spawnPosition, Quaternion.identity);
        plateObj.transform.SetParent(transform);
        
        // 添加到盘子列表
        spawnedPlates.Add(plateObj);
        
        // 播放生成音效
        if (audioSource != null && plateSpawnSound != null)
        {
            audioSource.PlayOneShot(plateSpawnSound);
        }
    }
    
    // 尝试取出一个盘子
    private GameObject TryGetPlate()
    {
        // 检查是否有可用的盘子
        if (spawnedPlates.Count == 0) return null;
        
        // 获取最上面的盘子（最后添加的盘子）
        GameObject plate = spawnedPlates[spawnedPlates.Count - 1];
        
        // 从列表中移除
        spawnedPlates.RemoveAt(spawnedPlates.Count - 1);
        
        return plate;
    }
    
    // 重写交互方法，实现取盘子的功能
    public override bool HandleInteraction(InteractionContext context)
    {
        // 如果玩家没有拿着物品，尝试给予盘子
        if (!context.HasItem)
        {
            // 尝试获取一个盘子
            GameObject plate = TryGetPlate();
            if (plate != null)
            {
                // 获取盘子的可拾取组件
                PickableItem plateItem = plate.GetComponent<PickableItem>();
                if (plateItem != null)
                {
                    // 让玩家拾取盘子
                    context.Player.PickUpItem(plateItem);
                    
                    return true;
                }
                else
                {
                    // 如果盘子没有PickableItem组件，销毁它并记录错误
                    Debug.LogError("盘子预制件没有PickableItem组件！");
                    Destroy(plate);
                }
            }
        }
        
        // 玩家手上拿着物品，不能取盘子
        return false;
    }
    
    // 重写物品验证方法
    public override bool CanAcceptItem(PickableItem item)
    {
        // 盘子台不接受物品放置
        return false;
    }
    
    // 重写可高亮方法，仅当玩家手上没有物品时才高亮
    public override bool CanHighlight(PlayerInteractor player)
    {
        // 只有当玩家手上没有物品且有可用盘子时才高亮
        return player.GetHeldItem() == null && spawnedPlates.Count > 0;
    }
    
    // 清理方法，防止内存泄漏
    private void OnDestroy()
    {
        // 清理所有生成的盘子
        foreach (var plate in spawnedPlates)
        {
            if (plate != null)
            {
                Destroy(plate);
            }
        }
        spawnedPlates.Clear();
    }
} 