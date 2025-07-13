using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 帮助按钮UI控制器
/// </summary>
public class HelpButtonUI : MonoBehaviour
{
    [Tooltip("教程UI引用")]
    public TutorialUI tutorialUI;
    
    [Tooltip("闪烁动画间隔时间")]
    public float pulseInterval = 2f;
    
    [Tooltip("闪烁动画持续时间")]
    public float pulseDuration = 0.5f;
    
    [Tooltip("帮助按钮图像")]
    public Image buttonImage;
    
    // 是否已经播放过闪烁动画
    private bool hasPlayedPulseAnimation = false;
    
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null && tutorialUI != null)
        {
            button.onClick.AddListener(tutorialUI.ShowTutorial);
        }
        
        // 定期播放闪烁动画
        InvokeRepeating(nameof(PlayPulseAnimation), pulseInterval, pulseInterval);
    }
    
    /// <summary>
    /// 播放按钮闪烁动画
    /// </summary>
    private void PlayPulseAnimation()
    {
        if (buttonImage == null || hasPlayedPulseAnimation)
            return;
            
        // 播放缩放动画
        LeanTween.scale(gameObject, Vector3.one * 1.2f, pulseDuration * 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setLoopPingPong(1)
            .setLoopCount(1);
            
        // 播放颜色闪烁动画
        Color originalColor = buttonImage.color;
        Color highlightColor = new Color(1f, 1f, 1f, 1f);
        
        LeanTween.value(gameObject, 0f, 1f, pulseDuration * 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnUpdate((float val) => {
                buttonImage.color = Color.Lerp(originalColor, highlightColor, val);
            })
            .setLoopPingPong(1)
            .setLoopCount(1);
            
        // 第一次玩游戏会多闪几次，之后减少闪烁频率
        if (!PlayerPrefs.HasKey("FirstPlay"))
        {
            PlayerPrefs.SetInt("FirstPlay", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // 三次闪烁后标记为已播放，不再频繁闪烁
            hasPlayedPulseAnimation = true;
            
            // 偶尔仍会闪烁
            Invoke(nameof(ResetPulseState), Random.Range(60f, 180f));
        }
    }
    
    /// <summary>
    /// 重置闪烁状态
    /// </summary>
    private void ResetPulseState()
    {
        hasPlayedPulseAnimation = false;
    }
} 