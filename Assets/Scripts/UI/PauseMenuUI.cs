using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 暂停菜单UI控制器
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("面板引用")]
    [Tooltip("主面板容器")]
    public RectTransform mainPanel;
    
    [Tooltip("暂停菜单Logo")]
    public Image logoImage;
    
    [Header("按钮引用")]
    [Tooltip("继续游戏按钮")]
    public Button resumeButton;
    
    [Tooltip("返回主菜单按钮")]
    public Button mainMenuButton;
    
    [Tooltip("退出游戏按钮")]
    public Button quitButton;
    
    [Header("动画设置")]
    [Tooltip("面板打开动画时间")]
    public float openAnimDuration = 0.3f;
    
    [Tooltip("按钮动画延迟")]
    public float buttonAnimDelay = 0.1f;
    
    private void Awake()
    {
        // 设置初始隐藏状态
        if (mainPanel != null)
        {
            mainPanel.localScale = Vector3.zero;
        }
        
        // 设置Logo初始状态
        if (logoImage != null)
        {
            logoImage.color = new Color(logoImage.color.r, logoImage.color.g, logoImage.color.b, 0f);
        }
        
        // 设置按钮初始状态
        SetupButtonInitialState(resumeButton);
        SetupButtonInitialState(mainMenuButton);
        SetupButtonInitialState(quitButton);
    }
    
    private void Start()
    {
        // 设置按钮点击事件
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitClicked);
        }
        
        // 确保第一个按钮被选中
        SelectFirstButton();
    }
    
    private void OnEnable()
    {
        // 当菜单启用时，选择第一个按钮
        SelectFirstButton();
    }
    
    /// <summary>
    /// 播放打开动画
    /// </summary>
    public void PlayOpenAnimation()
    {
        // 防止在没有LeanTween的情况下报错
        if (mainPanel == null) return;
        
        // 重置初始状态
        mainPanel.localScale = Vector3.zero;
        
        // 设置Logo初始透明状态
        if (logoImage != null)
        {
            Color logoColor = logoImage.color;
            logoImage.color = new Color(logoColor.r, logoColor.g, logoColor.b, 0f);
        }
        
        // 设置按钮初始状态
        SetupButtonInitialState(resumeButton);
        SetupButtonInitialState(mainMenuButton);
        SetupButtonInitialState(quitButton);
        
        // 主面板打开动画
        LeanTween.scale(mainPanel.gameObject, Vector3.one, openAnimDuration)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true);
        
        // Logo淡入动画
        if (logoImage != null)
        {
            LeanTween.value(logoImage.gameObject, 0f, 1f, openAnimDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnUpdate((float val) => {
                    Color color = logoImage.color;
                    logoImage.color = new Color(color.r, color.g, color.b, val);
                })
                .setIgnoreTimeScale(true);
                
            // 添加轻微的缩放动画使Logo更有活力
            LeanTween.scale(logoImage.gameObject, new Vector3(1.1f, 1.1f, 1f), openAnimDuration * 0.8f)
                .setEase(LeanTweenType.easeOutQuad)
                .setLoopPingPong(1)
                .setIgnoreTimeScale(true);
        }
        
        // 按钮依次动画
        AnimateButton(resumeButton, 0);
        AnimateButton(mainMenuButton, 1);
        AnimateButton(quitButton, 2);
        
        // 确保第一个按钮被选中
        Invoke("SelectFirstButton", openAnimDuration + 0.1f);
    }
    
    /// <summary>
    /// 设置按钮初始状态
    /// </summary>
    private void SetupButtonInitialState(Button button)
    {
        if (button == null) return;
        
        button.transform.localScale = Vector3.zero;
        
        // 如果按钮有文本组件，设置透明
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.alpha = 0f;
        }
    }
    
    /// <summary>
    /// 播放按钮动画
    /// </summary>
    private void AnimateButton(Button button, int index)
    {
        if (button == null) return;
        
        // 计算延迟
        float delay = buttonAnimDelay * index;
        
        // 缩放动画
        LeanTween.scale(button.gameObject, Vector3.one, openAnimDuration)
            .setDelay(delay)
            .setEase(LeanTweenType.easeOutBack)
            .setIgnoreTimeScale(true);
        
        // 文本淡入
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            LeanTween.value(buttonText.gameObject, 0f, 1f, openAnimDuration)
                .setDelay(delay)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnUpdate((float val) => { buttonText.alpha = val; })
                .setIgnoreTimeScale(true);
        }
    }
    
    /// <summary>
    /// 选择第一个可用的按钮
    /// </summary>
    private void SelectFirstButton()
    {
        if (resumeButton != null && resumeButton.gameObject.activeInHierarchy && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }
    
    /// <summary>
    /// 继续游戏按钮点击处理
    /// </summary>
    private void OnResumeClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    /// <summary>
    /// 返回主菜单按钮点击处理
    /// </summary>
    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
    }
    
    /// <summary>
    /// 退出游戏按钮点击处理
    /// </summary>
    private void OnQuitClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
    }
} 