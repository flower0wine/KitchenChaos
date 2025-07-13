using UnityEngine;

// 注意：为避免与Unity内置的CharacterController类冲突，你可能需要重命名此类
// 例如改为MyCharacterController或PlayerMovement
public class CharacterController : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("移动音效")]
    public AudioClip[] moveSounds;

    [Tooltip("移动速度")]
    public float moveSpeed = 8f;

    [Tooltip("旋转速度")]
    public float rotationSpeed = 10f;

    private int currentMoveSoundIndex = 0;
    private float moveSoundTimer = 0f;
    private float moveSoundInterval = 0.1f;

    private bool isWalking = false;
    private Rigidbody rb;

    private AudioSource audioSource;

    [Header("特效设置")]
    public ParticleSystem movementTrail;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 初始化粒子系统位置并清理
        if (movementTrail != null)
        {
            movementTrail.transform.position = transform.position;
            movementTrail.Clear(); // 清除任何已存在的粒子
        }
    }
    
    void FixedUpdate()
    {
        if (!GameManager.Instance.CanRunning()) return;

        HandleMovement();
    }
    
    void HandleMovement()
    {
        // 从InputManager获取移动输入
        Vector2 movementInput = InputManager.Instance.MovementInput;
        
        // 使用输入系统的值创建移动方向
        Vector3 movementDirection = new Vector3(movementInput.x, 0f, movementInput.y);
        
        // 更新行走状态
        isWalking = movementDirection.magnitude > 0.1f;
        
        if (isWalking)
        {
            // 计算朝向
            Quaternion targetRotation = Quaternion.LookRotation(-movementDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );
            
            // 移动角色
            rb.MovePosition(
                rb.position + movementDirection * moveSpeed * Time.fixedDeltaTime
            );

            moveSoundTimer += Time.fixedDeltaTime;
            if (moveSoundTimer >= moveSoundInterval)
            {
                PlayMoveSound();
                moveSoundTimer = 0f;
            }

            // 处理移动拖尾效果
            if (movementTrail != null)
            {
                // 确保粒子系统位置跟随角色
                movementTrail.transform.position = transform.position;
            }
        }
    }

    private void PlayMoveSound()
    {
        if (audioSource != null && moveSounds != null && moveSounds.Length > 0 && !audioSource.isPlaying)
        {
            audioSource.clip = moveSounds[currentMoveSoundIndex];
            audioSource.Play();
            currentMoveSoundIndex = (currentMoveSoundIndex + 1) % moveSounds.Length;
        }
    }
    
    public bool IsWalking()
    {
        return isWalking;
    }

}