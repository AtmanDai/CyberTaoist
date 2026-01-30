using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;

    [Header("Hand Tracking Reference")]
    public HandSignReceiver handSignReceiver;

    [Header("Screen Mapping")]
    [Tooltip("Dead zone in the center of screen where no movement occurs")]
    public float deadZone = 0.1f;
    [Tooltip("Maximum normalized distance from center for full speed")]
    public float maxDistance = 0.4f;

    private Rigidbody2D rb;
    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Try to find HandSignReceiver if not assigned
        if (handSignReceiver == null)
        {
            handSignReceiver = FindFirstObjectByType<HandSignReceiver>();
        }
    }

    void Update()
    {
        // Get movement input from hand tracking or keyboard fallback
        moveInput = GetMovementInput();
    }

    /// <summary>
    /// Get movement input from left index finger position or keyboard fallback
    /// </summary>
    private float GetMovementInput()
    {
        // Priority 1: Hand tracking (left index finger)
        if (handSignReceiver != null && handSignReceiver.hasValidMovementData)
        {
            // leftIndexX is normalized 0-1 (0 = left, 1 = right)
            // Convert to -1 to 1 range relative to center (0.5)
            float normalizedX = handSignReceiver.leftIndexX;
            float offsetFromCenter = normalizedX - 0.5f;
            
            // Apply dead zone
            if (Mathf.Abs(offsetFromCenter) < deadZone)
            {
                return 0f;
            }
            
            // Calculate movement intensity based on distance from center
            float sign = Mathf.Sign(offsetFromCenter);
            float magnitude = Mathf.Abs(offsetFromCenter) - deadZone;
            float normalizedMagnitude = Mathf.Clamp01(magnitude / (maxDistance - deadZone));
            
            return sign * normalizedMagnitude;
        }
        
        // Priority 2: Keyboard input as fallback
        return Input.GetAxisRaw("Horizontal");
    }

    void FixedUpdate()
    {
        // Apply horizontal movement only (2D plane, no jumping)
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        
        // Flip sprite based on movement direction
        if (moveInput > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0) transform.localScale = new Vector3(-1, 1, 1);
    }
}
