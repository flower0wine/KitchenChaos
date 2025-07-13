using UnityEngine;
using System;

/// <summary>
/// 切菜板容器，继承自ItemContainer
/// </summary>
public class CuttingContainer : ItemContainer
{
    [Header("切菜设置")]
    [Tooltip("切菜特效预制体")]
    public GameObject cuttingEffectPrefab;
    
    [Tooltip("切菜音效")]
    public AudioClip cuttingSound;
    
    [Header("UI设置")]
    [Tooltip("切菜进度条预制体")]
    public GameObject progressBarPrefab;
    [Tooltip("进度条显示位置偏移")]
    public Vector3 progressBarOffset = new Vector3(0, 2.34f, 0.44f);
    
    [Header("动画设置")]
    [Tooltip("切菜动画控制器")]
    public Animator cuttingAnimator;
    [Tooltip("切菜动画触发器名称")]
    public string cutAnimTriggerName = "Cut";
    
    private AudioSource audioSource;
    private bool isCutting = false;
    private ProgressUI progressUI;
    
    // 切菜事件
    public event Action<float> OnCuttingProgressChanged;
    public event Action OnCuttingCompleted;

    private void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && cuttingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 查找动画控制器
        if (cuttingAnimator == null)
        {
            cuttingAnimator = GetComponent<Animator>();
            if (cuttingAnimator == null)
            {
                cuttingAnimator = GetComponentInChildren<Animator>();
            }
        }
        
        // 创建进度条
        InitializeProgressUI();
    }
    
    private void InitializeProgressUI()
    {
        if (progressBarPrefab != null)
        {
            GameObject progressObj = Instantiate(progressBarPrefab, transform.position + progressBarOffset, Quaternion.identity);
            progressObj.transform.SetParent(transform);
            progressUI = progressObj.GetComponent<ProgressUI>();
            if (progressUI == null)
            {
                progressUI = progressObj.AddComponent<ProgressUI>();
            }
            progressUI.SetActive(false);
        }
    }

    public bool IsCutting()
    {
        return isCutting;
    }

    public override void OnPickUp(InteractionContext context)
    {
        progressUI.SetActive(false);
    }

    public override void OnPlace(InteractionContext context)
    {
        CuttableItem cuttableItem = heldItem.GetComponent<CuttableItem>();
        if (cuttableItem == null || !cuttableItem.CanBeCut()) return;

        progressUI.SetActive(true);
        // 更新进度UI
        UpdateProgressUI(cuttableItem.GetCuttingProgress());
    }
    
    public void TryCutting(CuttableItem cuttableItem)
    {
        // 开始切菜
        isCutting = true;
        
        // 播放切菜音效
        if (audioSource != null && cuttingSound != null)
        {
            audioSource.PlayOneShot(cuttingSound);
        }
        
        // 播放切菜特效
        if (cuttingEffectPrefab != null)
        {
            Instantiate(cuttingEffectPrefab, itemHoldPoint.position, Quaternion.identity);
        }
        
        // 播放切菜动画
        PlayCuttingAnimation();
        
        // 执行切割
        GameObject cutResult = cuttableItem.Cut();
        
        // 更新进度UI
        UpdateProgressUI(cuttableItem.GetCuttingProgress());
        
        // 广播进度变化
        OnCuttingProgressChanged?.Invoke(cuttableItem.GetCuttingProgress());
        
        // 如果切割完成，替换物品
        if (cutResult != null)
        {
            ReplaceWithCutResult(cutResult);
            OnCuttingCompleted?.Invoke();
            
            // 切割完成，隐藏进度条
            if (progressUI != null)
            {
                progressUI.SetActive(false);
            }
        }
        
        isCutting = false;
    }
    
    private void PlayCuttingAnimation()
    {
        if (cuttingAnimator != null)
        {
            cuttingAnimator.SetTrigger(cutAnimTriggerName);
        }
    }
    
    private void UpdateProgressUI(float progress)
    {
        if (progressUI != null)
        {
            // 显示进度条
            progressUI.SetActive(true);
            // 更新进度
            progressUI.SetProgress(progress);
        }
    }
    
    private void ReplaceWithCutResult(GameObject cutResultPrefab)
    {
        // 获取当前物品
        GameObject currentItem = GetHeldItem().gameObject;
        
        // 实例化切割后的物品
        GameObject cutResult = Instantiate(cutResultPrefab, itemHoldPoint.position, itemHoldPoint.rotation);
        
        // 设置切割后物品的父物体和位置
        cutResult.transform.position = itemHoldPoint.position;
        
        // 获取切割后物品的可交互组件
        PickableItem cutResultPickable = cutResult.GetComponent<PickableItem>();
        if (cutResultPickable == null)
        {
            Debug.LogError($"切割后的物品 {cutResult.name} 没有PickableItem组件");
            Destroy(cutResult);
            return;
        }
        
        // 销毁原物品
        Destroy(currentItem);
        
        cutResultPickable.PlaceOn(this);
    }
    
    // 重写物品验证方法
    public override bool CanAcceptItem(PickableItem item)
    {
        if (item.GetComponent<CuttableItem>() == null)
        {
            return false;
        }
        return true;
    }
    
    // 重写切割处理方法
    public override void HandleCutting(PlayerInteractor player)
    {
        // 检查是否有物品
        if (heldItem == null) return;
        
        // 检查物品是否可切割
        CuttableItem cuttable = heldItem.GetComponent<CuttableItem>();
        if (cuttable == null || !cuttable.CanBeCut()) return;
        
        // 执行切割
        TryCutting(cuttable);
    }
    
    private void OnDestroy()
    {
        // 销毁进度UI
        if (progressUI != null)
        {
            Destroy(progressUI.gameObject);
        }
    }

    public override bool CanHighlight(PlayerInteractor player)
    {
        if (player.GetHeldItem() != null)
        {
            CuttableItem cuttable = player.GetHeldItem().GetComponent<CuttableItem>();
            if (cuttable != null)
            {
                return true;
            }
        }
        return false;
    }
} 