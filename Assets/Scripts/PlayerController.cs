using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.6f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 1;

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchScaleY = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    // Player inputs
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool isCrouching;
    private int jumpsRemaining;

    private bool isActive = false;
    private bool isJumping;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    private void Start()
    {
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        RefreshGroundState();
        HandleCrouchScale();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // Gør så den anden spiller ikke kan bevæge sig når den ikke er aktiv
        if (!active)
        {
            moveInput = Vector2.zero;
            lookInput = Vector2.zero;
            isSprinting = false;
            isCrouching = false;
        }
    }

    public void ReceiveMove(Vector2 value)
    {
        if (!isActive) return;
        moveInput = value;
    }

    public void ReceiveLook(Vector2 value)
    {
        if (!isActive) return;
        lookInput = value;
    }

    public void ReceiveJump()
    {
        if (!isActive) return;
        if (jumpsRemaining <= 0) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        jumpsRemaining--;
    }

    public void ReceiveSprint(bool pressed)
    {
        if (!isActive) return;
        isSprinting = pressed;
    }

    public void ReceiveCrouch(bool pressed)
    {
        if (!isActive) return;
        isCrouching = pressed;
    }

    public void ReceiveAttack()
    {
        if (!isActive) return;
        Debug.Log("Attack!");
    }

    public void ReceiveInteract()
    {
        if (!isActive) return;
        Debug.Log("Interact!");
    }


    private void ApplyMovement()
    {
        float speed = moveSpeed;
        if (isSprinting && !isCrouching) speed *= sprintMultiplier; // sørger for at crouch ikke kan sprint
        if (isCrouching) speed *= crouchSpeedMultiplier; // sørger for at crouch hastighed er mindre

        rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
    }

    private void RefreshGroundState()
    {
        if (groundCheck == null) return;

        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (grounded && rb.linearVelocity.y <= 0)
        {
            jumpsRemaining = maxJumps;
            isJumping = false;
        }
    }

    private void HandleCrouchScale()
    {
        Vector3 target = isCrouching ? new Vector3(originalScale.x, originalScale.y * crouchScaleY, originalScale.z) : originalScale; // hvis du croucher så... ellers original scale

        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * 15f); // smooth transition mellem courch og stå
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        float flipInput = Mathf.Abs(moveInput.x) > 0.01f ? moveInput.x : lookInput.x;

        if (flipInput > 0.01f) spriteRenderer.flipX = false;
        else if (flipInput < -0.01f) spriteRenderer.flipX = true;
    }

    private void OnDrawGizmosSelected() //Debug til at se groundcheck radius i editor
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}