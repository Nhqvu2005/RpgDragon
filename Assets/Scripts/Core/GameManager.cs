using UnityEngine;

namespace RPGDragon.Core
{
    /// <summary>
    /// Enum representing the possible high-level states of the game.
    /// </summary>
    public enum GameState
    {
        Playing,
        Paused,
        Dialogue,
        Cutscene,
        GameOver
    }

    /// <summary>
    /// Core singleton manager responsible for game-wide state, player spawning,
    /// and lifecycle management. This object persists across scenes via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private Transform playerSpawnPoint;

        /// <summary>
        /// The current game state. Setting this updates Time.timeScale accordingly.
        /// </summary>
        public GameState CurrentState { get; private set; } = GameState.Playing;

        /// <summary>
        /// Reference to the spawned player GameObject, if any.
        /// </summary>
        public GameObject Player { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Player == null && playerSpawnPoint != null)
            {
                SpawnPlayer(playerSpawnPoint.position);
            }
        }

        /// <summary>
        /// Transitions the game to a new state. Pausing freezes Time.timeScale to 0;
        /// all other states use normal time scale.
        /// </summary>
        /// <param name="newState">The target game state.</param>
        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        }

        /// <summary>
        /// Instantiates the player prefab at the given world position and stores
        /// the reference. The player object persists across scenes.
        /// </summary>
        /// <param name="position">World position to spawn the player.</param>
        public void SpawnPlayer(Vector3 position)
        {
            if (playerPrefab != null)
            {
                Player = Instantiate(playerPrefab, position, Quaternion.identity);
                DontDestroyOnLoad(Player);
            }
        }
    }
}
