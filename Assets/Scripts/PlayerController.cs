using Assets.Scripts;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    #region Inspector variables / Character properties

    /// <summary>
    /// The character's run speed.
    /// </summary>
    [Tooltip("The character's run speed.")]
    public float RunSpeed = 4f;

    /// <summary>
    /// The impulse force to apply for a short hop.
    /// </summary>
    [Tooltip("The impulse force to apply for a short hop.")]
    public float ShortHopImpulse = 4f;

    /// <summary>
    /// The impulse force to apply for a full hop.
    /// </summary>
    [Tooltip("The impulse force to apply for a full hop.")]
    public float FullHopImpulse = 5.5f;

    /// <summary>
    /// The fall gravity multiplier.
    /// </summary>
    [Tooltip("The fall gravity multiplier.")]
    public float FallMultiplier = 2.5f;

    /// <summary>
    /// Start moving forward (or backward) once the horizontal axis absolute value overcomes this threshold.
    /// </summary>
    [Tooltip("Start moving forward (or backward) once the horizontal axis absolute value overcomes this threshold.")]
    public float AscendingHorizontalMovementThreshold = 0.1f;

    /// <summary>
    /// Stop the character's velocity once the horizontal axis absolute value falls below this threshold.
    /// </summary>
    [Tooltip("Stop the character's velocity once the horizontal axis absolute value falls below this threshold.")]
    public float DescendingHorizontalMovementThreshold = 0.5f;

    /// <summary>
    /// Amount of frames the 'jump' key needs to be held down for to achieve a full hop instead of a short hop.
    /// If the jump key is released before, a short hop will be performed instead.
    /// </summary>
    [Tooltip("Amount of frames the 'jump' key needs to be held down for to achieve a full hop instead of a short hop. If the jump key is released before, a short hop will be performed instead.")]
    public int JumpSquatAnimationFrameCount = 5;

    #endregion // Inspector variables

    #region Instance variables

    /// <summary>
    /// The player's rigid body 2D.
    /// </summary>
    private Rigidbody2D m_Player;

    /// <summary>
    /// The character's sprite renderer.
    /// </summary>
    private SpriteRenderer m_SpriteRenderer;

    /// <summary>
    /// Horizontal axis value at the previous frame. Used to know if the player is speeding up or slowing down.
    /// </summary>
    private float m_PreviousHorizontalValue;

    /// <summary>
    /// Is the player currently grounded ?
    /// </summary>
    private bool m_IsGrounded;

    /// <summary>
    /// Is the player currently in jump squat ?
    /// </summary>
    private bool m_IsInJumpSquat;

    /// <summary>
    /// The amount of frames that went by since the jump squat started.
    /// </summary>
    private int m_FrameCountSinceJumpSquat;

    /// <summary>
    /// The current horizontal axis value.
    /// </summary>
    private float m_HorizontalAxisValue;

    /// <summary>
    /// Is the jump key being held this frame ?
    /// </summary>
    private bool m_IsJumpKeyHeld;

    /// <summary>
    /// Should the player be performing a jump at this frame ?
    /// </summary>
    private bool m_MustPerformJump;

    #endregion // Instance variables

    #region Properties

    /// <summary>
    /// Is the player currently facing right ?
    /// </summary>
    public bool IsFacingRight { get { return !m_SpriteRenderer.flipX; } }

    #endregion // Properties

    #region Unity callbacks

    /// <summary>
    /// Called on start every time the script is activated.
    /// </summary>
    private void Start()
    {
        m_Player = GetComponent<Rigidbody2D>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Called once per frame.
    /// </summary>
    private void Update()
    {
        m_HorizontalAxisValue = InputHelper.GetHorizontalAxis();
        m_IsJumpKeyHeld = InputHelper.GetJumpButtonHeldDown();

        if (m_IsInJumpSquat)
        {
            m_FrameCountSinceJumpSquat++;
            if (m_FrameCountSinceJumpSquat == JumpSquatAnimationFrameCount)
            {
                m_MustPerformJump = true;
                m_IsInJumpSquat = false;
            }
        }

        if (!m_IsInJumpSquat && m_IsGrounded && InputHelper.GetJumpButtonDown())
        {
            m_IsInJumpSquat = true;
            m_FrameCountSinceJumpSquat = 0;
        }
    }

    /// <summary>
    /// Called every physics update. Used to make physics changes.
    /// </summary>
    private void FixedUpdate()
    {
        HandleRun();
        HandleFlip();
        HandleFall();

        if (m_MustPerformJump)
        {
            Jump();
        }
    }

    /// <summary>
    /// Called when this collider enters in collision with another one.
    /// </summary>
    /// <param name="collision">The collision information.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            m_IsGrounded = true;
        }
    }

    /// <summary>
    /// Called when this collider exits a previously made collision.
    /// </summary>
    /// <param name="collision">The collision information.</param>
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
        {
            m_IsGrounded = false;
        }
    }

    #endregion // Unity callbacks

    #region Methods

    /// <summary>
    /// Handle the run mechanic.
    /// </summary>
    private void HandleRun()
    {
        float absValue = Mathf.Abs(m_HorizontalAxisValue);

        // Ascending
        if (absValue >= Mathf.Abs(m_PreviousHorizontalValue))
        {
            if (absValue > AscendingHorizontalMovementThreshold)
            {
                m_Player.velocity = new Vector2(m_HorizontalAxisValue < 0 ? -RunSpeed : RunSpeed, m_Player.velocity.y);
            }
        }
        // Descending
        else
        {
            if (absValue < DescendingHorizontalMovementThreshold)
            {
                m_Player.velocity = new Vector2(0f, m_Player.velocity.y);
            }
        }

        m_PreviousHorizontalValue = m_HorizontalAxisValue;
    }

    /// <summary>
    /// Manage the sprite flipping depending on the current direction.
    /// </summary>
    private void HandleFlip()
    {
        if (IsFacingRight && m_HorizontalAxisValue < -AscendingHorizontalMovementThreshold)
        {
            m_SpriteRenderer.flipX = true;
        }
        else if (!IsFacingRight && m_HorizontalAxisValue > AscendingHorizontalMovementThreshold)
        {
            m_SpriteRenderer.flipX = false;
        }
    }

    /// <summary>
    /// Perform a jump
    /// </summary>
    private void Jump()
    {
        // If the key is still being held down, perform a full hop.
        if (m_IsJumpKeyHeld)
        {
            m_Player.AddForce(new Vector2(0f, FullHopImpulse), ForceMode2D.Impulse);
        }
        // otherwise perform a short hop
        else
        {
            m_Player.AddForce(new Vector2(0f, ShortHopImpulse), ForceMode2D.Impulse);
        }

        // Jump is done.
        m_MustPerformJump = false;
    }

    /// <summary>
    /// Handle the player's fall by applying more gravity to the rigid body.
    /// </summary>
    private void HandleFall()
    {
        // If the player is falling
        if (m_Player.velocity.y < 0)
        {
            m_Player.velocity += Vector2.up * Physics2D.gravity.y * (FallMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    #endregion // Methods

}
