using UnityEngine;
using RPGDragon.Core;

namespace RPGDragon.Player
{
    [RequireComponent(typeof(PlayerController), typeof(PlayerStats))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Combat Settings")]
        [SerializeField] private float attackCooldown = 0.3f;
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackRadius = 0.5f;
        [SerializeField] private float knockbackForce = 10f;
        [SerializeField] private LayerMask enemyLayer;

        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private PlayerStats playerStats;

        private float lastAttackTime = -Mathf.Infinity;
        private bool isAttacking = false;
        private Animator cachedAnimator;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            playerStats = GetComponent<PlayerStats>();
            cachedAnimator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (playerController.CurrentState == PlayerState.Dead)
                return;

            // Attack input: Space or Z key
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Z))
            {
                TryAttack();
            }
        }

        // --- Combat Logic ---

        /// <summary>
        /// Attempt to perform an attack if cooldown has elapsed and player is in a valid state.
        /// </summary>
        public void TryAttack()
        {
            if (isAttacking)
                return;

            if (Time.time < lastAttackTime + attackCooldown)
                return;

            if (playerStats.IsDead)
                return;

            // Cannot attack while already in a non-interruptible state
            PlayerState state = playerController.CurrentState;
            if (state == PlayerState.Hurt || state == PlayerState.Dead)
                return;

            PerformAttack();
        }

        private void PerformAttack()
        {
            isAttacking = true;
            lastAttackTime = Time.time;

            // Set player state
            playerController.SetState(PlayerState.Attack);

            // Trigger attack animation
            if (cachedAnimator != null)
            {
                Vector2 facing = playerController.GetFacingDirection();
                cachedAnimator.SetFloat("moveX", facing.x);
                cachedAnimator.SetFloat("moveY", facing.y);
                cachedAnimator.SetTrigger("attack");
            }

            // Calculate hitbox position in facing direction
            Vector2 facingDirection = playerController.GetFacingDirection();
            Vector2 attackPosition = (Vector2)transform.position + facingDirection * attackRange;

            // Detect enemies
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPosition, attackRadius, enemyLayer);

            foreach (Collider2D hit in hits)
            {
                // Ignore self
                if (hit.gameObject == gameObject)
                    continue;

                // Try to get EnemyBase component (character or base class)
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    // Calculate knockback direction away from player
                    Vector2 knockbackDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;

                    // Apply damage
                    enemy.TakeDamage(playerStats.AttackDamage);
                    enemy.ApplyKnockback(knockbackDir, knockbackForce);

                    // Apply knockback force to enemy's Rigidbody2D
                    Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
                    if (enemyRb != null)
                    {
                        enemyRb.velocity = Vector2.zero;
                        enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
                    }
                }
            }

            // End the attack after a short delay to allow animation to play
            StartCoroutine(AttackEndRoutine());
        }

        private System.Collections.IEnumerator AttackEndRoutine()
        {
            // Wait for attack animation to finish (attackCooldown controls the minimal duration)
            yield return new WaitForSeconds(attackCooldown * 0.5f);

            isAttacking = false;
            playerController.OnAttackEnd();
        }

        // --- Public Helpers ---

        /// <summary>
        /// Returns whether the player can currently attack (cooldown + state check).
        /// </summary>
        public bool CanAttack()
        {
            if (isAttacking)
                return false;

            if (Time.time < lastAttackTime + attackCooldown)
                return false;

            if (playerStats.IsDead)
                return false;

            PlayerState state = playerController.CurrentState;
            if (state == PlayerState.Hurt || state == PlayerState.Dead)
                return false;

            return true;
        }

        // --- Editor Visualization ---

        private void OnDrawGizmosSelected()
        {
            if (playerController == null)
                return;

            Vector2 facingDirection = playerController.GetFacingDirection();
            Vector2 attackPosition = (Vector2)transform.position + facingDirection * attackRange;

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
            Gizmos.DrawWireSphere(attackPosition, attackRadius);

            // Draw a line from player to attack center for clarity
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, attackPosition);
        }

        // --- Editor Validation ---

        private void OnValidate()
        {
            if (playerController == null)
                playerController = GetComponent<PlayerController>();
            if (playerStats == null)
                playerStats = GetComponent<PlayerStats>();

            attackRange = Mathf.Max(0.1f, attackRange);
            attackRadius = Mathf.Max(0.1f, attackRadius);
            attackCooldown = Mathf.Max(0.05f, attackCooldown);
        }
    }
}
