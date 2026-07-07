using System.Collections;
using RPGDragon.Player;
using UnityEngine;

namespace RPGDragon.Enemy
{
    public class EnemyMelee : EnemyBase
    {
        [Header("Patrol Settings")]
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private float waypointReachedDistance = 0.2f;
        [SerializeField] private float waitTimeAtWaypoint = 1f;

        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 1f;
        [SerializeField] private float attackHitboxRadius = 1.5f;
        [SerializeField] private LayerMask playerLayer;

        private int currentWaypointIndex = 0;
        private float lastAttackTime;
        private bool isWaitingAtWaypoint = false;

        protected override void Start()
        {
            base.Start();

            if (playerLayer == 0)
                playerLayer = LayerMask.GetMask("Player");

            EnterState(EnemyState.Patrol);
        }

        protected override void Update()
        {
            if (isDead) return;
            if (currentState == EnemyState.Hurt) return;

            float distToPlayer = DistanceToPlayer();

            if (distToPlayer <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    EnterState(EnemyState.Attack);
                }
                else
                {
                    EnterState(EnemyState.Idle);
                }
            }
            else if (distToPlayer <= detectionRange)
            {
                EnterState(EnemyState.Chase);
            }
            else if (currentState == EnemyState.Idle || currentState == EnemyState.Chase)
            {
                if (waypoints != null && waypoints.Length > 0)
                    EnterState(EnemyState.Patrol);
                else
                    EnterState(EnemyState.Idle);
            }

            base.Update();
        }

        protected override void PatrolBehavior()
        {
            if (waypoints == null || waypoints.Length == 0) return;

            if (isWaitingAtWaypoint) return;

            Transform targetWaypoint = waypoints[currentWaypointIndex];
            if (targetWaypoint == null) return;

            Vector2 targetPos = targetWaypoint.position;
            Vector2 currentPos = transform.position;

            float distance = Vector2.Distance(currentPos, targetPos);

            if (distance <= waypointReachedDistance)
            {
                StartCoroutine(WaitAtWaypoint());
                return;
            }

            Vector2 direction = (targetPos - currentPos).normalized;
            transform.position = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

            HandleFacingDirection(direction);
        }

        private IEnumerator WaitAtWaypoint()
        {
            isWaitingAtWaypoint = true;
            yield return new WaitForSeconds(waitTimeAtWaypoint);

            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            isWaitingAtWaypoint = false;
        }

        protected override void ChaseBehavior()
        {
            if (player == null) return;

            Vector2 direction = DirectionToPlayer();
            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );

            HandleFacingDirection(direction);
        }

        protected override void AttackBehavior()
        {
            if (player == null) return;

            lastAttackTime = Time.time;

            Vector2 attackPosition = (Vector2)transform.position + DirectionToPlayer() * 0.5f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackPosition,
                attackHitboxRadius,
                playerLayer
            );

            foreach (Collider2D hit in hits)
            {
                if (hit.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
                {
                    playerStats.TakeDamage(attackDamage);
                }
            }

            // Return to chase after attack
            EnterState(EnemyState.Chase);
        }

        private void HandleFacingDirection(Vector2 direction)
        {
            if (direction != Vector2.zero)
            {
                float scaleX = direction.x >= 0 ? 1f : -1f;
                Vector3 localScale = transform.localScale;
                localScale.x = Mathf.Abs(localScale.x) * scaleX;
                transform.localScale = localScale;
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Draw waypoint connections
            if (waypoints != null && waypoints.Length > 0)
            {
                Gizmos.color = Color.cyan;
                for (int i = 0; i < waypoints.Length; i++)
                {
                    if (waypoints[i] == null) continue;

                    Gizmos.DrawSphere(waypoints[i].position, 0.15f);

                    int nextIndex = (i + 1) % waypoints.Length;
                    if (waypoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                    }
                }
            }
        }
    }
}
