using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 游戏场景加载器，用于处理场景过渡和重新加载
/// </summary>
public class GameLoader : MonoBehaviour
{
    [Tooltip("加载场景名称")]
    public string loadingSceneName = "LoadingScene";
    
    [Tooltip("淡出动画持续时间")]
    public float fadeOutDuration = 0.5f;
    
    /// <summary>
    /// 重新加载当前游戏场景
    /// </summary>
    public void ReloadCurrentScene()
    {
        // 获取当前场景名
        string currentSceneName = SceneManager.GetActiveScene().name;
        
        // 加载场景
        LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// 加载指定场景
    /// </summary>
    public void LoadScene(string sceneName)
    {
        // 保存目标场景名称
        PlayerPrefs.SetString("TargetSceneName", sceneName);
        PlayerPrefs.Save();
        
        // 开始过渡
        StartCoroutine(TransitionToLoadingScene());
    }
    
    /// <summary>
    /// 过渡到加载场景
    /// </summary>
    private IEnumerator TransitionToLoadingScene()
    {
        int colorR = 0;
        int colorG = 0;
        int colorB = 0;
        // 创建淡出效果
        GameObject fadeCanvas = new GameObject("FadeCanvas");
        Canvas canvas = fadeCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;
        
        GameObject fadeImage = new GameObject("FadeImage");
        fadeImage.transform.SetParent(fadeCanvas.transform, false);
        UnityEngine.UI.Image image = fadeImage.AddComponent<UnityEngine.UI.Image>();
        image.color = new Color(colorR, colorG, colorB, 0);
        
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
            image.color = new Color(colorR, colorG, colorB, alpha);
            yield return null;
        }
        
        // 确保完全黑色
        image.color = new Color(colorR, colorG, colorB, 1);
        
        // 加载加载场景
        SceneManager.LoadScene(loadingSceneName);
    }
} 