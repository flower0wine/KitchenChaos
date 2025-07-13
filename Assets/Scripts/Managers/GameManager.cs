using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

/// <summary>
/// 游戏管理器，负责游戏状态和数据管理
/// </summary>
public class GameManager : MonoBehaviour
{
    // 单例实例
    public static GameManager Instance { get; private set; }
    
    // 游戏状态
    public enum GameState
    {
        NotStarted,  // 未开始
        CountDown,   // 倒计时中
        Playing,     // 游戏进行中
        Paused,      // 游戏暂停
        GameOver     // 游戏结束
    }
    
    [Header("游戏设置")]
    [Tooltip("游戏时长(秒)")]
    public float gameTime = 300f;
    
    [Tooltip("倒计时时长(秒)")]
    public float countdownTime = 3f;
    
    [Header("UI引用")]
    [Tooltip("游戏开始倒计时UI")]
    public GameStartUI gameStartUI;
    
    [Tooltip("游戏结束UI")]
    public GameOverUI gameOverUI;
    
    [Tooltip("暂停菜单UI")]
    public PauseMenuUI pauseMenuUI;
    
    [Header("音频设置")]
    [Tooltip("暂停音效")]
    public AudioClip pauseSound;
    
    [Tooltip("恢复音效")]
    public AudioClip resumeSound;
    
    [Tooltip("BGM 音频源引用")]
    public AudioSource bgmAudioSource;
    
    // 游戏暂停/恢复时的音效播放器
    private AudioSource pauseAudioSource;
    
    // 当前游戏状态
    private GameState currentState = GameState.NotStarted;
    
    // 游戏数据
    private float remainingTime = 0f;
    
    // 游戏状态改变事件
    public event Action<GameState> OnGameStateChanged;
    
    // 游戏数据改变事件
    public event Action<float> OnTimeChanged;     // 参数：剩余时间
    
    // 暂停状态改变事件
    public event Action<bool> OnPauseStateChanged;
    
    // 添加 GameLoader 引用
    private GameLoader gameLoader;
    
    // 游戏暂停前的状态
    private GameState stateBeforePause;
    
    private void Awake()
    {
        // 单例模式
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // 获取 GameLoader 组件
        gameLoader = GetComponent<GameLoader>();
        if (gameLoader == null)
        {
            gameLoader = gameObject.AddComponent<GameLoader>();
        }
        
        // 初始化暂停音效播放器
        pauseAudioSource = gameObject.AddComponent<AudioSource>();
        pauseAudioSource.playOnAwake = false;
        pauseAudioSource.loop = false;
        
        // 初始化游戏状态
        SetGameState(GameState.NotStarted);
    }
    
    private void Start()
    {
        // 如果未指定UI，尝试查找
        if (gameStartUI == null)
            gameStartUI = FindObjectOfType<GameStartUI>();
        
        if (gameOverUI == null)
            gameOverUI = FindObjectOfType<GameOverUI>();
            
        if (pauseMenuUI == null)
            pauseMenuUI = FindObjectOfType<PauseMenuUI>(true);
        
        // 初始化UI
        if (gameStartUI != null)
        {
            gameStartUI.SetCountdownTime(countdownTime);
            gameStartUI.gameObject.SetActive(false);
        }
        
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(false);
        }
        
        if (pauseMenuUI != null)
        {
            pauseMenuUI.gameObject.SetActive(false);
        }
        
        // 订阅输入事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePerformed += TogglePause;
        }
        
        // 默认开始游戏
        StartGame();
    }
    
    private void OnDestroy()
    {
        // 取消订阅输入事件
        if (InputManager.Instance != null)
        {
            InputManager.Instance.OnPausePerformed -= TogglePause;
        }
    }
    
    private void Update()
    {
        if (currentState == GameState.Playing)
        {
            // 更新剩余时间
            remainingTime -= Time.deltaTime;
            
            // 触发时间改变事件
            OnTimeChanged?.Invoke(remainingTime);
            
            // 检查游戏是否结束
            if (remainingTime <= 0)
            {
                EndGame();
            }
        }
    }
    
    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        // 重置游戏数据
        remainingTime = gameTime;
        
        // 重置分数
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        
        // 触发时间改变事件
        OnTimeChanged?.Invoke(remainingTime);
        
        // 开始倒计时
        StartCoroutine(StartCountdown());
    }
    
    /// <summary>
    /// 开始倒计时
    /// </summary>
    private IEnumerator StartCountdown()
    {
        // 设置状态为倒计时
        SetGameState(GameState.CountDown);
        
        // 显示开始UI
        if (gameStartUI != null)
        {
            gameStartUI.gameObject.SetActive(true);
            gameStartUI.StartCountdown();
        }
        
        // 等待倒计时完成
        yield return new WaitForSeconds(countdownTime);
        
        // 隐藏开始UI
        if (gameStartUI != null)
        {
            gameStartUI.gameObject.SetActive(false);
        }
        
        // 设置状态为游戏中
        SetGameState(GameState.Playing);
    }
    
    /// <summary>
    /// 结束游戏
    /// </summary>
    public void EndGame()
    {
        // 设置状态为游戏结束
        SetGameState(GameState.GameOver);
        
        // 获取最终分数
        int finalDeliveredCount = 0;
        int finalScore = 0;
        
        if (ScoreManager.Instance != null)
        {
            (finalDeliveredCount, finalScore) = ScoreManager.Instance.GetScore();
        }
        
        // 显示结束UI
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(true);
            gameOverUI.SetGameResult(finalDeliveredCount, finalScore);
        }
    }
    
    /// <summary>
    /// 重新开始游戏
    /// </summary>
    public void RestartGame()
    {
        // 隐藏结束UI
        if (gameOverUI != null)
        {
            gameOverUI.gameObject.SetActive(false);
        }
        
        // 使用 GameLoader 重载当前场景
        if (gameLoader != null)
        {
            gameLoader.ReloadCurrentScene();
        }
        else
        {
            Debug.LogError("GameLoader不存在，无法重新加载场景");
        }
    }
    
    /// <summary>
    /// 切换暂停状态
    /// </summary>
    public void TogglePause()
    {
        // 只在游戏中或倒计时时允许暂停
        if (currentState != GameState.Playing && currentState != GameState.CountDown && currentState != GameState.Paused)
            return;
            
        if (currentState == GameState.Paused)
            ResumeGame();
        else
            PauseGame();
    }
    
    /// <summary>
    /// 暂停游戏
    /// </summary>
    public void PauseGame()
    {
        // 保存当前状态
        stateBeforePause = currentState;
        
        // 设置状态为暂停
        SetGameState(GameState.Paused);
        
        // 暂停BGM
        PauseBGM();
        
        // 播放暂停音效
        PlayPauseSound();
        
        // 暂停时间
        Time.timeScale = 0f;
        
        // 显示暂停菜单
        if (pauseMenuUI != null)
        {
            pauseMenuUI.gameObject.SetActive(true);
            pauseMenuUI.PlayOpenAnimation();
        }
        
        // 触发暂停事件
        OnPauseStateChanged?.Invoke(true);
        
        // 重置输入状态
        if (InputManager.Instance != null)
        {
            InputManager.Instance.ResetInputState();
        }
        
        Debug.Log("游戏已暂停");
    }
    
    /// <summary>
    /// 恢复游戏
    /// </summary>
    public void ResumeGame()
    {
        // 播放恢复音效
        PlayResumeSound();
        
        // 恢复BGM
        ResumeBGM();
        
        // 恢复时间
        Time.timeScale = 1f;
        
        // 隐藏暂停菜单
        if (pauseMenuUI != null)
        {
            pauseMenuUI.gameObject.SetActive(false);
        }
        
        // 恢复之前的状态
        SetGameState(stateBeforePause);
        
        // 触发恢复事件
        OnPauseStateChanged?.Invoke(false);
        
        Debug.Log("游戏已恢复");
    }
    
    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    private void PauseBGM()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Pause();
            Debug.Log("BGM已暂停");
        }
    }
    
    /// <summary>
    /// 恢复背景音乐
    /// </summary>
    private void ResumeBGM()
    {
        if (bgmAudioSource != null && !bgmAudioSource.isPlaying)
        {
            bgmAudioSource.UnPause();
            Debug.Log("BGM已恢复");
        }
    }
    
    /// <summary>
    /// 播放暂停音效
    /// </summary>
    private void PlayPauseSound()
    {
        if (pauseSound != null && pauseAudioSource != null)
        {
            // 设置暂停音效不受时间缩放影响
            pauseAudioSource.ignoreListenerPause = true;
            pauseAudioSource.ignoreListenerVolume = true;
            
            pauseAudioSource.PlayOneShot(pauseSound);
            
            Debug.Log("播放暂停音效");
        }
    }
    
    /// <summary>
    /// 播放恢复音效
    /// </summary>
    private void PlayResumeSound()
    {
        if (resumeSound != null && pauseAudioSource != null)
        {
            // 设置恢复音效不受时间缩放影响
            pauseAudioSource.ignoreListenerPause = true;
            pauseAudioSource.ignoreListenerVolume = true;
            
            pauseAudioSource.PlayOneShot(resumeSound);
            
            Debug.Log("播放恢复音效");
        }
    }
    
    /// <summary>
    /// 返回主菜单
    /// </summary>
    public void ReturnToMainMenu()
    {
        // 恢复正常时间流逝
        Time.timeScale = 1f;
        
        // 加载主菜单场景
        if (gameLoader != null)
        {
            gameLoader.LoadScene("MainMenuScene");
        }
        else
        {
            // 备用方案
            SceneManager.LoadScene("MainMenuScene");
        }
    }
    
    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
        // 恢复正常时间流逝
        Time.timeScale = 1f;
        
        Debug.Log("退出游戏");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    /// <summary>
    /// 设置游戏状态
    /// </summary>
    private void SetGameState(GameState newState)
    {
        if (currentState != newState)
        {
            currentState = newState;
            
            // 触发状态改变事件
            OnGameStateChanged?.Invoke(currentState);
        }
    }
    
    /// <summary>
    /// 获取当前游戏状态
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// 获取当前游戏数据
    /// </summary>
    public float GetGameData()
    {
        return remainingTime;
    }

    /// <summary>
    /// 检查游戏是否处于运行状态
    /// </summary>
    public bool CanRunning()
    {
        return currentState == GameState.Playing;
    }
    
    /// <summary>
    /// 检查游戏是否已暂停
    /// </summary>
    public bool IsPaused()
    {
        return currentState == GameState.Paused;
    }
} 