using System.Collections;
using RPGDragon.Core;
using RPGDragon.Player;
using UnityEngine;

namespace RPGDragon.Enemy
{
    public class EnemyDragon : EnemyBase
    {
        [System.Serializable]
        public struct AttackPattern
        {
            public string name;
            public float clawDamageMultiplier;
            public float breathDamageMultiplier;
            public int breathBulletCount;
            public float breathSpreadAngle;
            public float breathBulletSpeed;
        }

        [Header("Phase Settings")]
        [SerializeField] private float phaseTransitionThreshold = 0.5f; // 50% HP
        [SerializeField] private AttackPattern phase1Pattern;
        [SerializeField] private AttackPattern phase2Pattern;

        [Header("Phase 1")]
        [SerializeField] private float phase1MoveSpeed = 2f;
        [SerializeField] private float phase1AttackCooldown = 2.5f;
        [SerializeField] private int attacksBeforeBreath = 3;

        [Header("Phase 2")]
        [SerializeField] private float phase2MoveSpeed = 4f;
        [SerializeField] private float phase2AttackCooldown = 1.5f;
        [SerializeField] private int phase2AttacksBeforeBreath = 2;

        [Header("Fire Breath")]
        [SerializeField] private GameObject fireBreathPrefab;
        [SerializeField] private Transform breathSpawnPoint;

        [Header("Claw Attack")]
        [SerializeField] private float clawAttackRange = 2.5f;
        [SerializeField] private float clawHitboxRadius = 1.8f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Summon")]
        [SerializeField] private GameObject[] minionPrefabs;
        [SerializeField] private int minionCount = 2;
        [SerializeField] private float minionSpawnRadius = 3f;

        [Header("Phase Transition")]
        [SerializeField] private float roarDuration = 2f;

        [Header("Reward")]
        [SerializeField] private GameObject rewardDropPrefab;

        private bool hasPhaseTransitioned = false;
        private bool isPhase2 = false;
        private int attackCount = 0;
        private float currentAttackCooldown;
        private float currentMoveSpeed;
        private int attacksBeforeBreathThreshold;

        protected override void Awake()
        {
            base.Awake();

            if (playerLayer == 0)
                playerLayer = LayerMask.GetMask("Player");

            InitializePhase1();
        }

        protected override void Start()
        {
            base.Start();
            StartCoroutine(AttackPatternRoutine());
        }

        protected override void Update()
        {
            if (isDead) return;

            // Only movement handled in Update; attacks from coroutine
            switch (currentState)
            {
                case EnemyState.Chase:
                    ChaseBehavior();
                    break;
                case EnemyState.Idle:
                case EnemyState.Patrol:
                    // Dragon does not patrol; idle means waiting for attack opportunity
                    break;
            }

            // Phase check is done in TakeDamage override, not here
        }

        private void InitializePhase1()
        {
            isPhase2 = false;
            currentMoveSpeed = phase1MoveSpeed;
            currentAttackCooldown = phase1AttackCooldown;
            attacksBeforeBreathThreshold = attacksBeforeBreath;
            moveSpeed = phase1MoveSpeed;
        }

        private void InitializePhase2()
        {
            isPhase2 = true;
            currentMoveSpeed = phase2MoveSpeed;
            currentAttackCooldown = phase2AttackCooldown;
            attacksBeforeBreathThreshold = phase2AttacksBeforeBreath;
            moveSpeed = phase2MoveSpeed;
        }

        #region Attack Pattern Coroutine

        private IEnumerator AttackPatternRoutine()
        {
            while (!isDead)
            {
                yield return new WaitForSeconds(0.5f); // initial delay

                while (DistanceToPlayer() > detectionRange && !isDead)
                {
                    EnterState(EnemyState.Idle);
                    yield return new WaitForSeconds(0.5f);
                }

                // Move into range
                while (DistanceToPlayer() > (isPhase2 ? clawAttackRange * 0.8f : clawAttackRange) && !isDead)
                {
                    EnterState(EnemyState.Chase);
                    yield return null;
                }

                // Execute attack pattern
                attackCount++;

                AttackPattern currentPattern = isPhase2 ? phase2Pattern : phase1Pattern;

                if (attackCount >= attacksBeforeBreathThreshold)
                {
                    // Fire breath attack
                    attackCount = 0;
                    EnterState(EnemyState.Attack);
                    yield return StartCoroutine(FireBreathAttack(currentPattern));
                }
                else
                {
                    // Claw attack
                    EnterState(EnemyState.Attack);
                    yield return StartCoroutine(ClawAttack());
                }

                // Cooldown between attacks
                yield return new WaitForSeconds(currentAttackCooldown);
            }
        }

        private IEnumerator ClawAttack()
        {
            // Short wind-up
            yield return new WaitForSeconds(0.3f);

            Vector2 attackPosition = (Vector2)transform.position + DirectionToPlayer() * 1f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackPosition,
                clawHitboxRadius,
                playerLayer
            );

            float clawDamage = attackDamage * (isPhase2 ? phase2Pattern.clawDamageMultiplier : phase1Pattern.clawDamageMultiplier);

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
                {
                    playerStats.TakeDamage(Mathf.RoundToInt(clawDamage));
                }
            }

            // Claw animation visual feedback via flash
            StopCoroutine(nameof(FlashRed));
            StartCoroutine(nameof(FlashRed));
        }

        private IEnumerator FireBreathAttack(AttackPattern pattern)
        {
            // Wind-up
            yield return new WaitForSeconds(0.5f);

            if (fireBreathPrefab == null || breathSpawnPoint == null)
                yield break;

            Vector2 baseDirection = DirectionToPlayer();
            if (baseDirection == Vector2.zero)
                baseDirection = Vector2.right;

            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
            float halfSpread = pattern.breathSpreadAngle * 0.5f;

            int bulletCount = pattern.breathBulletCount;
            float angleStep = bulletCount > 1 ? pattern.breathSpreadAngle / (bulletCount - 1) : 0f;
            float startAngle = baseAngle - halfSpread;

            for (int i = 0; i < bulletCount; i++)
            {
                float currentAngle = startAngle + (angleStep * i);
                Vector2 bulletDirection = new Vector2(
                    Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                    Mathf.Sin(currentAngle * Mathf.Deg2Rad)
                );

                GameObject bullet = Instantiate(
                    fireBreathPrefab,
                    breathSpawnPoint.position,
                    Quaternion.identity
                );

                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.velocity = bulletDirection * pattern.breathBulletSpeed;
                }

                // Auto-destroy bullets after a few seconds
                Destroy(bullet, 5f);
            }
        }

        #endregion

        #region Overrides

        public override void TakeDamage(int damage)
        {
            if (isDead || isHurt) return;

            base.TakeDamage(damage);

            // Check phase transition
            if (!hasPhaseTransitioned && !isDead)
            {
                float hpPercent = (float)currentHP / maxHP;
                if (hpPercent <= phaseTransitionThreshold)
                {
                    StartCoroutine(PhaseTransitionRoutine());
                }
            }
        }

        private IEnumerator PhaseTransitionRoutine()
        {
            hasPhaseTransitioned = true;
            isHurt = true;

            // Stop attacking
            StopAllCoroutines();

            // Roar animation: invulnerable, flash repeatedly
            float roarElapsed = 0f;
            float roarFlashInterval = 0.2f;

            while (roarElapsed < roarDuration)
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = spriteRenderer.color == Color.red ? Color.white : Color.red;

                yield return new WaitForSeconds(roarFlashInterval);
                roarElapsed += roarFlashInterval;
            }

            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;

            // Transition to phase 2
            InitializePhase2();

            // Summon minions
            StartCoroutine(SummonMinions());

            // Resume attack pattern
            isHurt = false;
            StartCoroutine(AttackPatternRoutine());
        }

        private IEnumerator SummonMinions()
        {
            if (minionPrefabs == null || minionPrefabs.Length == 0) yield break;

            for (int i = 0; i < minionCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * minionSpawnRadius;
                Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

                GameObject minionPrefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
                Instantiate(minionPrefab, spawnPos, Quaternion.identity);

                yield return new WaitForSeconds(0.3f);
            }
        }

        protected override void Die()
        {
            if (isDead) return;
            isDead = true;
            isHurt = true;

            StopAllCoroutines();
            EnterState(EnemyState.Dead);

            if (enemyCollider != null)
                enemyCollider.enabled = false;

            if (rb != null)
                rb.velocity = Vector2.zero;

            StartCoroutine(BossDeathSequence());
        }

        private IEnumerator BossDeathSequence()
        {
            // Longer death animation
            float deathDuration = 2f;
            float elapsed = 0f;
            while (elapsed < deathDuration)
            {
                if (spriteRenderer != null)
                {
                    float alpha = Mathf.Lerp(1f, 0f, elapsed / deathDuration);
                    Color c = spriteRenderer.color;
                    c.a = alpha;
                    spriteRenderer.color = c;
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Raise boss defeated event
            EventBus.Raise(new BossDefeatedEvent
            {
                Position = transform.position,
                BossObject = gameObject
            });

            // Drop reward
            if (rewardDropPrefab != null)
            {
                Instantiate(rewardDropPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        #endregion

        #region Behavior Overrides

        protected override void PatrolBehavior()
        {
            // Dragon does not patrol
        }

        protected override void ChaseBehavior()
        {
            if (player == null || isDead) return;

            Vector2 direction = DirectionToPlayer();
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                currentMoveSpeed * Time.deltaTime
            );

            // Flip sprite
            if (direction != Vector2.zero)
            {
                Vector3 localScale = transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * (direction.x >= 0 ? 1f : -1f);
                transform.localScale = localScale;
            }
        }

        protected override void AttackBehavior()
        {
            // Handled by coroutine
        }

        #endregion

        #region Gizmos

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, clawAttackRange);

            if (breathSpawnPoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(breathSpawnPoint.position, 0.15f);
            }
        }

        #endregion
    }
}
