using UnityEngine;
using RPGDragon.Core;

namespace RPGDragon.Player
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Attack,
        Hurt,
        Dead
    }

    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;

        [Header("State")]
        [SerializeField] private PlayerState currentState = PlayerState.Idle;

        private Vector2 inputDirection;
        private Vector2 lastFacingDirection = Vector2.down; // Default facing down in top-down RPG

        private bool inputDisabled = false;

        // --- Properties ---

        public PlayerState CurrentState => currentState;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();

            if (animator == null)
                animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (currentState == PlayerState.Dead || inputDisabled)
                return;

            HandleInput();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            if (currentState == PlayerState.Dead || inputDisabled)
            {
                rb.velocity = Vector2.zero;
                return;
            }

            if (currentState == PlayerState.Idle || currentState == PlayerState.Walk)
            {
                Move();
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }

        // --- Input ---

        private void HandleInput()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            inputDirection = new Vector2(moveX, moveY).normalized;

            if (inputDirection.magnitude > 0.01f)
            {
                lastFacingDirection = inputDirection;

                if (currentState != PlayerState.Attack)
                    SetState(PlayerState.Walk);
            }
            else
            {
                if (currentState == PlayerState.Walk)
                    SetState(PlayerState.Idle);
            }
        }

        // --- Movement ---

        private void Move()
        {
            if (inputDisabled)
                return;

            rb.velocity = inputDirection * moveSpeed;
        }

        // --- State Machine ---

        public void SetState(PlayerState newState)
        {
            if (currentState == newState)
                return;

            if (currentState == PlayerState.Dead)
                return; // Dead is terminal

            PlayerState previousState = currentState;
            currentState = newState;

            OnStateEnter(previousState);
        }

        private void OnStateEnter(PlayerState previousState)
        {
            switch (currentState)
            {
                case PlayerState.Hurt:
                    StartCoroutine(HurtRoutine());
                    break;

                case PlayerState.Dead:
                    HandleDeath();
                    break;

                case PlayerState.Attack:
                    // Attack state duration is managed by PlayerCombat
                    break;
            }
        }

        private System.Collections.IEnumerator HurtRoutine()
        {
            rb.velocity = Vector2.zero;
            inputDisabled = true;

            // Allow short hurt animation to play
            yield return new WaitForSeconds(0.3f);

            inputDisabled = false;

            // Only return to idle if still in Hurt state (not overridden by Death etc.)
            if (currentState == PlayerState.Hurt)
            {
                SetState(PlayerState.Idle);
            }
        }

        private void HandleDeath()
        {
            inputDisabled = true;
            rb.velocity = Vector2.zero;
            rb.simulated = false;

            if (animator != null)
                animator.SetTrigger("isDead");

            // Delay then raise GameOverEvent
            StartCoroutine(GameOverRoutine());
        }

        private System.Collections.IEnumerator GameOverRoutine()
        {
            yield return new WaitForSeconds(2f);
            EventBus.Raise<GameOverEvent>(new GameOverEvent());
        }

        // --- Public API ---

        public Vector2 GetFacingDirection()
        {
            return lastFacingDirection;
        }

        // --- Animator ---

        private void UpdateAnimator()
        {
            if (animator == null)
                return;

            animator.SetFloat("moveX", lastFacingDirection.x);
            animator.SetFloat("moveY", lastFacingDirection.y);
            animator.SetBool("isWalking", currentState == PlayerState.Walk);
            animator.SetBool("isDead", currentState == PlayerState.Dead);
        }

        // --- External Event Triggers ---

        /// <summary>
        /// Called by PlayerStats or external damage sources to put the player in Hurt state.
        /// </summary>
        public void OnDamaged(Vector2 knockbackDirection)
        {
            if (currentState == PlayerState.Dead)
                return;

            SetState(PlayerState.Hurt);

            // Apply knockback
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.AddForce(knockbackDirection * 8f, ForceMode2D.Impulse);
            }

            if (animator != null)
                animator.SetTrigger("isHurt");
        }

        /// <summary>
        /// Called externally (e.g., by PlayerCombat) when attack animation completes.
        /// </summary>
        public void OnAttackEnd()
        {
            if (currentState == PlayerState.Attack)
            {
                if (inputDirection.magnitude > 0.01f)
                    SetState(PlayerState.Walk);
                else
                    SetState(PlayerState.Idle);
            }
        }

        private void OnValidate()
        {
            // Ensure references are assigned in the editor
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
            if (animator == null)
                animator = GetComponent<Animator>();
        }
    }
}
