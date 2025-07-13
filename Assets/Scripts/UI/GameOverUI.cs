using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 游戏结束UI
/// </summary>
public class GameOverUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("递送数量文本")]
    public TextMeshProUGUI deliveredCountText;
    
    [Tooltip("总分数文本")]
    public TextMeshProUGUI totalScoreText;
    
    [Tooltip("重新开始按钮")]
    public Button restartButton;
    
    [Tooltip("退出按钮")]
    public Button quitButton;
    
    [Header("设置")]
    [Tooltip("结束音效")]
    public AudioClip gameOverSound;
    
    // 音频源
    private AudioSource audioSource;
    
    private void Awake()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && gameOverSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 设置按钮监听
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }
    
    private void OnEnable()
    {
        // 播放结束音效
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // 动画效果
        transform.localScale = Vector3.zero;
        LeanTween.scale(gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
    }
    
    /// <summary>
    /// 设置游戏结果
    /// </summary>
    public void SetGameResult(int deliveredCount, int totalScore)
    {
        // 更新UI文本
        if (deliveredCountText != null)
        {
            deliveredCountText.text = deliveredCount.ToString();
        }
        
        if (totalScoreText != null)
        {
            totalScoreText.text = totalScore.ToString();
        }
    }
    
    /// <summary>
    /// 重新开始按钮点击事件
    /// </summary>
    private void OnRestartClicked()
    {
        // 通知游戏管理器重新开始游戏
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
    
    /// <summary>
    /// 退出按钮点击事件
    /// </summary>
    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
} 