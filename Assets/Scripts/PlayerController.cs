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
    [SerializeField] public float dashGravityAfter = 2f;

    //public bool CanDash => !isDashing && dashCooldownTimer <= 0 && dashRemaning > 0; // Til dash indikator

    [Header("WallJump")]
    [SerializeField] private bool isWallSliding;
    [SerializeField] private float wallSlideSpeed = 1f;
    [SerializeField] private float wallCheckRadius = 0.15f;
    private bool isFlipped;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;

    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    [SerializeField] private float wallJumpingDuration = 0.4f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 16f);



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

        WallJump();
        RefreshWallState();
        WallSlide();
        RefreshGroundState();
        HandleCrouchScale();
        
        if (!isWallJumping)
        {
           FlipSprite();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing && !isWallJumping)
        {
            ApplyMovement();
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // G�r s� den anden spiller ikke kan bev�ge sig n�r den ikke er aktiv
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

        // walljump
        if (wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = wallJumpingDirection < 0; // kig til rigtigt side efter walljump
                isFlipped = wallJumpingDirection < 0;
            }

            CancelInvoke(nameof(StopWallJumping));
            Invoke(nameof(StopWallJumping), wallJumpingDuration);
            return;
        }

        //normal
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
        if (isSprinting && !isCrouching) speed *= sprintMultiplier; // s�rger for at crouch ikke kan sprint
        if (isCrouching) speed *= crouchSpeedMultiplier; // s�rger for at crouch hastighed er mindre

        float decel = onIce ? iceDeceleration : normalDeceleration;

        if (moveInput.x == 0)
        {
            // Glidende stop p� is, ellers normalt stop
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

        if (isGrounded() && rb.linearVelocity.y <= 0)
        {
            jumpsRemaining = maxJumps;
            dashRemaning = maxDash;
            //isJumping = false;
        }
    }

    private void RefreshWallState()
    {
        if (wallCheck == null) return;

        float absX = Mathf.Abs(wallCheck.localPosition.x);

        if (isFlipped) // Flipper vores wallcheck den vej spilleren drejer
        {
            wallCheck.localPosition = new Vector3(-absX, wallCheck.localPosition.y, wallCheck.localPosition.z);
        }
        else
        {
            wallCheck.localPosition = new Vector3(absX, wallCheck.localPosition.y, wallCheck.localPosition.z);
        }
    }

    private void HandleCrouchScale()
    {
        Vector3 target = isCrouching ? new Vector3(originalScale.x, originalScale.y * crouchScaleY, originalScale.z) : originalScale; // hvis du croucher s�... ellers original scale

        transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * 15f); // smooth transition mellem courch og st�
    }

    private void FlipSprite()
    {
        if (spriteRenderer == null) return;

        float flipInput = Mathf.Abs(moveInput.x) > 0.01f ? moveInput.x : lookInput.x;

        if (flipInput > 0.01f)
        { 
            spriteRenderer.flipX = false;
            isFlipped = false;
        }

        else if (flipInput < -0.01f)
        {
            spriteRenderer.flipX = true;
            isFlipped = true;
        }
    }

    private void OnDrawGizmosSelected() //Debug til at se groundcheck radius i editor
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (wallCheck == null) return;
        //Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);

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

        rb.gravityScale = 0f; //sl� gravcity fra under dash
        rb.linearVelocity = dashDirection * dashForce;

        if (dashTrail != null) dashTrail.emitting = true; //dash effekt

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = dashGravityAfter;
        isDashing = false;

        if (dashTrail != null) dashTrail.emitting = false;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        onIce = collision.gameObject.CompareTag("Ice");
    }

    private bool isWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    private bool isGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer); 
    }

    private void WallSlide()
        {
        if (isWalled() && !isGrounded() && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWallSliding)
        {
            wallJumpingDirection = isFlipped ? 1f : -1f; // Bestem retning at hop i
            wallJumpingCounter = wallJumpingTime;
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }
}