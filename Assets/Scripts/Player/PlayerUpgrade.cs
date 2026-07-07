using UnityEngine;
using RPGDragon.Core;

namespace RPGDragon.Player
{
    public class PlayerUpgrade : MonoBehaviour
    {
        [Header("Levels")]
        [SerializeField] private int weaponLevel = 1;
        [SerializeField] private int armorLevel = 1;
        private const int MAX_LEVEL = 5;

        [Header("Current Shards")]
        [SerializeField] private int weaponShards = 0;
        [SerializeField] private int armorShards = 0;

        [Header("Thresholds (per level)")]
        [SerializeField] private int[] weaponShardNeed = new int[] { 3, 6, 10, 15, 20 };
        [SerializeField] private int[] armorShardNeed = new int[] { 3, 6, 10, 15, 20 };

        [Header("Upgrade Bonuses")]
        [SerializeField] private int attackBonusPerLevel = 3;
        [SerializeField] private int hpBonusPerLevel = 10;

        public int WeaponLevel => weaponLevel;
        public int ArmorLevel => armorLevel;
        public int WeaponShards => weaponShards;
        public int ArmorShards => armorShards;
        public int WeaponShardsForNext => weaponLevel < MAX_LEVEL ? weaponShardNeed[weaponLevel - 1] : 0;
        public int ArmorShardsForNext => armorLevel < MAX_LEVEL ? armorShardNeed[armorLevel - 1] : 0;

        private PlayerStats stats;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        /// <summary>
        /// Add shards of a specific type. Auto-upgrades if threshold met.
        /// </summary>
        public void AddShard(string type, int amount = 1)
        {
            if (type == "Weapon")
            {
                weaponShards += amount;
                TryUpgradeWeapon();
            }
            else if (type == "Armor")
            {
                armorShards += amount;
                TryUpgradeArmor();
            }
        }

        private void TryUpgradeWeapon()
        {
            if (weaponLevel >= MAX_LEVEL) return;
            int needed = weaponShardNeed[weaponLevel - 1];
            if (weaponShards >= needed)
            {
                weaponShards -= needed;
                weaponLevel++;
                stats.SetAttackDamage(stats.AttackDamage + attackBonusPerLevel);
                EventBus.Raise(new PlayerUpgradedEvent { UpgradeType = "Weapon", NewLevel = weaponLevel });
                Debug.Log($"[Upgrade] Vu khi len cap {weaponLevel}! ATK +{attackBonusPerLevel}");
            }
        }

        private void TryUpgradeArmor()
        {
            if (armorLevel >= MAX_LEVEL) return;
            int needed = armorShardNeed[armorLevel - 1];
            if (armorShards >= needed)
            {
                armorShards -= needed;
                armorLevel++;
                stats.IncreaseMaxHP(hpBonusPerLevel);
                EventBus.Raise(new PlayerUpgradedEvent { UpgradeType = "Armor", NewLevel = armorLevel });
                Debug.Log($"[Upgrade] Giap len cap {armorLevel}! HP +{hpBonusPerLevel}");
            }
        }

        /// <summary>
        /// Bootstrap: set shards for testing (or loading save).
        /// </summary>
        public void SetShards(string type, int amount)
        {
            if (type == "Weapon") weaponShards = amount;
            else if (type == "Armor") armorShards = amount;
        }
    }
}
