using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGDragon.Core;

namespace RPGDragon.UI
{
    /// <summary>
    /// Handles the player's health bar display: smooth fill lerp, color transitions,
    /// and event-driven updates via <see cref="EventBus"/>.
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Settings")]
        [SerializeField] private float lerpSpeed = 5f;
        [SerializeField] private float maxHealth = 100f;

        private float _targetFill = 1f;
        private float _currentFill = 1f;

        // ── Unity Lifecycle ─────────────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Register<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Register<PlayerDiedEvent>(OnPlayerDied);
        }

        private void OnDisable()
        {
            EventBus.Unregister<PlayerDamagedEvent>(OnPlayerDamaged);
            EventBus.Unregister<PlayerDiedEvent>(OnPlayerDied);
        }

        private void Awake()
        {
            if (fillImage == null)
                fillImage = GetComponent<Image>();
        }

        private void Update()
        {
            if (Mathf.Approximately(_currentFill, _targetFill))
            {
                _currentFill = _targetFill;
                return;
            }

            _currentFill = Mathf.Lerp(_currentFill, _targetFill, lerpSpeed * Time.unscaledDeltaTime);
            fillImage.fillAmount = _currentFill;
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Manually update the health bar. The bar will smoothly lerp to the new ratio.
        /// </summary>
        public void UpdateHealth(float currentHP, float maxHP)
        {
            _targetFill = Mathf.Clamp01(currentHP / maxHP);
            UpdateColor(_targetFill);
            UpdateHpText(currentHP, maxHP);
        }

        // ── Event Handlers ──────────────────────────────────────────────────────

        private void OnPlayerDamaged(PlayerDamagedEvent evt)
        {
            _targetFill = Mathf.Clamp01(evt.CurrentHP / maxHealth);
            UpdateColor(_targetFill);
            UpdateHpText(evt.CurrentHP, maxHealth);
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            _targetFill = 0f;
            fillImage.color = GetHealthColor(0f);
            UpdateHpText(0, maxHealth);
        }

        // ── Visual Helpers ──────────────────────────────────────────────────────

        private void UpdateColor(float fillRatio)
        {
            if (fillImage != null)
                fillImage.color = GetHealthColor(fillRatio);
        }

        private void UpdateHpText(float current, float max)
        {
            if (hpText != null)
                hpText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
        }

        private Color GetHealthColor(float ratio)
        {
            if (ratio > 0.6f)
                return Color.green;
            if (ratio > 0.3f)
                return Color.yellow;
            return Color.red;
        }
    }
}
