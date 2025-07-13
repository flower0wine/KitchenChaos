using UnityEngine;
using System;

/// <summary>
/// 炉灶台面，用于烹饪食物
/// </summary>
public class StoveCounter : ItemContainer
{
    [Header("烹饪设置")]
    [Tooltip("烹饪时的粒子效果")]
    public GameObject cookingEffectPrefab;
    
    [Tooltip("烹饪音效")]
    public AudioClip cookingSound;
    
    [Tooltip("烹饪完成音效")]
    public AudioClip cookingCompleteSound;
    
    [Header("视觉效果")]
    [Tooltip("灶台启动时的材质")]
    public GameObject stoveOnEffectPrefab;
    
    [Tooltip("烹饪进度条预制体")]
    public GameObject progressBarPrefab;
    
    [Tooltip("进度条显示位置偏移")]
    public Vector3 progressBarOffset = new Vector3(0, 2.34f, 0);
    
    // 内部变量
    private bool isStoveOn = false;
    private AudioSource audioSource;
    private GameObject cookingEffect;
    private GameObject stoveOnEffect;
    private ProgressUI progressUI;
    
    // 烹饪事件
    public event Action<float> OnCookingProgressChanged;
    public event Action OnCookingCompleted;

    private void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && cookingSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
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
    
    private void Update()
    {
        if (!GameManager.Instance.CanRunning()) return;

        // 检查是否有物品在灶台上
        if (heldItem == null)
        {
            // 如果没有物品但灶台是开着的，关闭灶台
            if (isStoveOn)
            {
                SetStoveState(false);
            }
            return;
        }
        
        // 检查物品是否可烹饪
        CookableItem cookable = heldItem.GetComponent<CookableItem>();
        if (cookable == null || !cookable.CanBeCookedFurther())
        {
            // 物品不可烹饪或已烹饪完成，确保灶台是关闭的
            if (isStoveOn)
            {
                SetStoveState(false);
            }
            return;
        }
        
        // 如果有可烹饪物品但灶台是关的，打开灶台
        if (!isStoveOn)
        {
            SetStoveState(true);
        }
        
        // 更新烹饪进度
        GameObject cookResult = cookable.Cook(Time.deltaTime);
        
        // 更新UI进度条
        UpdateProgressUI(cookable.GetCookingProgress());
        
        // 触发进度变化事件
        OnCookingProgressChanged?.Invoke(cookable.GetCookingProgress());
        
        // 如果烹饪有结果（完成），替换物品
        if (cookResult != null)
        {
            // 播放完成音效
            if (audioSource != null && cookingCompleteSound != null)
            {
                audioSource.PlayOneShot(cookingCompleteSound);
            }
            
            // 替换为烹饪后的物品
            ReplaceWithCookResult(cookResult);
            
            // 触发烹饪完成事件
            OnCookingCompleted?.Invoke();
        }
    }
    
    // 设置灶台状态（开/关）
    private void SetStoveState(bool isOn)
    {
        isStoveOn = isOn;
        
        if (cookingEffectPrefab != null)
        {
            cookingEffectPrefab.SetActive(isStoveOn);
        }
        
        if (stoveOnEffectPrefab != null)
        {
            stoveOnEffectPrefab.SetActive(isStoveOn);
        }
        
        // 更新音效
        UpdateCookingSound();
        
        // 更新UI
        if (progressUI != null)
        {
            progressUI.SetActive(isOn);
        }
    }

    public override bool CanAcceptItem(PickableItem item)
    {
        if (item == null) return false;
        
        // 检查是否是可烹饪物品或已经是烹饪后的结果
        CookableItem cookable = item.GetComponent<CookableItem>();
        return cookable != null;
    }

    public override bool CanHighlight(PlayerInteractor player)
    {
        if (player.GetHeldItem() != null)
        {
            CookableItem cookableItem = player.GetHeldItem().GetComponent<CookableItem>();
            if (cookableItem != null)
            {
                return cookableItem.StoveContainerCanHighlight(this, player);
            }
            return false;
        }

        return GetHeldItem() != null;
    }

    // 更新烹饪音效
    private void UpdateCookingSound()
    {
        if (audioSource != null && cookingSound != null)
        {
            if (isStoveOn && !audioSource.isPlaying)
            {
                audioSource.clip = cookingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (!isStoveOn && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }
    
    // 更新进度条UI
    private void UpdateProgressUI(float cookingProgress)
    {
        if (progressUI != null)
        {
            progressUI.SetProgress(cookingProgress);
        }
    }
    
    // 替换物品为烹饪结果
    private void ReplaceWithCookResult(GameObject resultPrefab)
    {
        if (resultPrefab == null || heldItem == null)
        {
            return;
        }
        
        // 获取当前物品
        GameObject currentItem = heldItem.gameObject;
        
        // 实例化烹饪后的物品
        GameObject cookResult = Instantiate(resultPrefab, itemHoldPoint.position, itemHoldPoint.rotation);
        
        // 获取新物品的组件
        PickableItem resultPickable = cookResult.GetComponent<PickableItem>();
        if (resultPickable == null)
        {
            Debug.LogError($"烹饪后的物品 {cookResult.name} 没有PickableItem组件");
            Destroy(cookResult);
            return;
        }
        
        // 销毁原物品
        Destroy(currentItem);
        
        // 放置新物品到灶台上
        resultPickable.PlaceOn(this);
    }

} 