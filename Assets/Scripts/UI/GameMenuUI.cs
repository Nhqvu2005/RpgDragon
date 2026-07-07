using UnityEngine;
using UnityEngine.SceneManagement;
using RPGDragon.Core;

namespace RPGDragon.UI
{
    /// <summary>
    /// Pause / main menu overlay toggled with ESC. Manages time scale and
    /// game state transitions.
    ///
    /// <para>
    /// Depends on <c>RPGDragon.Core.GameManager</c> and expects a
    /// <c>RPGDragon.Core.SaveManager</c> with static <c>Save()</c> and <c>Load()</c> methods.
    /// </para>
    /// </summary>
    public class GameMenuUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject menuPanel;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

        private bool _isOpen;

        // ── Unity Lifecycle ─────────────────────────────────────────────────────

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleMenu();
            }
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Open or close the pause menu, toggling time scale and game state.
        /// </summary>
        public void ToggleMenu()
        {
            if (_isOpen)
                CloseMenu();
            else
                OpenMenu();
        }

        /// <summary>
        /// Open the pause menu (freeze time, set paused state).
        /// </summary>
        public void OpenMenu()
        {
            _isOpen = true;

            if (menuPanel != null)
                menuPanel.SetActive(true);

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Paused);
            else
                Time.timeScale = 0f;
        }

        /// <summary>
        /// Close the pause menu (resume time, set playing state).
        /// </summary>
        public void CloseMenu()
        {
            _isOpen = false;

            if (menuPanel != null)
                menuPanel.SetActive(false);

            if (GameManager.Instance != null)
                GameManager.Instance.SetState(GameState.Playing);
            else
                Time.timeScale = 1f;
        }

        // ── Button Handlers ─────────────────────────────────────────────────────

        /// <summary>
        /// Resume gameplay — same as pressing ESC again.
        /// </summary>
        public void OnContinuePressed()
        {
            CloseMenu();
        }

        /// <summary>
        /// Save the game via SaveManager.
        /// </summary>
        public void OnSavePressed()
        {
            SaveManager.Save();
        }

        /// <summary>
        /// Load a saved game via SaveManager.
        /// </summary>
        public void OnLoadPressed()
        {
            SaveManager.Load();
        }

        /// <summary>
        /// Quit to the main menu scene. Destroys all DontDestroyOnLoad game managers
        /// to ensure a clean state on next play.
        /// </summary>
        public void OnQuitPressed()
        {
            Time.timeScale = 1f;

            // Destroy all persistent game managers so they re-initialise fresh
            // when the main menu starts a new game.
            CleanUpDontDestroyOnLoad();

            SceneManager.LoadScene("MainMenu");
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static void CleanUpDontDestroyOnLoad()
        {
            // Create a temporary root to find DontDestroyOnLoad objects.
            var tempGO = new GameObject("TempCleanup");
            Object.DontDestroyOnLoad(tempGO);

            // Walk the DontDestroyOnLoad scene.
            var dontDestroyScene = tempGO.scene;
            Object.Destroy(tempGO);

            if (!dontDestroyScene.isLoaded)
                return;

            foreach (var root in dontDestroyScene.GetRootGameObjects())
            {
                // Keep the temporary cleaner alive until after the loop.
                if (root.name == "TempCleanup")
                    continue;

                // Destroy each manager. Adjust filtering as needed (e.g., keep
                // EventSystem, AudioListener, etc.)
                var manager = root.GetComponent<GameManager>();
                if (manager != null)
                {
                    Object.Destroy(root);
                }
                else
                {
                    // Fallback: destroy anything that isn't a Unity-critical object.
                    Object.Destroy(root);
                }
            }
        }
    }
}
