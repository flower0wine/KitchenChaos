using UnityEngine;

/// <summary>
/// 可销毁物品组件
/// </summary>
public class TrashableItem : Interactable
{
    [Tooltip("销毁时是否播放特效")]
    public bool playEffectOnTrash = true;
    
    [Tooltip("销毁特效预制体")]
    public GameObject trashEffectPrefab;

    public bool Trash()
    {
        // 播放销毁特效
        if (playEffectOnTrash && trashEffectPrefab != null)
        {
            Instantiate(trashEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // 在这里可以添加额外的销毁逻辑
        Debug.Log($"物品 {gameObject.name} 被销毁");
        
        // 销毁游戏对象
        Destroy(gameObject);
        
        return true;
    }
} 