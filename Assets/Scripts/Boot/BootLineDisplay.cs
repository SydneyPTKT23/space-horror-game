using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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

        private void Awake()
        {
            if (textComponent == null)
                textComponent = GetComponent<TMP_Text>();

            // Ensure no leftover text from prefab
            if (textComponent != null)
                textComponent.text = "";
        }

        public void SetTextInstant(string text)
        {
            if (textComponent != null)
            {
                textComponent.color = Color.white;
                textComponent.text = text;
            }
        }

        public IEnumerator TypeLineWithEffects(string line, bool glitch, float charDelay)
        {
            if (textComponent == null)
                yield break;

            textComponent.text = "";
            textComponent.color = glitch ? glitchColor : Color.white;

            List<char> typedChars = new();
            int cursor = 0;

            while (cursor < line.Length)
            {
                char nextChar = line[cursor];

                // Apply glitch character
                if (glitch && Random.value < glitchCharChance && !char.IsWhiteSpace(nextChar))
                {
                    nextChar = glitchChars[Random.Range(0, glitchChars.Length)];
                }

                typedChars.Add(nextChar);
                textComponent.text = new string(typedChars.ToArray());
                yield return new WaitForSeconds(charDelay);

                // Backtrack simulation
                if (enableBacktrack && Random.value < backtrackChance && typedChars.Count > 5)
                {
                    int backtrackCount = Random.Range(2, Mathf.Min(5, typedChars.Count));
                    cursor = Mathf.Max(cursor - backtrackCount, 0);
                    typedChars.RemoveRange(cursor, typedChars.Count - cursor);
                    textComponent.text = new string(typedChars.ToArray());
                    yield return new WaitForSeconds(charDelay * 2f);
                }
                else
                {
                    cursor++;
                }
            }

            textComponent.text = new string(typedChars.ToArray());
        }
    }
}