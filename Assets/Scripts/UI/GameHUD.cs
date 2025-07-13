using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// 游戏HUD，显示分数和时间
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("分数显示")]
    [Tooltip("分数文本")]
    public TextMeshProUGUI scoreText;
    
    [Tooltip("分数图标")]
    public Image scoreIcon;
    
    [Tooltip("分数面板")]
    public RectTransform scorePanel;
    
    [Header("时间显示")]
    [Tooltip("时间文本")]
    public TextMeshProUGUI timeText;
    
    [Tooltip("时间图标")]
    public Image timeIcon;
    
    [Tooltip("时间条")]
    public Image timeBar;
    
    [Tooltip("时间条背景")]
    public Image timeBarBg;
    
    [Header("动画设置")]
    [Tooltip("分数变化动画持续时间")]
    public float scoreAnimDuration = 0.25f;
    
    [Tooltip("UI入场动画持续时间")]
    public float introAnimDuration = 0.5f;
    
    [Header("警告设置")]
    [Tooltip("时间不足警告阈值(秒)")]
    public float timeWarningThreshold = 30f;
    
    [Tooltip("闪烁间隔(秒)")]
    public float warningBlinkInterval = 0.5f;
    
    [Tooltip("警告颜色")]
    public Color warningColor = Color.red;
    
    // 内部变量
    private int currentScore = 0;
    private int targetScore = 0;
    private float currentTime = 0f;
    private float maxTime;
    private Color normalTimeColor;
    private int timeWarningTweenId = -1;
    private bool isWarningActive = false;
    
    private void Awake()
    {
        // 初始化文本
        if (scoreText != null)
        {
            scoreText.text = "0";
        }
        
        // 初始化时间条
        if (timeBar != null)
        {
            timeBar.fillAmount = 1f;
        }

        UpdateTimeText(maxTime);
        
        // 准备UI入场动画的初始状态
        PrepareIntroAnimation();
    }
    
    private void Start()
    {
        // 订阅事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged += UpdateTime;
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            
            // 初始化最大时间
            maxTime = GameManager.Instance.gameTime;
        }
        
        // 订阅分数管理器事件
        if (ScoreManager.Instance != null)
        {
            Debug.Log("GameHUD: 订阅 ScoreManager 的分数事件");
            ScoreManager.Instance.OnScoreChanged += UpdateScore;
        }
        else
        {
            Debug.LogError("GameHUD: ScoreManager.Instance 为空，无法订阅事件");
        }
        
        // 播放入场动画
        PlayIntroAnimation();
    }
    
    private void OnDestroy()
    {
        // 取消订阅事件
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnTimeChanged -= UpdateTime;
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScore;
        }
        
        // 停止所有LeanTween动画
        LeanTween.cancel(gameObject);
        
        if (scorePanel != null)
            LeanTween.cancel(scorePanel.gameObject);
            
        if (scoreText != null)
            LeanTween.cancel(scoreText.gameObject);
            
        if (timeText != null)
            LeanTween.cancel(timeText.gameObject);
            
        if (timeBar != null)
            LeanTween.cancel(timeBar.gameObject);
    }
    
    /// <summary>
    /// 处理游戏状态变化
    /// </summary>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Playing)
        {
            // 游戏开始，播放入场动画
            PlayIntroAnimation();
        }
        else if (newState == GameManager.GameState.GameOver)
        {
            // 游戏结束，播放退场动画
            PlayOutroAnimation();
        }
    }
    
    /// <summary>
    /// 准备UI入场动画的初始状态
    /// </summary>
    private void PrepareIntroAnimation()
    {
        // 将时间条缩放为0
        if (timeBar != null)
        {
            timeBar.transform.localScale = new Vector3(0, 1, 1);
        }
        
        // 将时间文本透明度设为0
        if (timeText != null)
        {
            Color textColor = timeText.color;
            timeText.color = new Color(textColor.r, textColor.g, textColor.b, 0);
        }
    }
    
    /// <summary>
    /// 播放UI入场动画
    /// </summary>
    private void PlayIntroAnimation()
    {
        // 时间条展开动画
        if (timeBar != null)
        {
            LeanTween.scaleX(timeBar.gameObject, 1f, introAnimDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setDelay(0.3f);
        }
        
        // 时间文本淡入动画
        if (timeText != null)
        {
            Color targetColor = timeText.color;
            Color startColor = new Color(targetColor.r, targetColor.g, targetColor.b, 0);
            timeText.color = startColor;
            
            LeanTween.value(timeText.gameObject, 0f, 1f, introAnimDuration)
                .setEase(LeanTweenType.easeOutQuad)
                .setDelay(0.4f)
                .setOnUpdate((float val) => {
                    timeText.color = new Color(targetColor.r, targetColor.g, targetColor.b, val);
                });
        }
        
        // 图标弹出动画
        if (scoreIcon != null)
        {
            scoreIcon.transform.localScale = Vector3.zero;
            LeanTween.scale(scoreIcon.gameObject, Vector3.one, introAnimDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.5f);
        }
        
        if (timeIcon != null)
        {
            timeIcon.transform.localScale = Vector3.zero;
            LeanTween.scale(timeIcon.gameObject, Vector3.one, introAnimDuration)
                .setEase(LeanTweenType.easeOutBack)
                .setDelay(0.6f);
        }
    }
    
    /// <summary>
    /// 播放UI退场动画
    /// </summary>
    private void PlayOutroAnimation()
    {
        // 时间条收缩动画
        if (timeBar != null)
        {
            LeanTween.scaleX(timeBar.gameObject, 0f, introAnimDuration)
                .setEase(LeanTweenType.easeInQuad);
        }
        
        // 时间文本淡出动画
        if (timeText != null)
        {
            Color startColor = timeText.color;
            
            LeanTween.value(timeText.gameObject, 1f, 0f, introAnimDuration)
                .setEase(LeanTweenType.easeInQuad)
                .setOnUpdate((float val) => {
                    timeText.color = new Color(startColor.r, startColor.g, startColor.b, val);
                });
        }
        
        // 图标缩小动画
        if (scoreIcon != null)
        {
            LeanTween.scale(scoreIcon.gameObject, Vector3.zero, introAnimDuration)
                .setEase(LeanTweenType.easeInBack);
        }
        
        if (timeIcon != null)
        {
            LeanTween.scale(timeIcon.gameObject, Vector3.zero, introAnimDuration)
                .setEase(LeanTweenType.easeInBack);
        }
    }
    
    /// <summary>
    /// 更新分数显示
    /// </summary>
    private void UpdateScore(int deliveredCount, int totalScore)
    {
        targetScore = totalScore;
        
        // 使用LeanTween更新分数
        if (scoreText != null)
        {
            // 停止之前的动画
            LeanTween.cancel(scoreText.gameObject, false);
            
            // 分数增加时的动画
            if (targetScore > currentScore)
            {
                // 放大缩小动画
                LeanTween.scale(scoreText.gameObject, Vector3.one * 1.3f, scoreAnimDuration * 0.5f)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnComplete(() => {
                        LeanTween.scale(scoreText.gameObject, Vector3.one, scoreAnimDuration * 0.5f)
                            .setEase(LeanTweenType.easeInOutQuad);
                    });
                
                // 数值变化动画
                LeanTween.value(scoreText.gameObject, currentScore, targetScore, scoreAnimDuration)
                    .setEase(LeanTweenType.easeOutQuad)
                    .setOnUpdate((float val) => {
                        scoreText.text = Mathf.FloorToInt(val).ToString();
                    })
                    .setOnComplete(() => {
                        currentScore = targetScore;
                        scoreText.text = targetScore.ToString();
                    });
            }
            else
            {
                // 直接设置分数
                currentScore = targetScore;
                scoreText.text = targetScore.ToString();
            }
        }
    }
    
    /// <summary>
    /// 更新时间显示
    /// </summary>
    private void UpdateTime(float remainingTime)
    {
        currentTime = remainingTime;
        
        UpdateTimeText(remainingTime);
        
        // 使用LeanTween平滑更新时间条
        if (timeBar != null)
        {
            LeanTween.value(timeBar.gameObject, timeBar.fillAmount, remainingTime / maxTime, 0.2f)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnUpdate((float val) => {
                    timeBar.fillAmount = val;
                });
        }
        
        // 检查是否需要显示警告
        CheckTimeWarning(remainingTime);
    }

    private void UpdateTimeText(float remainingTime)
    {
        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            if (minutes < 0)
            {
                minutes = 0;
            }
            if (seconds < 0)
            {
                seconds = 0;
            }
            timeText.text = $"{minutes}:{seconds:00}";
        }
    }
    
    /// <summary>
    /// 检查时间警告
    /// </summary>
    private void CheckTimeWarning(float remainingTime)
    {
        if (remainingTime <= timeWarningThreshold && !isWarningActive)
        {
            // 时间不足，启动警告
            StartTimeWarning();
        }
        else if (remainingTime > timeWarningThreshold && isWarningActive)
        {
            // 时间充足，停止警告
            StopTimeWarning();
        }
    }
    
    /// <summary>
    /// 启动时间警告
    /// </summary>
    private void StartTimeWarning()
    {
        isWarningActive = true;
        
        // 停止之前的警告
        if (timeWarningTweenId != -1)
        {
            LeanTween.cancel(timeWarningTweenId);
        }
        
        // 时间文本闪烁
        if (timeText != null)
        {
            // 保存正常颜色
            Color normalColor = timeText.color;
            
            // 创建循环闪烁动画
            timeWarningTweenId = LeanTween.value(timeText.gameObject, 0f, 1f, warningBlinkInterval)
                .setLoopPingPong()
                .setOnUpdate((float val) => {
                    timeText.color = Color.Lerp(normalColor, warningColor, val);
                })
                .id;
        }
        
        // 时间条闪烁
        if (timeBar != null)
        {
            LeanTween.value(timeBar.gameObject, 0f, 1f, warningBlinkInterval)
                .setLoopPingPong()
                .setOnUpdate((float val) => {
                    timeBar.color = Color.Lerp(Color.white, warningColor, val);
                });
        }
    }
    
    /// <summary>
    /// 停止时间警告
    /// </summary>
    private void StopTimeWarning()
    {
        isWarningActive = false;
        
        // 停止所有警告动画
        if (timeText != null)
        {
            LeanTween.cancel(timeText.gameObject, false);
            timeText.color = normalTimeColor;
        }
        
        if (timeBar != null)
        {
            LeanTween.cancel(timeBar.gameObject, false);
            timeBar.color = Color.white;
        }
        
        timeWarningTweenId = -1;
    }
} 