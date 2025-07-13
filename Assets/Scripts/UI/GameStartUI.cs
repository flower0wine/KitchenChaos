using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 游戏开始倒计时UI
/// </summary>
public class GameStartUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("倒计时文本")]
    public TextMeshProUGUI countdownText;
    
    [Tooltip("准备文本")]
    public TextMeshProUGUI readyText;
    
    [Tooltip("开始文本")]
    public TextMeshProUGUI startText;
    
    [Header("设置")]
    [Tooltip("倒计时时长(秒)")]
    public float countdownTime = 3f;
    
    [Tooltip("倒计时音效")]
    public AudioClip countdownSound;
    
    [Tooltip("开始音效")]
    public AudioClip startSound;
    
    // 音频源
    private AudioSource audioSource;
    
    private void Awake()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (countdownSound != null || startSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 初始化UI元素
        if (countdownText != null)
        {
            countdownText.text = countdownTime.ToString("0");
        }
        
        if (readyText != null)
        {
            readyText.gameObject.SetActive(true);
        }
        
        if (startText != null)
        {
            startText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 设置倒计时时长
    /// </summary>
    public void SetCountdownTime(float time)
    {
        countdownTime = time;
        
        if (countdownText != null)
        {
            countdownText.text = countdownTime.ToString("0");
        }
    }
    
    /// <summary>
    /// 开始倒计时
    /// </summary>
    public void StartCountdown()
    {
        // 开始倒计时协程
        StartCoroutine(CountdownCoroutine());
    }
    
    /// <summary>
    /// 倒计时协程
    /// </summary>
    private IEnumerator CountdownCoroutine()
    {
        float currentTime = countdownTime;
        
        // 显示准备文本
        if (readyText != null)
        {
            readyText.gameObject.SetActive(true);
        }
        
        // 隐藏开始文本
        if (startText != null)
        {
            startText.gameObject.SetActive(false);
        }
        
        // 倒计时循环
        while (currentTime > 0)
        {
            // 更新倒计时文本
            if (countdownText != null)
            {
                countdownText.text = currentTime.ToString("0");
                
                // 简单的动画效果
                countdownText.transform.localScale = Vector3.one * 1.5f;
                LeanTween.scale(countdownText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            }
            
            // 播放倒计时音效
            if (audioSource != null && countdownSound != null)
            {
                audioSource.PlayOneShot(countdownSound);
            }
            
            // 等待一秒
            yield return new WaitForSeconds(1f);
            
            // 减少时间
            currentTime--;
        }
        
        // 隐藏准备文本
        if (readyText != null)
        {
            readyText.gameObject.SetActive(false);
        }
        
        // 显示开始文本
        if (startText != null)
        {
            startText.gameObject.SetActive(true);
            
            // 简单的动画效果
            startText.transform.localScale = Vector3.zero;
            LeanTween.scale(startText.gameObject, Vector3.one * 1.2f, 0.5f).setEase(LeanTweenType.easeOutBack);
        }
        
        // 隐藏倒计时文本
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // 播放开始音效
        if (audioSource != null && startSound != null)
        {
            audioSource.PlayOneShot(startSound);
        }
        
        // 等待一秒后隐藏
        yield return new WaitForSeconds(1f);
        
        // 隐藏整个UI
        gameObject.SetActive(false);
    }
} 