using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPGDragon.NPC
{
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "RPGDragon/NPC/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private List<DialogueLine> lines = new List<DialogueLine>();

        public List<DialogueLine> Lines => lines;
    }

    [Serializable]
    public struct DialogueLine
    {
        [Header("Dialogue Text")]
        public string speakerName;

        [TextArea(2, 4)]
        public string text;

        [Header("Quest Triggers")]
        [Tooltip("Quest ID to give when this line is reached. Shows accept/decline choice.")]
        public string questTrigger;

        [Tooltip("Quest ID to complete when this line is reached.")]
        public string questComplete;
    }
}
