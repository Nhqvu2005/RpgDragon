using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RPGDragon.UI
{
    /// <summary>
    /// Floating damage number that fades out over one second. Uses a simple
    /// object pool to avoid allocations.
    ///
    /// <para>
    /// Usage:
    /// <code>DamagePopup.Create(worldPosition, damageAmount, isCritical);</code>
    /// </para>
    /// </summary>
    public class DamagePopup : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI damageText;

        [Header("Settings")]
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float horizontalSpread = 0.5f;

        [Header("Critical Hit")]
        [SerializeField] private Color criticalColor = Color.yellow;
        [SerializeField] private float criticalFontSize = 48f;
        [SerializeField] private float normalFontSize = 32f;
        [SerializeField] private Color normalColor = Color.white;

        // ── Object Pool ─────────────────────────────────────────────────────────

        private static readonly Queue<DamagePopup> _pool = new Queue<DamagePopup>();
        private static DamagePopup _prefab;

        private CanvasGroup _canvasGroup;
        private float _elapsed;

        // ── Unity Lifecycle ─────────────────────────────────────────────────────

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();

            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (damageText == null)
                damageText = GetComponentInChildren<TextMeshProUGUI>();
        }

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;

            // Float upward.
            transform.Translate(Vector3.up * (floatSpeed * Time.unscaledDeltaTime));

            // Fade alpha.
            float t = Mathf.Clamp01(_elapsed / lifetime);
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f - t;

            if (_elapsed >= lifetime)
                ReturnToPool();
        }

        // ── Static Factory ──────────────────────────────────────────────────────

        /// <summary>
        /// Create a floating damage number at the given world position.
        /// </summary>
        /// <param name="position">World position to spawn the popup.</param>
        /// <param name="damage">Amount to display.</param>
        /// <param name="isCritical">If true, uses larger yellow text.</param>
        public static void Create(Vector2 position, int damage, bool isCritical = false)
        {
            DamagePopup popup = GetFromPool();

            // Position with slight random horizontal offset.
            float offsetX = Random.Range(-popup.horizontalSpread, popup.horizontalSpread);
            Vector3 spawnPos = new Vector3(position.x + offsetX, position.y, 0f);
            popup.transform.position = spawnPos;

            // Reset state.
            popup._elapsed = 0f;

            if (popup._canvasGroup != null)
                popup._canvasGroup.alpha = 1f;

            // Configure text.
            if (popup.damageText != null)
            {
                popup.damageText.text = damage.ToString();

                if (isCritical)
                {
                    popup.damageText.color = popup.criticalColor;
                    popup.damageText.fontSize = popup.criticalFontSize;
                }
                else
                {
                    popup.damageText.color = popup.normalColor;
                    popup.damageText.fontSize = popup.normalFontSize;
                }
            }

            popup.gameObject.SetActive(true);
        }

        /// <summary>
        /// Register a prefab for the object pool. Call once (e.g. in a Bootstrapper).
        /// </summary>
        public static void SetPrefab(DamagePopup prefab)
        {
            _prefab = prefab;
        }

        // ── Pool Helpers ────────────────────────────────────────────────────────

        private static DamagePopup GetFromPool()
        {
            while (_pool.Count > 0)
            {
                var popup = _pool.Dequeue();

                if (popup != null)
                    return popup;
            }

            // Pool exhausted — instantiate a new one.
            if (_prefab == null)
            {
                Debug.LogError("[DamagePopup] No prefab registered. Call DamagePopup.SetPrefab() before using Create().");
                return null;
            }

            return Object.Instantiate(_prefab);
        }

        private void ReturnToPool()
        {
            gameObject.SetActive(false);
            _pool.Enqueue(this);
        }
    }
}
