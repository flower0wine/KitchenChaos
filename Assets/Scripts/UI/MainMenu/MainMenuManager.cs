using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 主菜单管理器，处理主菜单UI和场景转换
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("加载场景名称")]
    public string loadingSceneName = "LoadingScene";
    
    [Tooltip("游戏场景名称")]
    public string gameSceneName = "GameScene";
    
    [Header("UI引用")]
    [Tooltip("开始游戏按钮")]
    public Button startButton;
    
    [Tooltip("退出游戏按钮")]
    public Button quitButton;
    
    [Header("转场效果")]
    [Tooltip("转场淡出时间")]
    public float fadeOutDuration = 0.5f;
    
    [Tooltip("按钮点击音效")]
    public AudioClip buttonClickSound;
    
    private AudioSource audioSource;
    
    private void Awake()
    {
        // 初始化音频源
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && buttonClickSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    private void Start()
    {
        // 设置按钮监听
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }
        else
        {
            Debug.LogError("开始按钮未设置，请在Inspector中设置引用");
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        else
        {
            Debug.LogError("退出按钮未设置，请在Inspector中设置引用");
        }
    }
    
    /// <summary>
    /// 处理开始按钮点击
    /// </summary>
    public void OnStartButtonClicked()
    {
        // 播放按钮点击音效
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // 禁用按钮防止重复点击
        if (startButton != null) startButton.interactable = false;
        if (quitButton != null) quitButton.interactable = false;
        
        // 保存目标场景名称，供加载场景使用
        PlayerPrefs.SetString("TargetSceneName", gameSceneName);
        PlayerPrefs.Save();
        
        // 开始转场
        StartCoroutine(TransitionToLoadingScene());
    }
    
    /// <summary>
    /// 处理退出按钮点击
    /// </summary>
    public void OnQuitButtonClicked()
    {
        // 播放按钮点击音效
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
        
        // 禁用按钮防止重复点击
        if (startButton != null) startButton.interactable = false;
        if (quitButton != null) quitButton.interactable = false;
        
        // 延迟一点退出，让音效有时间播放
        StartCoroutine(DelayQuit());
    }
    
    /// <summary>
    /// 延迟退出游戏
    /// </summary>
    private IEnumerator DelayQuit()
    {
        yield return new WaitForSeconds(0.5f);
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 转场到加载场景
    /// </summary>
    private IEnumerator TransitionToLoadingScene()
    {
        // 创建黑色遮罩用于淡出效果
        GameObject fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 确保在最上层
        
        GameObject fadeImage = new GameObject("FadeImage");
        fadeImage.transform.SetParent(canvas.transform, false);
        Image image = fadeImage.AddComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
        
        RectTransform rectTransform = fadeImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        
        // 执行淡出动画
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeOutDuration);
            image.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        
        // 确保完全黑色
        image.color = Color.black;
        
        // 加载加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(loadingSceneName);
    }
} 