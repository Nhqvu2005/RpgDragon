using UnityEngine;
using RPGDragon.Quest;

namespace RPGDragon.NPC
{
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Settings")]
        [SerializeField] private string npcId;
        [SerializeField] private string npcName;
        [SerializeField] private DialogueData dialogueData;

        [Header("Quest Settings")]
        [SerializeField] private QuestData questToGive;
        [SerializeField] private QuestData questToComplete;

        [Header("UI")]
        [SerializeField] private GameObject questMarker;
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        public string NpcId => npcId;

        private bool playerInRange = false;

        private void Start()
        {
            UpdateQuestMarker();
        }

        private void LateUpdate()
        {
            // Keep marker updated as quest state changes
            if (playerInRange)
                UpdateQuestMarker();
        }

        private void UpdateQuestMarker()
        {
            if (questMarker == null) return;

            bool show = false;

            // "?" marker: player has quest and objectives are done, ready to turn in
            if (questToComplete != null
                && QuestManager.Instance.HasQuest(questToComplete.QuestId)
                && questToComplete.IsAllObjectivesCompleted())
            {
                show = true;
                // TODO: Change marker sprite/color to indicate completion (e.g. "?")
            }
            // "!" marker: quest is available for pickup
            else if (questToGive != null
                && !QuestManager.Instance.HasQuest(questToGive.QuestId)
                && !QuestManager.Instance.IsQuestCompleted(questToGive.QuestId))
            {
                show = true;
                // TODO: Change marker sprite/color to indicate available (e.g. "!")
            }

            questMarker.SetActive(show);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                // TODO: Show "Press E to interact" UI prompt
                Debug.Log($"[NPC] Press {interactKey} to talk to {npcName}");
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (playerInRange && other.CompareTag("Player") && Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                // TODO: Hide interaction UI prompt
            }
        }

        public void Interact()
        {
            if (DialogueSystem.Instance == null)
            {
                Debug.LogError("[NPC] DialogueSystem instance not found.");
                return;
            }

            if (dialogueData == null)
            {
                Debug.LogWarning($"[NPC] {npcName} has no dialogue data assigned.");
                return;
            }

            // Priority 1: Player has a completed quest ready to turn in.
            if (questToComplete != null
                && QuestManager.Instance.HasQuest(questToComplete.QuestId)
                && questToComplete.IsAllObjectivesCompleted())
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, npcId);
                UpdateQuestMarker();
                return;
            }

            // Priority 2: NPC has a quest to give and player doesn't have it yet.
            if (questToGive != null
                && !QuestManager.Instance.HasQuest(questToGive.QuestId)
                && !QuestManager.Instance.IsQuestCompleted(questToGive.QuestId))
            {
                DialogueSystem.Instance.StartDialogue(dialogueData, npcId, questToGive);
                return;
            }

            // Priority 3: Default dialogue.
            DialogueSystem.Instance.StartDialogue(dialogueData, npcId);
        }
    }
}
