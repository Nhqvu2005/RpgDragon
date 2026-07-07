using System.Collections;
using UnityEngine;
using RPGDragon.Core;

namespace RPGDragon.Map
{
    public class VictoryController : MonoBehaviour
    {
        [Header("Victory Settings")]
        [SerializeField] private string hubSceneName = "Map1_Forest";
        [SerializeField] private string hubEntranceId = "Victory";
        [SerializeField] private float victoryDelay = 3f;
        [SerializeField] private GameObject victoryTextPrefab; // optional

        private void OnEnable()
        {
            EventBus.Register<BossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDisable()
        {
            EventBus.Unregister<BossDefeatedEvent>(OnBossDefeated);
        }

        private void OnBossDefeated(BossDefeatedEvent evt)
        {
            StartCoroutine(VictoryRoutine());
        }

        private IEnumerator VictoryRoutine()
        {
            Debug.Log("[Victory] Boss defeated! Preparing victory sequence...");
            yield return new WaitForSeconds(victoryDelay);

            // Mark game as won
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Cutscene);
            }

            // Raise game won event for UI
            EventBus.Raise(new GameWonEvent());

            // Load hub scene (return to village)
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(hubSceneName, hubEntranceId);
            }

            Debug.Log("[Victory] Returned to village as hero!");
        }
    }
}
