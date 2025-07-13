using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 游戏操作提示/教程UI控制器
/// </summary>
public class TutorialUI : MonoBehaviour
{
    [Header("UI元素")]
    [Tooltip("教程面板")]
    public RectTransform tutorialPanel;
    
    [Tooltip("关闭按钮")]
    public Button closeButton;
    
    [Tooltip("不再显示复选框")]
    public Toggle doNotShowAgainToggle;
    
    [Header("引用")]
    [Tooltip("快捷按钮")]
    public GameObject helpButton;
    
    [Header("动画设置")]
    [Tooltip("面板打开动画时间")]
    public float openAnimDuration = 0.5f;
    
    // 教程保存键名
    private const string TUTORIAL_SHOWN_KEY = "TutorialShown";
    
    // 是否处于播放动画状态
    private bool isAnimating = false;
    
    private void Awake()
    {
        // 设置初始状态
        if (tutorialPanel != null)
            tutorialPanel.gameObject.SetActive(false);
            
        if (helpButton != null)
            helpButton.SetActive(true);
            
        // 设置按钮事件
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTutorial);
            
        // 如果有常驻按钮，添加点击事件
        Button helpButtonComponent = helpButton?.GetComponent<Button>();
        if (helpButtonComponent != null)
            helpButtonComponent.onClick.AddListener(ShowTutorial);
    }
    
    private void Start()
    {
        // 检查是否已经显示过教程
        bool tutorialShown = PlayerPrefs.GetInt(TUTORIAL_SHOWN_KEY, 0) == 1;
        
        // 首次游戏自动显示教程
        if (!tutorialShown && GameManager.Instance?.GetCurrentState() == GameManager.GameState.NotStarted)
        {
            ShowTutorial();
        }
    }
    
    /// <summary>
    /// 显示教程
    /// </summary>
    public void ShowTutorial()
    {
        // 激活教程面板
        if (tutorialPanel != null)
        {
            tutorialPanel.gameObject.SetActive(true);
            
            // 播放打开动画
            PlayOpenAnimation();
        }
    }
    
    /// <summary>
    /// 关闭教程
    /// </summary>
    public void CloseTutorial()
    {
        // 检查是否选择了不再显示
        if (doNotShowAgainToggle != null && doNotShowAgainToggle.isOn)
        {
            PlayerPrefs.SetInt(TUTORIAL_SHOWN_KEY, 1);
            PlayerPrefs.Save();
        }
        
        // 播放关闭动画后隐藏
        PlayCloseAnimation();
    }
    
    /// <summary>
    /// 播放打开动画
    /// </summary>
    private void PlayOpenAnimation()
    {
        if (tutorialPanel == null)
            return;
            
        isAnimating = true;
            
        // 重置初始状态
        tutorialPanel.localScale = Vector3.zero;
        
        // 播放缩放动画
        LeanTween.scale(tutorialPanel.gameObject, Vector3.one, openAnimDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setOnComplete(() => {
                isAnimating = false;
            });
    }
    
    /// <summary>
    /// 播放关闭动画
    /// </summary>
    private void PlayCloseAnimation()
    {
        if (tutorialPanel == null)
            return;
            
        isAnimating = true;
            
        // 播放缩放动画
        LeanTween.scale(tutorialPanel.gameObject, Vector3.zero, openAnimDuration)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() => {
                tutorialPanel.gameObject.SetActive(false);
                isAnimating = false;
            });
    }
    
} 