using System;

namespace RPGDragon.Quest
{
    public enum ObjectiveType
    {
        Kill,
        Collect,
        Talk
    }

    [Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string targetId;
        public int targetCount;
        public int currentCount;

        public bool IsCompleted => currentCount >= targetCount;

        public void Reset()
        {
            currentCount = 0;
        }

        public void Increment()
        {
            if (currentCount < targetCount)
            {
                currentCount++;
            }
        }
    }
}
