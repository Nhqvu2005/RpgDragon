using System;
using System.Collections.Generic;
using UnityEngine;
using RPGDragon.Core;

namespace RPGDragon.Quest
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Quest Lists")]
        [SerializeField] private List<QuestData> activeQuests = new List<QuestData>();
        [SerializeField] private List<QuestData> completedQuests = new List<QuestData>();

        public IReadOnlyList<QuestData> ActiveQuests => activeQuests;
        public IReadOnlyList<QuestData> CompletedQuests => completedQuests;

        /// <summary>
        /// Fired when a quest is started (accepted). Subscribers receive the QuestData.
        /// </summary>
        public event Action<QuestData> QuestStartedEvent;

        /// <summary>
        /// Fired when a quest is fully completed and turned in.
        /// </summary>
        public event Action<QuestData> QuestCompletedEvent;

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

        private void OnEnable()
        {
            EventBus.Register<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Register<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDisable()
        {
            EventBus.Unregister<EnemyDefeatedEvent>(OnEnemyDefeated);
            EventBus.Unregister<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnEnemyDefeated(EnemyDefeatedEvent evt)
        {
            UpdateProgressForType(ObjectiveType.Kill, evt.EnemyType);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt)
        {
            // If a quest was completed during dialogue, check for follow-up logic.
            // This hook exists for future quest-chain progression or post-dialogue rewards.
            if (!string.IsNullOrEmpty(evt.QuestId))
            {
                Debug.Log($"[QuestManager] Dialogue ended with quest {evt.QuestId} context.");
            }
        }

        private void UpdateProgressForType(ObjectiveType type, string targetId)
        {
            foreach (QuestData quest in activeQuests)
            {
                foreach (QuestObjective objective in quest.Objectives)
                {
                    if (objective.type == type && objective.targetId == targetId)
                    {
                        objective.Increment();
                        EventBus.Raise(new QuestProgressEvent
                        {
                            QuestId = quest.QuestId,
                            ObjectiveId = objective.targetId,
                            Current = objective.currentCount,
                            Target = objective.targetCount
                        });
                        return;
                    }
                }
            }
        }

        public void AcceptQuest(QuestData quest)
        {
            if (quest == null)
            {
                Debug.LogWarning("[QuestManager] Attempted to accept a null quest.");
                return;
            }

            if (HasQuest(quest.QuestId) || IsQuestCompleted(quest.QuestId))
            {
                Debug.Log($"[QuestManager] Quest '{quest.Title}' is already active or completed.");
                return;
            }

            foreach (QuestObjective objective in quest.Objectives)
            {
                objective.Reset();
            }

            activeQuests.Add(quest);
            QuestStartedEvent?.Invoke(quest);
            EventBus.Raise(new QuestStartedEvent { QuestId = quest.QuestId });
            Debug.Log($"[QuestManager] Quest started: {quest.Title}");
        }

        public void UpdateProgress(string questId, string targetId)
        {
            QuestData quest = GetActiveQuest(questId);
            if (quest == null) return;

            foreach (QuestObjective objective in quest.Objectives)
            {
                if (objective.targetId == targetId)
                {
                    objective.Increment();
                    EventBus.Raise(new QuestProgressEvent
                    {
                        QuestId = quest.QuestId,
                        ObjectiveId = objective.targetId,
                        Current = objective.currentCount,
                        Target = objective.targetCount
                    });
                    return;
                }
            }
        }

        public void CompleteQuest(string questId)
        {
            QuestData quest = GetActiveQuest(questId);
            if (quest == null)
            {
                Debug.LogWarning($"[QuestManager] Quest '{questId}' not found in active quests.");
                return;
            }

            if (!quest.IsAllObjectivesCompleted())
            {
                Debug.LogWarning($"[QuestManager] Cannot complete quest '{quest.Title}' — not all objectives are done.");
                return;
            }

            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            Debug.Log($"[QuestManager] Quest completed: {quest.Title}, Reward EXP: {quest.RewardExp}");

            QuestCompletedEvent?.Invoke(quest);
            EventBus.Raise(new QuestCompletedEvent { QuestId = quest.QuestId });
        }

        public QuestData GetActiveQuest(string questId)
        {
            return activeQuests.Find(q => q.QuestId == questId);
        }

        public bool HasQuest(string questId)
        {
            return activeQuests.Exists(q => q.QuestId == questId);
        }

        public bool IsQuestCompleted(string questId)
        {
            return completedQuests.Exists(q => q.QuestId == questId);
        }
    }
}
