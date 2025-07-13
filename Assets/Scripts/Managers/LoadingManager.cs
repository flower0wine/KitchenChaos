using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// 加载场景管理器，负责显示加载进度和执行场景切换
/// </summary>
public class LoadingManager : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("进度条")]
    public Slider progressBar;
    
    [Tooltip("进度文本")]
    public TextMeshProUGUI progressText;
    
    [Tooltip("提示文本")]
    public TextMeshProUGUI tipText;
    
    [Header("加载设置")]
    [Tooltip("进度条平滑填充速度")]
    public float fillSpeed = 2f;
    
    [Tooltip("完成后等待时间")]
    public float completeWaitTime = 0.5f;
    
    [Tooltip("游戏提示")]
    public string[] gameTips;
    
    [Tooltip("提示文本变换间隔(秒)")]
    public float tipChangeInterval = 3f;
    
    // 内部变量
    private string targetSceneName;
    private float targetProgress = 0f;
    private float currentDisplayedProgress = 0f;
    private float tipTimer = 0f;
    
    private void Awake()
    {
        // 尝试从PlayerPrefs获取目标场景名称
        targetSceneName = PlayerPrefs.GetString("TargetSceneName", "GameScene");
        
        // 设置初始进度
        if (progressBar != null)
            progressBar.value = 0f;
            
        if (progressText != null)
            progressText.text = "准备加载...";
            
        // 设置初始提示
        SetRandomTip();
    }
    
    private void Start()
    {
        // 开始加载目标场景
        StartCoroutine(LoadTargetScene());
    }
    
    private void Update()
    {
        // 平滑更新进度条显示
        if (currentDisplayedProgress < targetProgress)
        {
            currentDisplayedProgress = Mathf.MoveTowards(currentDisplayedProgress, targetProgress, fillSpeed * Time.deltaTime);
            if (progressBar != null)
                progressBar.value = currentDisplayedProgress;
                
            if (progressText != null)
                progressText.text = $"加载中... {(currentDisplayedProgress * 100):F0}%";
        }
        
        // 更新提示文本
        tipTimer += Time.deltaTime;
        if (tipTimer >= tipChangeInterval && gameTips != null && gameTips.Length > 0)
        {
            tipTimer = 0f;
            SetRandomTip();
        }
    }
    
    /// <summary>
    /// 设置随机游戏提示
    /// </summary>
    private void SetRandomTip()
    {
        if (tipText != null && gameTips != null && gameTips.Length > 0)
        {
            string newTip = gameTips[Random.Range(0, gameTips.Length)];
            
            // 应用提示文本
            tipText.text = newTip;
            
            // 可选：添加文本动画效果
            tipText.transform.localScale = Vector3.one * 0.9f;
            LeanTween.cancel(tipText.gameObject);
            LeanTween.scale(tipText.gameObject, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);
            
            // 可选：淡入效果
            Color originalColor = tipText.color;
            Color fadeColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
            tipText.color = fadeColor;
            LeanTween.value(tipText.gameObject, 0f, 1f, 0.5f)
                .setOnUpdate((float val) => {
                    tipText.color = new Color(originalColor.r, originalColor.g, originalColor.b, val);
                });
        }
    }
    
    /// <summary>
    /// 加载目标场景
    /// </summary>
    private IEnumerator LoadTargetScene()
    {
        // 等待一帧，确保所有对象都已初始化
        yield return null;
        
        // 初始提示暂停显示几秒
        yield return new WaitForSeconds(0.5f);
        
        // 开始异步加载
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        
        // 设置不自动激活场景
        asyncLoad.allowSceneActivation = false;
        
        // 监控加载进度
        while (asyncLoad.progress < 0.9f) // Unity加载到0.9就表示加载完成
        {
            // 更新目标进度
            targetProgress = asyncLoad.progress / 0.9f;
            
            yield return null;
        }
        
        // 加载完成，设置进度为100%
        targetProgress = 1.0f;
        
        // 等待进度条追上
        while (currentDisplayedProgress < 0.99f)
        {
            yield return null;
        }
        
        // 更新完成文本
        if (progressText != null)
            progressText.text = "加载完成";
            
        // 等待指定时间
        yield return new WaitForSeconds(completeWaitTime);
        
        // 激活场景
        asyncLoad.allowSceneActivation = true;
    }
} 