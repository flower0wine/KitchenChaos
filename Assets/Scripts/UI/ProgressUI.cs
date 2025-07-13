using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private Image backgroundImage;
    
    private Camera mainCamera;
    private Canvas canvas;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        canvas = GetComponent<Canvas>();
        
        if (fillImage == null)
        {
            fillImage = transform.Find("Bar")?.GetComponent<Image>() 
                     ?? transform.Find("Fill")?.GetComponent<Image>();
            
            if (fillImage == null)
            {
                Debug.LogError("进度条填充图像未找到，请手动分配");
            }
        }
        
        // 初始隐藏进度条
        SetActive(false);
    }
    
    private void LateUpdate()
    {
        // 让进度条始终面向相机
        if (mainCamera != null)
        {
            transform.forward = mainCamera.transform.forward;
        }
    }
    
    public void SetProgress(float normalizedProgress)
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = normalizedProgress;
        }
    }
    
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
} 