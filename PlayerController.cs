using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;
    
    [Header("Jump Settings")]
    public int maxJumps = 2; // 最大跳跃次数（2表示二段跳）
    private int jumpsLeft;   // 当前剩余跳跃次数

    [Header("Ground Detection")]
    public Transform groundCheck; 
    public LayerMask groundLayer; 
    public float checkRadius = 0.2f; // 把检测半径提取出来方便调整

    private Rigidbody2D rb;
    private bool isGrounded;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsLeft = maxJumps;
    }

    void Update()
    {
        // 1. 地面检测
        // bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);

        // 2. 落地重置逻辑：
        // 如果当前在地面上，且垂直速度接近0（防止刚起跳瞬间被判定为落地），则重置次数
        if (isGrounded && rb.linearVelocity.y <= 0.1f && rb.linearVelocity.y >= 0f)
        {
            jumpsLeft = maxJumps;
        }

        // 3. 移动输入
        moveInput = Input.GetAxisRaw("Horizontal");

        // 4. 跳跃逻辑修改
        if (Input.GetButtonDown("Jump"))
        {
            // 只要还有剩余次数，就可以跳
            if (jumpsLeft > 0)
            {
                Jump();
                jumpsLeft = jumpsLeft - 1; // 消耗一次机会
            }
        }
    }

    void Jump()
    {
        // 直接设置垂直速度，确保二段跳手感一致（抵消下落惯性）
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // 面朝向翻转
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
    
    // 可视化辅助线：在 Scene 窗口里画个圈，方便调节 GroundCheck 大小
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}