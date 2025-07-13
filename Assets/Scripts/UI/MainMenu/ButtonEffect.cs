using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 按钮效果控制器，提供视觉反馈
/// </summary>
public class ButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    /// <summary>
    /// 悬停效果类型
    /// </summary>
    public enum HoverEffectType
    {
        None,       // 无颜色效果
        Brighten,   // 高亮效果
        Darken      // 变暗效果
    }
    
    [Tooltip("鼠标悬停缩放")]
    public Vector3 hoverScale = new Vector3(1.1f, 1.1f, 1.1f);
    
    [Tooltip("点击缩放")]
    public Vector3 clickScale = new Vector3(0.95f, 0.95f, 0.95f);
    
    [Tooltip("动画速度")]
    public float animationSpeed = 8f;
    
    [Tooltip("悬停颜色效果类型")]
    public HoverEffectType hoverEffect = HoverEffectType.Brighten;
    
    [Tooltip("颜色变化强度 (0-1)")]
    [Range(0.0f, 0.3f)]
    public float colorChangeIntensity = 0.1f;
    
    private Vector3 originalScale;
    private Vector3 targetScale;
    private Color originalColor;
    private Color targetColor;
    private Image buttonImage;
    private bool isInteractable = true;
    
    private void Awake()
    {
        // 保存原始缩放和颜色
        originalScale = transform.localScale;
        targetScale = originalScale;
        
        // 获取按钮图像和当前状态
        buttonImage = GetComponent<Image>();
        Button button = GetComponent<Button>();
        
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
            targetColor = originalColor;
            
            // 检查按钮是否可交互
            if (button != null)
            {
                isInteractable = button.interactable;
                // 监听按钮可交互状态变化
                button.onClick.AddListener(() => CheckInteractable());
            }
        }
    }
    
    private void OnEnable()
    {
        // 重置为原始状态
        if (buttonImage != null)
        {
            originalColor = buttonImage.color;
            targetColor = originalColor;
        }
    }
    
    private void Update()
    {
        // 平滑动画过渡
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        
        if (buttonImage != null && hoverEffect != HoverEffectType.None)
        {
            buttonImage.color = Color.Lerp(buttonImage.color, targetColor, Time.deltaTime * animationSpeed);
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;
        
        // 鼠标悬停效果
        targetScale = originalScale * hoverScale.x;
        
        // 应用颜色效果
        if (buttonImage != null && hoverEffect != HoverEffectType.None)
        {
            targetColor = CalculateHoverColor(originalColor, hoverEffect, colorChangeIntensity);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // 恢复原始状态
        targetScale = originalScale;
        if (buttonImage != null)
        {
            targetColor = originalColor;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;
        
        // 点击效果
        targetScale = originalScale * clickScale.x;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsButtonInteractable()) return;
        
        // 点击后恢复悬停状态
        targetScale = originalScale * hoverScale.x;
    }
    
    /// <summary>
    /// 计算悬停时的颜色
    /// </summary>
    private Color CalculateHoverColor(Color baseColor, HoverEffectType effect, float intensity)
    {
        // 根据效果类型计算颜色
        switch (effect)
        {
            case HoverEffectType.Brighten:
                // 提高亮度，但不超过1
                return new Color(
                    Mathf.Clamp01(baseColor.r + intensity),
                    Mathf.Clamp01(baseColor.g + intensity),
                    Mathf.Clamp01(baseColor.b + intensity),
                    baseColor.a
                );
                
            case HoverEffectType.Darken:
                // 降低亮度，但不低于0
                return new Color(
                    Mathf.Clamp01(baseColor.r - intensity),
                    Mathf.Clamp01(baseColor.g - intensity),
                    Mathf.Clamp01(baseColor.b - intensity),
                    baseColor.a
                );
                
            default:
                return baseColor;
        }
    }
    
    /// <summary>
    /// 检查按钮是否可交互
    /// </summary>
    private bool IsButtonInteractable()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            return button.interactable;
        }
        return isInteractable;
    }
    
    /// <summary>
    /// 检查并更新按钮可交互状态
    /// </summary>
    private void CheckInteractable()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            isInteractable = button.interactable;
            
            // 如果按钮变为不可交互，恢复原始状态
            if (!isInteractable)
            {
                transform.localScale = originalScale;
                if (buttonImage != null)
                {
                    buttonImage.color = originalColor;
                }
            }
        }
    }
} 