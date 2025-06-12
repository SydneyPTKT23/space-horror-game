using UnityEngine;
using TMPro;
using System.Collections;

namespace SLC.SpaceHorror
{
    public class BootLineDisplay : MonoBehaviour
    {
        public TMP_Text textComponent;

        [Header("Glitch Settings")]
        public Color glitchColor = Color.green;
        public string glitchChars = "#$%&@*+=<>?/\\";
        public float glitchCharChance = 0.15f;

        [Header("Backtrack Settings")]
        [Range(0f, 1f)] public float backtrackChance = 0.3f;
        public bool enableBacktrack = true;

        [Header("Fade Settings")]
        public float fadeDuration = 0.5f;

        [Header("Cursor Settings")]
        public string cursorChar = "_";
        public float cursorBlinkInterval = 0.5f;

        private bool showCursor = true;
        private float cursorTimer = 0f;
        private string currentText = "";
        private bool isTyping = false;

        private void Awake()
        {
            if (textComponent == null)
                textComponent = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (!isTyping || textComponent == null) return;

            cursorTimer += Time.deltaTime;
            if (cursorTimer >= cursorBlinkInterval)
            {
                cursorTimer = 0f;
                showCursor = !showCursor;
                UpdateTextWithCursor();
            }
        }

        private void UpdateTextWithCursor()
        {
            textComponent.text = currentText + (showCursor ? cursorChar : " ");
        }

        public void SetTextInstant(string text)
        {
            if (textComponent != null)
                textComponent.text = text;
        }

        public void ResetDisplay()
        {
            if (textComponent != null)
            {
                textComponent.text = "";
                textComponent.color = Color.white;
            }
        }

        public IEnumerator FadeIn()
        {
            if (textComponent == null) yield break;

            Color c = textComponent.color;
            c.a = 0f;
            textComponent.color = c;

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                c.a = Mathf.Clamp01(t / fadeDuration);
                textComponent.color = c;
                yield return null;
            }
        }

        public IEnumerator TypeLine(string line, bool glitch, float charDelay)
        {
            if (textComponent == null)
                yield break;

            // Start fully transparent and fade in
            textComponent.color = glitch ? glitchColor : Color.white;
            textComponent.alpha = 0f;

            yield return FadeIn();

            isTyping = true;
            cursorTimer = 0f;
            showCursor = true;

            currentText = "";

            int cursor = 0;

            while (cursor < line.Length)
            {
                char nextChar = line[cursor];

                // Glitch replacement
                if (glitch && !char.IsWhiteSpace(nextChar) && Random.value < glitchCharChance)
                {
                    nextChar = glitchChars[Random.Range(0, glitchChars.Length)];
                }

                currentText += nextChar;
                UpdateTextWithCursor();

                yield return new WaitForSeconds(charDelay);

                // Backtrack
                if (enableBacktrack && Random.value < backtrackChance && currentText.Length > 5)
                {
                    int backtrack = Random.Range(2, Mathf.Min(5, currentText.Length));
                    cursor = Mathf.Max(0, cursor - backtrack);
                    currentText = currentText[..cursor];
                    UpdateTextWithCursor();
                    yield return new WaitForSeconds(charDelay * 2f);
                    continue;
                }

                cursor++;
            }

            // After typing, show cursor for a moment
            yield return new WaitForSeconds(1f);
            isTyping = false;

            // Final text, no cursor
            textComponent.text = currentText;
        }
    }
}