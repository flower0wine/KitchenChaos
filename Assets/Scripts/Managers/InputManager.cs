using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// 集中式输入管理器，负责处理所有输入并通过事件广播给其他组件
/// </summary>
public class InputManager : MonoBehaviour, PlayerInputActions.IPlayerActions
{
    // 单例模式
    public static InputManager Instance { get; private set; }

    // 输入系统
    private PlayerInputActions inputActions;

    // 交互事件
    public event Action OnInteractPerformed;
    // 切菜事件
    public event Action OnCutPerformed;
    // 暂停事件
    public event Action OnPausePerformed;
    
    // 移动输入
    public Vector2 MovementInput { get; private set; }
    
    // 交互输入状态
    public bool IsInteracting { get; private set; }
    // 切菜输入状态
    public bool IsCutting { get; private set; }

    private void Awake()
    {
        // 单例模式设置
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // 确保在场景切换时不被销毁
        DontDestroyOnLoad(gameObject);
        
        // 初始化输入系统
        InitializeInputActions();
    }
    
    private void InitializeInputActions()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
            inputActions.Player.SetCallbacks(this);
        }
    }

    private void OnEnable()
    {
        InitializeInputActions();
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Player.Disable();
        }
    }

    // 实现IPlayerActions接口的方法
    public void OnInteract(InputAction.CallbackContext context)
    {
        // 当交互按键按下时
        if (context.performed)
        {
            IsInteracting = true;
            OnInteractPerformed?.Invoke();
        }
        else if (context.canceled)
        {
            IsInteracting = false;
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        // 更新移动输入值
        MovementInput = context.ReadValue<Vector2>();
    }
    
    // 新增切菜输入回调方法
    public void OnCut(InputAction.CallbackContext context)
    {
        // 当切菜按键按下时
        if (context.performed)
        {
            IsCutting = true;
            OnCutPerformed?.Invoke(); // 恢复事件触发
        }
        else if (context.canceled)
        {
            IsCutting = false;
        }
    }
    
    // 重置所有输入状态（在场景切换或游戏暂停时调用）
    public void ResetInputState()
    {
        MovementInput = Vector2.zero;
        IsInteracting = false;
        IsCutting = false; // 添加切菜状态重置
    }

    // 实现暂停输入回调方法
    public void OnPause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnPausePerformed?.Invoke();
        }
    }
}
