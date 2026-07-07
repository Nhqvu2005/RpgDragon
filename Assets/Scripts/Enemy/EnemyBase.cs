using System.Collections;
using RPGDragon.Core;
using RPGDragon.Player;
using UnityEngine;

namespace RPGDragon.Enemy
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead
    }

    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] protected string enemyType = "Enemy";
        [SerializeField] protected int maxHP = 10;
        [SerializeField] protected int currentHP;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected int attackDamage = 1;
        [SerializeField] protected float detectionRange = 5f;
        [SerializeField] protected float attackRange = 1.5f;

        [Header("Components")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Collider2D enemyCollider;
        [SerializeField] protected Rigidbody2D rb;

        [Header("Knockback")]
        [SerializeField] protected float knockbackResistance = 1f;

        [Header("Shard Drops")]
        [SerializeField] private string shardDropType = "Weapon"; // "Weapon", "Armor", or "" for none
        [SerializeField] private int shardDropCount = 1;
        [SerializeField] private float shardDropChance = 0.5f; // 50%
        [SerializeField] private GameObject shardPickupPrefab = null; // optional physical pickup prefab

        protected EnemyState currentState;
        protected Transform player;
        protected bool isDead = false;
        protected bool isHurt = false;

        protected virtual void Awake()
        {
            currentHP = maxHP;

            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (enemyCollider == null)
                enemyCollider = GetComponent<Collider2D>();
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void Start()
        {
            if (GameManager.Instance != null)
                player = GameManager.Instance.Player;

            EnterState(EnemyState.Idle);
        }

        protected virtual void Update()
        {
            if (isDead) return;

            switch (currentState)
            {
                case EnemyState.Idle:
                    break;

                case EnemyState.Patrol:
                    PatrolBehavior();
                    break;

                case EnemyState.Chase:
                    ChaseBehavior();
                    break;

                case EnemyState.Attack:
                    AttackBehavior();
                    break;

                case EnemyState.Hurt:
                    break;

                case EnemyState.Dead:
                    break;
            }
        }

        #region State Machine

        protected virtual void EnterState(EnemyState newState)
        {
            if (currentState == newState) return;
            ExitState(currentState);
            currentState = newState;
        }

        protected virtual void ExitState(EnemyState state)
        {
            switch (state)
            {
                case EnemyState.Idle:
                    break;
                case EnemyState.Patrol:
                    break;
                case EnemyState.Chase:
                    break;
                case EnemyState.Attack:
                    break;
                case EnemyState.Hurt:
                    isHurt = false;
                    break;
                case EnemyState.Dead:
                    break;
            }
        }

        #endregion

        #region Abstract Behaviors

        protected abstract void PatrolBehavior();
        protected abstract void ChaseBehavior();
        protected abstract void AttackBehavior();

        #endregion

        #region Combat

        public virtual void TakeDamage(int damage)
        {
            if (isDead) return;

            currentHP -= damage;

            StopCoroutine(nameof(FlashRed));
            StartCoroutine(nameof(FlashRed));

            if (currentHP <= 0)
            {
                Die();
            }
            else
            {
                isHurt = true;
                EnterState(EnemyState.Hurt);
            }
        }

        protected virtual void Die()
        {
            if (isDead) return;
            isDead = true;

            EnterState(EnemyState.Dead);

            if (enemyCollider != null)
                enemyCollider.enabled = false;

            if (rb != null)
                rb.velocity = Vector2.zero;

            StartCoroutine(PlayDeathAndDestroy());
        }

        private IEnumerator PlayDeathAndDestroy()
        {
            EventBus.Raise<RPGDragon.Core.EnemyDefeatedEvent>(new RPGDragon.Core.EnemyDefeatedEvent
            {
                EnemyType = enemyType,
                Position = transform.position
            });

            // Drop shards
            DropShards();

            yield return new WaitForSeconds(1f);

            Destroy(gameObject);
        }

        /// <summary>
        /// Drop shards on death. Spawns physical prefab if assigned, otherwise
        /// adds directly to the player's inventory.
        /// </summary>
        protected virtual void DropShards()
        {
            if (string.IsNullOrEmpty(shardDropType) || shardDropCount <= 0) return;
            if (Random.value > shardDropChance) return;

            if (shardPickupPrefab != null)
            {
                // Spawn physical pickup prefab
                for (int i = 0; i < shardDropCount; i++)
                {
                    Vector2 offset = Random.insideUnitCircle * 0.5f;
                    Instantiate(shardPickupPrefab, (Vector2)transform.position + offset, Quaternion.identity);
                }
            }
            else
            {
                // Direct add to player (works without prefab)
                if (GameManager.Instance?.Player != null)
                {
                    var upgrade = GameManager.Instance.Player.GetComponent<PlayerUpgrade>();
                    if (upgrade != null)
                        upgrade.AddShard(shardDropType, shardDropCount);
                }
            }
        }

        public virtual void ApplyKnockback(Vector2 direction, float force)
        {
            if (isDead || rb == null) return;

            float effectiveForce = force / knockbackResistance;
            rb.AddForce(direction.normalized * effectiveForce, ForceMode2D.Impulse);
        }

        #endregion

        #region Visual Feedback

        protected IEnumerator FlashRed()
        {
            if (spriteRenderer == null) yield break;

            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }

        #endregion

        #region Helpers

        protected float DistanceToPlayer()
        {
            if (player == null) return Mathf.Infinity;
            return Vector2.Distance(transform.position, player.position);
        }

        protected Vector2 DirectionToPlayer()
        {
            if (player == null) return Vector2.zero;
            return ((Vector2)player.position - (Vector2)transform.position).normalized;
        }

        protected virtual void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }

        #endregion
    }
}
