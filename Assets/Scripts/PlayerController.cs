using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.6f;

    [Header("Jump")]
    public float jumpForce = 12f;
    public int maxJumps = 1;
    private int jumpsRemaining;

    [Header("Crouch")]
    public float crouchSpeedMultiplier = 0.5f;
    public float crouchScaleY = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;
    public float dashVerticalMultiplier = 0.5f;
    public int maxDash = 1;
    private int dashRemaning;
    public bool canDash;
    public AudioClip dashLyd;

    //public bool CanDash => !isDashing && dashCooldownTimer <= 0 && dashRemaning > 0; // Til dash indikator

    [Header("Ice")]
    public float normalDeceleration = 10f;
    public float iceDeceleration = 1f;  // jo lavere jo mere glidende
    private bool onIce = false;

    private bool isDashing;
    private float dashCooldownTimer;
    private TrailRenderer dashTrail;

    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    // Player inputs
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting;
    private bool isCrouching;

    private bool isActive = false;
    //private bool isJumping;

    private AudioSource audioSource;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;

        dashTrail = GetComponentInChildren<TrailRenderer>();
        if (dashTrail != null) dashTrail.emitting = false;

        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        dashRemaning = maxDash;
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }


        if (!isDashing && dashCooldownTimer <= 0 && dashRemaning > 0) // Til dash indikator
        {
            canDash = true;
        }
        else
        {
            canDash = false;
        }

        RefreshGroundState();
        HandleCrouchScale();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            ApplyMovement();
        }
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
    public void ReceiveDash()
    {
        if (!isActive) return;
        if (isDashing) return;
        if (dashCooldownTimer > 0) return;
        if (dashRemaning <= 0) return;

        StartCoroutine(DashCoroutine());

        dashRemaning--;
    }

    private void ApplyMovement()
    {
        float speed = moveSpeed;
        if (isSprinting && !isCrouching) speed *= sprintMultiplier; // sørger for at crouch ikke kan sprint
        if (isCrouching) speed *= crouchSpeedMultiplier; // sørger for at crouch hastighed er mindre

        float decel = onIce ? iceDeceleration : normalDeceleration;

        if (moveInput.x == 0)
        {
            // Glidende stop på is, ellers normalt stop
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, decel * Time.deltaTime), rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed, rb.linearVelocity.y);
        }
    }

    private void RefreshGroundState()
    {
        if (groundCheck == null) return;

        bool grounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (grounded && rb.linearVelocity.y <= 0)
        {
            jumpsRemaining = maxJumps;
            dashRemaning = maxDash;
            //isJumping = false;
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

    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        if (dashLyd != null && audioSource != null) // afpil dash lyd
        {
            audioSource.PlayOneShot(dashLyd);
        }

        dashCooldownTimer = dashCooldown;

        Vector2 dashDirection = moveInput.magnitude > 0.1f ? moveInput.normalized : new Vector2(spriteRenderer.flipX ? -1f : 1f, 0f);

        dashDirection = new Vector2(dashDirection.x, dashDirection.y * dashVerticalMultiplier);

        rb.gravityScale = 0f; //slå gravcity fra under dash
        rb.linearVelocity = dashDirection * dashForce;

        if (dashTrail != null) dashTrail.emitting = true; //dash effekt

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = 1f;
        isDashing = false;

        if (dashTrail != null) dashTrail.emitting = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        onIce = collision.gameObject.CompareTag("Ice");
    }
}