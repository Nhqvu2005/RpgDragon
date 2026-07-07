using UnityEngine;

namespace RPGDragon.Quest
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "RPGDragon/Quest/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [Header("Quest Info")]
        [SerializeField] private string questId;
        [SerializeField] private string title;
        [SerializeField][TextArea(3, 6)] private string description;
        [SerializeField] private QuestObjective[] objectives;
        [SerializeField] private int rewardExp;

        [Header("NPC References")]
        [SerializeField] private string npcGiverId;
        [SerializeField] private string npcTurnInId;

        public string QuestId => questId;
        public string Title => title;
        public string Description => description;
        public QuestObjective[] Objectives => objectives;
        public int RewardExp => rewardExp;
        public string NpcGiverId => npcGiverId;
        public string NpcTurnInId => npcTurnInId;

        public bool IsAllObjectivesCompleted()
        {
            if (objectives == null || objectives.Length == 0)
                return false;

            for (int i = 0; i < objectives.Length; i++)
            {
                if (!objectives[i].IsCompleted)
                    return false;
            }
            return true;
        }
    }
}
