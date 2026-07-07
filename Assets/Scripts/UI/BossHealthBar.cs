using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGDragon.Core;

namespace RPGDragon.UI
{
    /// <summary>
    /// Boss-specific health bar. Hidden by default; shown via <see cref="Show"/>
    /// when a boss encounter begins and hidden when the boss is defeated.
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject barContainer;
        [SerializeField] private Image fillImage;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private TextMeshProUGUI hpText;

        [Header("Settings")]
        [SerializeField] private float lerpSpeed = 5f;

        private float _maxHP = 1f;
        private float _targetFill = 1f;
        private float _currentFill = 1f;

        // ── Unity Lifecycle ─────────────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Register<BossDefeatedEvent>(OnBossDefeated);
        }

        private void OnDisable()
        {
            EventBus.Unregister<BossDefeatedEvent>(OnBossDefeated);
        }

        private void Awake()
        {
            if (barContainer != null)
                barContainer.SetActive(false);
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
        /// Show the boss health bar and set the boss name and max HP.
        /// </summary>
        public void Show()
        {
            if (barContainer != null)
                barContainer.SetActive(true);
        }

        /// <summary>
        /// Hide the boss health bar.
        /// </summary>
        public void Hide()
        {
            if (barContainer != null)
                barContainer.SetActive(false);
        }

        /// <summary>
        /// Initialise the bar for a new boss encounter.
        /// </summary>
        /// <param name="bossName">Display name shown above the bar.</param>
        /// <param name="maxHP">Total health of the boss.</param>
        public void SetBoss(string bossName, float maxHP)
        {
            _maxHP = maxHP;
            _targetFill = 1f;
            _currentFill = 1f;

            if (bossNameText != null)
                bossNameText.text = bossName;

            if (fillImage != null)
            {
                fillImage.fillAmount = 1f;
                fillImage.color = Color.green;
            }

            UpdateHpText(maxHP, maxHP);
            Show();
        }

        /// <summary>
        /// Update the boss health bar with the current HP value.
        /// </summary>
        public void UpdateHealth(float currentHP)
        {
            _targetFill = Mathf.Clamp01(currentHP / _maxHP);
            UpdateColor(_targetFill);
            UpdateHpText(currentHP, _maxHP);
        }

        // ── Event Handlers ──────────────────────────────────────────────────────

        private void OnBossDefeated(BossDefeatedEvent evt)
        {
            _targetFill = 0f;
            UpdateColor(0f);
            UpdateHpText(0, _maxHP);

            // Brief delay before hiding so the player sees the bar empty out.
            Invoke(nameof(Hide), 1.5f);
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
