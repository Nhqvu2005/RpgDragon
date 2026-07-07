using UnityEngine;
using RPGDragon.Core;

namespace RPGDragon.Player
{
    public class PlayerStats : MonoBehaviour
    {
        [Header("Base Stats")]
        [SerializeField] private int maxHP = 100;
        [SerializeField] private int currentHP;
        [SerializeField] private int attackDamage = 10;
        [SerializeField] private float moveSpeed = 5f;

        private PlayerController playerController;

        // --- Properties ---

        public int MaxHP => maxHP;
        public int CurrentHP => currentHP;
        public int AttackDamage => attackDamage;
        public float MoveSpeed => moveSpeed;

        public bool IsDead => currentHP <= 0;

        public float HPPercentage => (float)currentHP / maxHP;

        // --- Unity Lifecycle ---

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            currentHP = maxHP;
        }

        private void Start()
        {
            // Notify UI or other systems of initial HP
            EventBus.Raise<PlayerDamagedEvent>(new PlayerDamagedEvent { Damage = 0, CurrentHP = currentHP });
        }

        // --- Public API ---

        /// <summary>
        /// Apply damage to the player. Clamps HP and raises appropriate events.
        /// </summary>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="knockbackDirection">Direction the player should be knocked back.</param>
        public void TakeDamage(int damage, Vector2 knockbackDirection)
        {
            if (IsDead)
                return;

            damage = Mathf.Max(0, damage);
            currentHP = Mathf.Clamp(currentHP - damage, 0, maxHP);

            // Raise the damage event for UI, audio, etc.
            EventBus.Raise<PlayerDamagedEvent>(new PlayerDamagedEvent { Damage = damage, CurrentHP = currentHP });

            // Notify the controller for state/animation
            if (playerController != null)
                playerController.OnDamaged(knockbackDirection);

            // Check for death
            if (currentHP <= 0)
            {
                EventBus.Raise<PlayerDiedEvent>(new PlayerDiedEvent());
                playerController?.SetState(PlayerState.Dead);
            }
        }

        /// <summary>
        /// Convenience overload: TakeDamage with zero knockback.
        /// </summary>
        public void TakeDamage(int damage)
        {
            TakeDamage(damage, Vector2.zero);
        }

        /// <summary>
        /// Heal the player by the given amount. Cannot exceed maxHP.
        /// </summary>
        /// <param name="amount">Amount of HP to restore.</param>
        public void Heal(int amount)
        {
            if (IsDead)
                return;

            amount = Mathf.Max(0, amount);
            int previousHP = currentHP;
            currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);

            int actualHeal = currentHP - previousHP;
            if (actualHeal > 0)
            {
                EventBus.Raise<PlayerHealedEvent>(new PlayerHealedEvent { Amount = actualHeal, CurrentHP = currentHP });
            }
        }

        /// <summary>
        /// Modify the player's attack damage (e.g., from equipment or buffs).
        /// </summary>
        public void SetAttackDamage(int newDamage)
        {
            attackDamage = Mathf.Max(0, newDamage);
        }

        /// <summary>
        /// Modify the player's move speed (e.g., from buffs or debuffs).
        /// </summary>
        public void SetMoveSpeed(float newSpeed)
        {
            moveSpeed = Mathf.Max(0f, newSpeed);
        }

        /// <summary>
        /// Increase max HP (from armor upgrades etc.). Also heals by the same amount.
        /// </summary>
        public void IncreaseMaxHP(int amount)
        {
            if (amount <= 0) return;
            maxHP += amount;
            currentHP += amount;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }

        /// <summary>
        /// Fully restore HP to max.
        /// </summary>
        public void RestoreFullHP()
        {
            int missing = maxHP - currentHP;
            if (missing > 0)
                Heal(missing);
        }

        // --- Editor ---

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (maxHP < 1)
                maxHP = 1;

            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        }
    }
}
