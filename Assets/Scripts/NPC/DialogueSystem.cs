using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPGDragon.Core;
using RPGDragon.Quest;

namespace RPGDragon.NPC
{
    public enum DialogueState
    {
        Idle,
        Playing,
        WaitingChoice,
        Ended
    }

    [Serializable]
    public class DialogueUI
    {
        [Header("UI Panel")]
        public GameObject panel;

        [Header("Text Elements")]
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI dialogueText;

        [Header("Buttons")]
        public GameObject continueButton;
        public GameObject acceptButton;
        public GameObject declineButton;
    }

    public class DialogueSystem : MonoBehaviour
    {
        public static DialogueSystem Instance { get; private set; }

        [Header("UI Reference")]
        [SerializeField] private DialogueUI dialogueUI;

        [Header("Typewriter Settings")]
        [SerializeField] private float typewriterSpeed = 0.05f;

        private DialogueData currentData;
        private int currentLineIndex = -1;
        private DialogueState state = DialogueState.Idle;
        private Coroutine typewriterCoroutine;
        private bool isSkipping = false;

        // Track which NPC started the dialogue and any quest they offer.
        private string currentNpcId;
        private QuestData currentQuestToGive;
        private string completedQuestId;

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
            if (dialogueUI != null && dialogueUI.panel != null)
                dialogueUI.panel.SetActive(false);
        }

        /// <summary>
        /// Begins a dialogue session. Optionally supply the NPC's ID (for events)
        /// and a QuestData to offer when a line with questTrigger is reached.
        /// </summary>
        public void StartDialogue(DialogueData data, string npcId = "", QuestData questToGive = null)
        {
            if (data == null || data.Lines.Count == 0)
            {
                Debug.LogWarning("[DialogueSystem] Cannot start dialogue: null or empty data.");
                return;
            }

            if (state != DialogueState.Idle && state != DialogueState.Ended)
            {
                EndDialogue();
            }

            currentData = data;
            currentLineIndex = -1;
            currentNpcId = npcId;
            currentQuestToGive = questToGive;
            completedQuestId = null;
            state = DialogueState.Playing;

            if (dialogueUI != null)
            {
                if (dialogueUI.panel != null)
                    dialogueUI.panel.SetActive(true);

                SetChoiceButtonsActive(false);
                SetContinueButtonActive(true);
            }

            // Notify other systems that dialogue has started.
            EventBus.Raise(new DialogueStartedEvent { NpcId = npcId });

            NextLine();
        }

        /// <summary>
        /// Advances to the next dialogue line. Handles quest triggers, quest completion,
        /// and typewriter display.
        /// </summary>
        public void NextLine()
        {
            if (state == DialogueState.WaitingChoice || state == DialogueState.Ended)
                return;

            currentLineIndex++;

            if (currentLineIndex >= currentData.Lines.Count)
            {
                EndDialogue();
                return;
            }

            DialogueLine line = currentData.Lines[currentLineIndex];

            // Update speaker name UI.
            if (dialogueUI != null && dialogueUI.speakerNameText != null)
                dialogueUI.speakerNameText.text = line.speakerName;

            // Start typewriter effect for this line.
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine(line.text));

            // Handle quest trigger (show accept/decline choice).
            if (!string.IsNullOrEmpty(line.questTrigger))
                HandleQuestTrigger(line.questTrigger);

            // Handle quest completion.
            if (!string.IsNullOrEmpty(line.questComplete))
                HandleQuestComplete(line.questComplete);
        }

        private IEnumerator TypewriterRoutine(string fullText)
        {
            isSkipping = false;

            if (dialogueUI == null || dialogueUI.dialogueText == null)
                yield break;

            dialogueUI.dialogueText.text = "";

            foreach (char c in fullText)
            {
                if (isSkipping)
                {
                    dialogueUI.dialogueText.text = fullText;
                    break;
                }

                dialogueUI.dialogueText.text += c;
                yield return new WaitForSeconds(typewriterSpeed);
            }

            typewriterCoroutine = null;
        }

        /// <summary>
        /// Called by the UI continue-skip button. Completes the typewriter instantly
        /// if it is still running.
        /// </summary>
        public void SkipTypewriter()
        {
            isSkipping = true;
        }

        private void SetChoiceButtonsActive(bool active)
        {
            if (dialogueUI == null) return;
            if (dialogueUI.acceptButton != null)
                dialogueUI.acceptButton.SetActive(active);
            if (dialogueUI.declineButton != null)
                dialogueUI.declineButton.SetActive(active);
        }

        private void SetContinueButtonActive(bool active)
        {
            if (dialogueUI == null) return;
            if (dialogueUI.continueButton != null)
                dialogueUI.continueButton.SetActive(active);
        }

        private void HandleQuestTrigger(string questId)
        {
            if (currentQuestToGive == null || currentQuestToGive.QuestId != questId)
                return;

            // Don't offer if the quest is already active or completed.
            if (QuestManager.Instance.HasQuest(questId) || QuestManager.Instance.IsQuestCompleted(questId))
                return;

            state = DialogueState.WaitingChoice;
            SetChoiceButtonsActive(true);
            SetContinueButtonActive(false);
        }

        private void HandleQuestComplete(string questId)
        {
            if (QuestManager.Instance.HasQuest(questId) && QuestManager.Instance.GetActiveQuest(questId).IsAllObjectivesCompleted())
            {
                QuestManager.Instance.CompleteQuest(questId);
                completedQuestId = questId;
            }
        }

        // ── UI Button Callbacks ────────────────────────────────────────────────

        /// <summary>
        /// Called by the accept button in the UI. Accepts the currently offered quest.
        /// </summary>
        public void OnAcceptQuest()
        {
            if (currentQuestToGive == null)
            {
                SetChoiceButtonsActive(false);
                SetContinueButtonActive(true);
                state = DialogueState.Playing;
                return;
            }

            QuestManager.Instance.AcceptQuest(currentQuestToGive);
            currentQuestToGive = null; // Only offer once per conversation.

            SetChoiceButtonsActive(false);
            SetContinueButtonActive(true);
            state = DialogueState.Playing;
        }

        /// <summary>
        /// Called by the decline button in the UI. Declines the currently offered quest.
        /// </summary>
        public void OnDeclineQuest()
        {
            SetChoiceButtonsActive(false);
            SetContinueButtonActive(true);
            state = DialogueState.Playing;
        }

        /// <summary>
        /// Called by the continue button in the UI. Advances to the next line.
        /// </summary>
        public void OnContinueClicked()
        {
            if (state == DialogueState.WaitingChoice || state == DialogueState.Ended)
                return;

            // If typewriter is still running, skip it; otherwise advance.
            if (typewriterCoroutine != null)
            {
                SkipTypewriter();
            }
            else
            {
                NextLine();
            }
        }

        /// <summary>
        /// Ends the dialogue session, hides the UI, and fires DialogueEndedEvent.
        /// </summary>
        public void EndDialogue()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }

            state = DialogueState.Ended;

            if (dialogueUI != null && dialogueUI.panel != null)
                dialogueUI.panel.SetActive(false);

            SetChoiceButtonsActive(false);
            SetContinueButtonActive(false);

            // Notify other systems that dialogue ended.
            EventBus.Raise(new DialogueEndedEvent { QuestId = completedQuestId ?? "" });

            // Fire internal event for systems that subscribe directly (e.g. NPCController cleanup).
            OnDialogueEnded?.Invoke();

            currentData = null;
            currentLineIndex = -1;
            currentNpcId = null;
            currentQuestToGive = null;
            completedQuestId = null;
            state = DialogueState.Idle;
        }

        /// <summary>
        /// Internal event fired when dialogue ends. Used by systems that need
        /// synchronous notification without going through EventBus.
        /// </summary>
        public event Action OnDialogueEnded;
    }
}
