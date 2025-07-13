using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 图标模板信息类，用于指定图标模板中图标的位置
/// </summary>
public class IconTemplateInfo : MonoBehaviour
{
    [Tooltip("图标渲染组件引用")]
    public Image iconImage;
    
    /// <summary>
    /// 设置图标精灵
    /// </summary>
    public void SetIcon(Sprite icon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }
        else
        {
            Debug.LogWarning("图标渲染组件未设置，无法显示图标");
        }
    }
} 