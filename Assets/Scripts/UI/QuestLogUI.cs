using System.Collections.Generic;
using UnityEngine;
using TMPro;
using RPGDragon.Core;
using RPGDragon.Quest;

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
            if (questListText == null) return;
            questListText.text = "";

            var activeQuests = QuestManager.Instance.ActiveQuests;
            if (activeQuests == null || activeQuests.Count == 0)
            {
                questLogPanel.SetActive(false);
                if (emptyStateMessage != null)
                    emptyStateMessage.SetActive(true);
                return;
            }

            if (emptyStateMessage != null)
                emptyStateMessage.SetActive(false);

            foreach (var quest in activeQuests)
            {
                questListText.text += $"<b>{quest.Title}</b>\n{quest.Description}\n";
                foreach (var obj in quest.Objectives)
                {
                    string status = obj.IsCompleted ? " <color=green>✓</color>" : $" ({obj.currentCount}/{obj.targetCount})";
                    questListText.text += $"  - {obj.targetId}{status}\n";
                }
                questListText.text += "\n";
            }
        }

        // ── Event Handlers ──────────────────────────────────────────────────────

        private void OnQuestChanged<T>(T evt) where T : struct
        {
            if (_isOpen)
                UpdateQuestLog();
        }
    }
}
