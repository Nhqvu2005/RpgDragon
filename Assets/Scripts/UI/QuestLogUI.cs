using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPGDragon.Core;

namespace RPGDragon.UI
{
    /// <summary>
    /// Displays the player's active quests and their progress. Toggled with the J key
    /// and auto-refreshes via <see cref="EventBus"/>.
    ///
    /// <para>
    /// Depends on <c>RPGDragon.Quest.QuestManager</c> which is expected to expose:
    /// <code>
    /// public static List&lt;QuestData&gt; ActiveQuests { get; }
    /// </code>
    /// where <c>QuestData</c> has Title, ObjectiveDescription, CurrentProgress, TargetProgress,
    /// and IsCompleted fields.
    /// </para>
    /// </summary>
    public class QuestLogUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject questLogPanel;
        [SerializeField] private TextMeshProUGUI questListText;
        [SerializeField] private GameObject emptyStateMessage;

        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.J;

        private bool _isOpen;

        // ── Unity Lifecycle ─────────────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Register<QuestStartedEvent>(OnQuestChanged);
            EventBus.Register<QuestProgressEvent>(OnQuestChanged);
            EventBus.Register<QuestCompletedEvent>(OnQuestChanged);
        }

        private void OnDisable()
        {
            EventBus.Unregister<QuestStartedEvent>(OnQuestChanged);
            EventBus.Unregister<QuestProgressEvent>(OnQuestChanged);
            EventBus.Unregister<QuestCompletedEvent>(OnQuestChanged);
        }

        private void Start()
        {
            if (questLogPanel != null)
                questLogPanel.SetActive(false);

            _isOpen = false;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleQuestLog();
            }
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Toggle the quest log panel open/closed.
        /// </summary>
        public void ToggleQuestLog()
        {
            _isOpen = !_isOpen;

            if (questLogPanel != null)
                questLogPanel.SetActive(_isOpen);

            if (_isOpen)
                UpdateQuestLog();
        }

        /// <summary>
        /// Clear and rebuild the quest list from <c>QuestManager.ActiveQuests</c>.
        /// </summary>
        public void UpdateQuestLog()
        {
            // Fetch active quests from the QuestManager.
            // Replace this with the actual QuestManager API once implemented.
            var activeQuests = GetActiveQuests();

            if (activeQuests == null || activeQuests.Count == 0)
            {
                if (questListText != null)
                    questListText.text = string.Empty;

                if (emptyStateMessage != null)
                    emptyStateMessage.SetActive(true);

                if (questLogPanel != null && _isOpen)
                    questLogPanel.SetActive(false);

                return;
            }

            if (emptyStateMessage != null)
                emptyStateMessage.SetActive(false);

            if (questListText == null)
                return;

            var sb = new System.Text.StringBuilder();

            foreach (var quest in activeQuests)
            {
                // Format: [title] - [objective] ([current]/[target])
                string checkmark = quest.IsCompleted ? "<color=green>✓ </color>" : "";
                sb.AppendLine($"{checkmark}{quest.Title} - {quest.ObjectiveDescription} ({quest.CurrentProgress}/{quest.TargetProgress})");
            }

            questListText.text = sb.ToString().TrimEnd();
        }

        // ── Event Handlers ──────────────────────────────────────────────────────

        private void OnQuestChanged<T>(T evt) where T : struct
        {
            if (_isOpen)
                UpdateQuestLog();
        }

        // ── Quest Data Adapter ──────────────────────────────────────────────────

        /// <summary>
        /// Placeholder method that reflects the expected QuestManager API.
        /// When RPGDragon.Quest.QuestManager is implemented, replace the body with:
        /// <code>return QuestManager.ActiveQuests;</code>
        /// </summary>
        private List<QuestData> GetActiveQuests()
        {
            // TODO: Wire up to RPGDragon.Quest.QuestManager.ActiveQuests
            return new List<QuestData>();
        }

        // ── Inner Quest Data Type ───────────────────────────────────────────────

        /// <summary>
        /// Lightweight data contract used by the quest log UI.
        /// This mirrors the fields expected from RPGDragon.Quest.QuestData.
        /// </summary>
        public class QuestData
        {
            public string Title;
            public string ObjectiveDescription;
            public int CurrentProgress;
            public int TargetProgress;
            public bool IsCompleted;
        }
    }
}
