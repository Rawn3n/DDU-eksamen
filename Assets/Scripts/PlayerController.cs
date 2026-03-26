using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float sprintMultiplier = 1.6f;
    public bool isRunning;

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
    [SerializeField] private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private bool isHoldingWallSlide;

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

    public Vector2 distractionInput;

    // Animations
    private Animator animator;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalScale = transform.localScale;

        dashTrail = GetComponentInChildren<TrailRenderer>();
        if (dashTrail != null) dashTrail.emitting = false;

        audioSource = GetComponent<AudioSource>();

        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        dashRemaning = maxDash;
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {

        isRunning = Mathf.Abs(rb.linearVelocity.x) > 0.01f && !isCrouching && !isDashing && isGrounded() && !isWallSliding && !isWallJumping;
        Debug.Log("IsRunning: " + isRunning);

        if (animator != null) // lav løbe animation
        {
            animator.SetBool("isRunning", isRunning);
        }



        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }


        if (!isDashing && dashCooldownTimer <= 0 && dashRemaning > 0 && isWallSliding == false) // Til dash indikator
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

        if (!isDashing && !isWallJumping)
        {
            ApplyMovement();
        }
    }

    private void FixedUpdate()
    {
        //if (!isDashing && !isWallJumping)
        //{
        //    ApplyMovement();
        //}
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
            isHoldingWallSlide = false;
            isRunning = false;
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

        //wall jump
        if (wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            rb.linearVelocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            wallJumpingCounter = 0f;
            coyoteTimeCounter = 0f;

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = wallJumpingDirection < 0; // Vend sprite den rigtige vej efter wall jump
                isFlipped = wallJumpingDirection < 0;
            }

            CancelInvoke(nameof(StopWallJumping));
            Invoke(nameof(StopWallJumping), wallJumpingDuration); // Stop wall jump state efter kort tid
            return; // Undgå at normal hop logik også kører samtidigt
        }

        // normal jump
        if (coyoteTimeCounter <= 0f) return;
        if (jumpsRemaining <= 0) return;
        rb.gravityScale = dashGravityAfter; // sørger for at man rester gravity før man kan hoppe

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Nulstil Y så hop altid føles ens
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpsRemaining--;
        coyoteTimeCounter = 0f; // coyote time = 0 så man ikke kan hoppe igen
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

    public void ReceiveWallSlide(bool pressed)
    {
        if (!isActive) return;
        isHoldingWallSlide = pressed;
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
        if (isWallSliding) return;
        if (dashCooldownTimer > 0) return;
        if (dashRemaning <= 0) return;

        StartCoroutine(DashCoroutine());
    }

    private void ApplyMovement()
    {
        //isRunning = Mathf.Abs(moveInput.x) > 0.01f && !isCrouching && !isDashing;
        //Debug.Log("IsRunning: " + isRunning);
        float speed = moveSpeed;
        if (isSprinting && !isCrouching) speed *= sprintMultiplier; // sorger for at crouch ikke kan sprint
        if (isCrouching) speed *= crouchSpeedMultiplier; // sorger for at crouch hastighed er mindre

        float decel = onIce ? iceDeceleration : normalDeceleration; // vælger deceleration baseret på vores onIce bool

        Vector2 combinedInput = moveInput + distractionInput; // Spiller kan kæmpe imod distraction
        combinedInput.x = Mathf.Clamp(combinedInput.x, -1f, 1f);

        if (combinedInput.x == 0)
        {
            // Glidende stop på is, ellers normalt stop
            rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, decel * Time.deltaTime), rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(combinedInput.x * speed, rb.linearVelocity.y);
        }
    }

    private void RefreshGroundState()
    {
        if (groundCheck == null) return;

        bool grounded = isGrounded();

        // Udnyt coyote time til at give spilleren en buffer
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else if (!isWallSliding)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (grounded && rb.linearVelocity.y <= 0)
        {
            jumpsRemaining = maxJumps;
            if (!isDashing)
            {
                dashRemaning = maxDash;
            }
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
        dashRemaning--;
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
        if (isWalled() && !isGrounded() && rb.linearVelocity.y < 0 && isHoldingWallSlide == true)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (isWalled() && !isGrounded())
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>());
        }
    }
}