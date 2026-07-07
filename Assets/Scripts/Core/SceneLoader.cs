using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPGDragon.Core
{
    /// <summary>
    /// Singleton manager for scene-to-scene transitions with a fade-to-black
    /// overlay. After a scene loads, the player is teleported to a named
    /// spawn point ("SpawnPoint_{entranceId}") if one is provided.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        [Header("Fade Settings")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float defaultFadeDuration = 1f;

        private bool _isLoading;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create a fade overlay at runtime if none was assigned in the Inspector.
            if (fadeCanvasGroup == null)
            {
                CreateFadeOverlay();
            }
        }

        /// <summary>
        /// Builds a full-screen black CanvasGroup overlay at runtime so the
        /// SceneLoader works without any scene setup.
        /// </summary>
        private void CreateFadeOverlay()
        {
            GameObject canvasObj = new GameObject("FadeCanvas");
            canvasObj.transform.SetParent(transform);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            // Stretch the Canvas to fill the screen.
            RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.sizeDelta = Vector2.zero;

            // Add a black Image for the visual overlay.
            Image image = canvasObj.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;

            // CanvasGroup controls alpha and blocks raycasts when fully opaque.
            fadeCanvasGroup = canvasObj.AddComponent<CanvasGroup>();
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// Initiates an asynchronous scene load. Optionally teleports the player
        /// GameObject to a spawn point named "SpawnPoint_{entranceId}" once the
        /// scene is fully loaded.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load (must be in Build Settings).</param>
        /// <param name="entranceId">
        /// Identifier for the entrance; leave empty or null to skip teleportation.
        /// </param>
        /// <param name="onComplete">Optional callback invoked after the fade-in finishes.</param>
        public void LoadScene(string sceneName, string entranceId = "", Action onComplete = null)
        {
            if (_isLoading) return;
            StartCoroutine(LoadSceneCoroutine(sceneName, entranceId, onComplete));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName, string entranceId, Action onComplete)
        {
            _isLoading = true;

            // ── Fade out ──────────────────────────────────────────────────────
            yield return StartCoroutine(Fade(1f, defaultFadeDuration));

            // ── Load scene async ──────────────────────────────────────────────
            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = true;

            while (!asyncOp.isDone)
            {
                yield return null;
            }

            // Wait one frame for newly spawned objects to settle.
            yield return null;

            // ── Teleport player ───────────────────────────────────────────────
            if (!string.IsNullOrEmpty(entranceId)
                && GameManager.Instance != null
                && GameManager.Instance.Player != null)
            {
                GameObject spawnPoint = GameObject.Find("SpawnPoint_" + entranceId);
                if (spawnPoint != null)
                {
                    GameManager.Instance.Player.transform.position = spawnPoint.transform.position;
                }
            }

            // ── Fade in ───────────────────────────────────────────────────────
            yield return StartCoroutine(Fade(0f, defaultFadeDuration));

            _isLoading = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Smoothly interpolates the fade overlay's alpha between its current
        /// value and the target. Blocks raycasts while the overlay is mostly opaque.
        /// </summary>
        private IEnumerator Fade(float targetAlpha, float duration)
        {
            if (fadeCanvasGroup == null) yield break;

            float startAlpha = fadeCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            fadeCanvasGroup.alpha = targetAlpha;
            fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.5f;
        }
    }
}
