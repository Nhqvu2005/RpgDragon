// This file is deprecated — dialogue UI is managed by RPGDragon.NPC.DialogueSystem.
using System.Collections;
using UnityEngine;
using TMPro;

namespace RPGDragon.UI
{
    /// <summary>
    /// Manages the in-game dialogue panel: typewriter effect, continue prompt,
    /// and show/hide. This is a pass-through UI controller — the actual dialogue
    /// logic lives in <c>RPGDragon.NPC.DialogueSystem</c>, which calls
    /// <see cref="ShowDialogue"/> and <see cref="HideDialogue"/> directly.
    /// <para>Deprecated — kept only for the typewriter/blink presentation layer.</para>
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI dialogueText;
        [SerializeField] private TextMeshProUGUI continuePrompt;

        [Header("Settings")]
        [SerializeField] private float charsPerSecond = 30f;
        [SerializeField] private float blinkInterval = 0.5f;

        private Coroutine _typewriterCoroutine;
        private Coroutine _blinkCoroutine;
        private string _fullText;
        private bool _skipTyping;

        // ── Unity Lifecycle ─────────────────────────────────────────────────────

        private void Start()
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (continuePrompt != null)
                continuePrompt.gameObject.SetActive(false);
        }

        private void Update()
        {
            // Allow player to skip / progress dialogue.
            if (Input.GetKeyDown(KeyCode.E) && dialoguePanel.activeSelf)
            {
                if (_typewriterCoroutine != null)
                {
                    // First press during typewriter: skip to full text.
                    if (!_skipTyping)
                    {
                        _skipTyping = true;
                    }
                    else
                    {
                        // Already skipped — text is fully visible. Next press
                        // would advance; here we just let the caller handle it
                        // via DialogueEndedEvent or similar.
                    }
                }
            }
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// Show the dialogue panel and begin the typewriter effect.
        /// </summary>
        /// <param name="speaker">Name of the speaking character.</param>
        /// <param name="text">Full dialogue text to reveal.</param>
        public void ShowDialogue(string speaker, string text)
        {
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            if (speakerNameText != null)
                speakerNameText.text = speaker;

            _fullText = text;
            _skipTyping = false;

            if (continuePrompt != null)
                continuePrompt.gameObject.SetActive(false);

            // Stop any running typewriter and restart.
            if (_typewriterCoroutine != null)
                StopCoroutine(_typewriterCoroutine);

            _typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        }

        /// <summary>
        /// Hide the dialogue panel and stop all coroutines.
        /// </summary>
        public void HideDialogue()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }

            StopBlink();

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        // ── Coroutines ──────────────────────────────────────────────────────────

        private IEnumerator TypewriterRoutine()
        {
            if (dialogueText != null)
                dialogueText.text = string.Empty;

            float delay = 1f / Mathf.Max(charsPerSecond, 1f);
            int charIndex = 0;

            while (charIndex < _fullText.Length)
            {
                if (_skipTyping)
                {
                    // Reveal remaining text instantly.
                    if (dialogueText != null)
                        dialogueText.text = _fullText;

                    charIndex = _fullText.Length;
                    break;
                }

                if (dialogueText != null)
                    dialogueText.text = _fullText[..(charIndex + 1)];

                charIndex++;
                yield return new WaitForSecondsRealtime(delay);
            }

            _typewriterCoroutine = null;
            StartBlink();
        }

        private void StartBlink()
        {
            StopBlink();

            if (continuePrompt != null)
            {
                continuePrompt.gameObject.SetActive(true);
                _blinkCoroutine = StartCoroutine(BlinkRoutine());
            }
        }

        private void StopBlink()
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }

            if (continuePrompt != null)
                continuePrompt.gameObject.SetActive(false);
        }

        private IEnumerator BlinkRoutine()
        {
            while (true)
            {
                if (continuePrompt != null)
                    continuePrompt.gameObject.SetActive(!continuePrompt.gameObject.activeSelf);

                yield return new WaitForSecondsRealtime(blinkInterval);
            }
        }
    }
}
