using UnityEngine;
using RPGDragon.Core;
using RPGDragon.Quest;

namespace RPGDragon.Map
{
    /// <summary>
    /// Zone trigger for map transitions. Place at map exits.
    /// Optionally gated by quest completion.
    /// </summary>
    public class ExitZone : MonoBehaviour
    {
        [Header("Target Scene")]
        [SerializeField] private string targetScene;
        [SerializeField] private string entranceId;

        [Header("Quest Gate (optional)")]
        [SerializeField] private string requiredQuestId;
        [SerializeField] private string lockedMessage = "Cần hoàn thành nhiệm vụ trước!";

        [Header("Auto-Transition")]
        [SerializeField] private bool autoTransition = true;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (!autoTransition) return;

            TryTransition();
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (!autoTransition && Input.GetKeyDown(KeyCode.E))
            {
                TryTransition();
            }
        }

        public void TryTransition()
        {
            // Check quest requirement
            if (!string.IsNullOrEmpty(requiredQuestId))
            {
                bool completed = QuestManager.Instance != null &&
                    QuestManager.Instance.IsQuestCompleted(requiredQuestId);
                if (!completed)
                {
                    Debug.Log($"[ExitZone] Locked: {lockedMessage}");
                    // TODO: Show message on UI
                    return;
                }
            }

            if (!string.IsNullOrEmpty(targetScene) && SceneLoader.Instance != null)
            {
                Debug.Log($"[ExitZone] Transition to {targetScene} (entrance: {entranceId})");
                SceneLoader.Instance.LoadScene(targetScene, entranceId);
            }
        }

        private void OnDrawGizmos()
        {
            // Visualize exit zone
            BoxCollider2D box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                Gizmos.DrawCube(transform.position, box.size);
            }
        }
    }
}
