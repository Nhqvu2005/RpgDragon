using UnityEngine;
using RPGDragon.Player;

namespace RPGDragon.Core
{
    /// <summary>
    /// A physical pickup on the ground. Player walks over it to collect shards.
    /// Works with prefabs placed in the scene. For direct drops from enemies,
    /// see EnemyBase.DropShards().
    /// </summary>
    public class ShardPickup : MonoBehaviour
    {
        [SerializeField] private string shardType = "Weapon"; // "Weapon" or "Armor"
        [SerializeField] private int amount = 1;
        [SerializeField] private float lifetime = 15f;
        [SerializeField] private float magnetRange = 0f; // 0 = no magnet, just touch

        private void Start()
        {
            if (lifetime > 0)
                Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            if (magnetRange > 0f)
            {
                // Optional magnet effect: pull toward player when close
                var player = GameManager.Instance?.Player;
                if (player != null && Vector2.Distance(transform.position, player.transform.position) <= magnetRange)
                {
                    transform.position = Vector2.MoveTowards(
                        transform.position,
                        player.transform.position,
                        3f * Time.deltaTime
                    );
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            var upgrade = other.GetComponent<PlayerUpgrade>();
            if (upgrade != null)
            {
                upgrade.AddShard(shardType, amount);
                Destroy(gameObject);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (magnetRange > 0f)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(transform.position, magnetRange);
            }
        }
    }
}
