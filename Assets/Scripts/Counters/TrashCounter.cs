using UnityEngine;
using System;

/// <summary>
/// 垃圾桶容器，用于销毁符合条件的物品
/// </summary>
public class TrashContainer : Interactable
{
    [Header("垃圾桶设置")]
    [Tooltip("垃圾桶音效")]
    public AudioClip trashSound;
    
    [Tooltip("垃圾桶特效预制体")]
    public GameObject trashEffectPrefab;
    
    [Tooltip("动画触发器名称")]
    public string trashAnimTriggerName = "Trash";
    
    private AudioSource audioSource;
    private Animator trashAnimator;
    
    // 事件
    public event Action OnItemTrashed;

    private void Start()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && trashSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 获取动画控制器
        trashAnimator = GetComponent<Animator>();
        if (trashAnimator == null)
        {
            trashAnimator = GetComponentInChildren<Animator>();
        }
    }
    
    // 重写交互方法
    public override bool HandleInteraction(InteractionContext context)
    {
        // 如果没有物品，执行默认交互
        if (!context.HasItem)
        {
            OnInteract(context.Player);
            return true;
        }
        
        // 检查物品是否可以被销毁
        TrashableItem trashable = context.HeldItem.GetComponent<TrashableItem>();
        if (trashable == null)
        {
            Debug.Log("此物品不可被垃圾桶销毁");
            return false; // 未处理，允许丢弃
        }
        
        // 尝试销毁物品
        bool trashed = trashable.Trash();
        if (trashed)
        {
            // 从玩家手中移除物品
            context.Player.DestroyHeldItem();
            
            // 播放音效和特效
            PlayTrashEffects();
            
            // 触发事件
            OnItemTrashed?.Invoke();
            return true;
        }
        
        return false; // 销毁失败，允许丢弃
    }

    private void PlayTrashEffects()
    {
        // 播放音效
        if (audioSource != null && trashSound != null)
        {
            audioSource.PlayOneShot(trashSound);
        }
        
        // 播放特效
        if (trashEffectPrefab != null)
        {
            Instantiate(trashEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
        
        // 播放动画
        if (trashAnimator != null)
        {
            trashAnimator.SetTrigger(trashAnimTriggerName);
        }
    }

    public override bool CanHighlight(PlayerInteractor player)
    {
        if (player.GetHeldItem() != null)
        {
            TrashableItem trashable = player.GetHeldItem().GetComponent<TrashableItem>();
            if (trashable != null)
            {
                return true;
            }
        }
        return false;
    }
} 