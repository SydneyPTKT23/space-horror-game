using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class SpectrumAnalyzer : MonoBehaviour
    {
        public AnalyzerSettings settings;

        public Transform parent;
        public List<AudioVisualizer> pillars;

        public float minHeight = 15f;
        public float maxHeight = 400f;

        public Color m_color = Color.blue;

        [Space(15)]
        public AudioClip m_clip;
        public bool loop = true;

        private AudioSource m_audioSource;

        private void OnValidate()
        {
            if (parent != null)
                parent.GetComponentsInChildren(includeInactive: true, result: pillars);
        }

        private void Start()
        {
            if (!m_clip)
                return;

            m_audioSource = GetComponent<AudioSource>();
            m_audioSource.loop = loop;
            m_audioSource.clip = m_clip;
            m_audioSource.Play();
        }

        [System.Obsolete]
        private void Update()
        {
            float[] t_spectrumData = m_audioSource.GetSpectrumData((int)settings.spectrum.sampleRate, 0, settings.spectrum.FffWindowType);

            for (int i = 0; i < pillars.Count; i++)
            {
                Vector2 t_desiredSize = pillars[i].GetComponent<RectTransform>().rect.size;
                float t_soundLevel = minHeight + (t_spectrumData[i] * (maxHeight - minHeight) * settings.pillar.speed);

                t_desiredSize.y = Mathf.Clamp(Mathf.Lerp(t_desiredSize.y, t_soundLevel, settings.pillar.sensitivity * Time.deltaTime), minHeight, maxHeight);

                pillars[i].rect.sizeDelta = t_desiredSize;
                pillars[i].image.color = m_color;
            }
        }
    }
}