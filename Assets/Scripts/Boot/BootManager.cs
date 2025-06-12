using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace SLC.SpaceHorror
{
    public class BootManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject bootLinePrefab;
        public GameObject blankLinePrefab;
        public Transform bootLineContainer;
        public ScrollRect scrollRect;

        [Header("Boot Timing")]
        public float lineDelay = 0.25f;
        public float charDelay = 0.02f;

        [Tooltip("Resource paths (without extensions) for boot text files, processed in order")]
        public string[] bootTextFilePaths = {
            "BootSequences/boot_bios",
            "BootSequences/boot_kernel",
            "BootSequences/boot_ai",
            "BootSequences/boot_complete"
        };

        [Tooltip("Chance (0 to 1) a line will glitch")]
        [Range(0f, 1f)]
        public float glitchChance = 0.15f;

        [Tooltip("Minimum number of lines to glitch per file")]
        public int minGlitchLines = 2;

        private readonly List<string> currentBootLines = new();

        private void Start()
        {
            StartCoroutine(PlayFullBootSequence());
        }

        private IEnumerator PlayFullBootSequence()
        {
            foreach (string resourcePath in bootTextFilePaths)
            {
                LoadBootTextFromFile(resourcePath);
                yield return PlayBootSequenceWithRandomGlitch();

                // Add 1–2 blank lines for segment separation
                int blanks = Random.Range(1, 3);
                for (int i = 0; i < blanks; i++)
                {
                    GameObject blankLine = Instantiate(blankLinePrefab, bootLineContainer);

                    // Optional: ensure text is just a non-breaking space for layout
                    TMP_Text blankTMP = blankLine.GetComponent<TMP_Text>();
                    if (blankTMP != null)
                        blankTMP.text = "\u00A0";

                    yield return null;
                }
            }
        }

        private void LoadBootTextFromFile(string resourcePath)
        {
            currentBootLines.Clear();

            TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogError($"Boot text file not found at Resources/{resourcePath}");
                return;
            }

            using (StringReader reader = new StringReader(textAsset.text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        currentBootLines.Add(line);
                }
            }
        }

        private IEnumerator PlayBootSequenceWithRandomGlitch()
        {
            int linesCount = currentBootLines.Count;
            int glitchLinesCount = Mathf.Min(minGlitchLines, linesCount);

            HashSet<int> glitchIndices = new HashSet<int>();
            while (glitchIndices.Count < glitchLinesCount)
            {
                glitchIndices.Add(Random.Range(0, linesCount));
            }

            for (int i = 0; i < linesCount; i++)
            {
                string line = currentBootLines[i];
                bool forceGlitch = glitchIndices.Contains(i);
                bool shouldGlitch = forceGlitch || Random.value < glitchChance;

                GameObject lineGO = Instantiate(bootLinePrefab, bootLineContainer);
                BootLineDisplay lineDisplay = lineGO.GetComponent<BootLineDisplay>();

                if (lineDisplay != null)
                {
                    yield return StartCoroutine(lineDisplay.TypeLineWithEffects(line, shouldGlitch, charDelay));
                }
                else
                {
                    TMP_Text tmp = lineGO.GetComponent<TMP_Text>();
                    if (tmp != null)
                        tmp.text = shouldGlitch ? Glitchify(line) : line;

                    yield return new WaitForSeconds(lineDelay);
                }

                yield return null;

                if (scrollRect != null)
                    scrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private string Glitchify(string input)
        {
            const string glitchChars = "@#$%&*!?/\\|><^~";
            char[] chars = input.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                if (Random.value < 0.1f)
                {
                    chars[i] = glitchChars[Random.Range(0, glitchChars.Length)];
                }
            }
            return new string(chars);
        }
    }
}
