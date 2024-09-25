using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SLC.SpaceHorror.Core
{
    public class AudioVisualizer : MonoBehaviour
    {
        public float minHeight = 15f;
        public float maxHeight = 400f;

        public float sens = 0.5f;

        [Range(64, 8192)] public int samples = 64;

        public Color m_color = Color.blue;
        private AudioPillar[] pillars;

        [Space(15)]
        public AudioClip m_clip;
        public bool loop = true;

        private AudioSource m_audioSource;

        private void Start()
        {
            pillars = GetComponentsInChildren<AudioPillar>();

            if (!m_clip)
                return;

            m_audioSource = new GameObject("Audio Source").AddComponent<AudioSource>();
            m_audioSource.loop = loop;
            m_audioSource.clip = m_clip;
            m_audioSource.Play();
        }

        [System.Obsolete]
        private void Update()
        {
            float[] spectrumData = m_audioSource.GetSpectrumData(samples, 0, FFTWindow.Rectangular);

            for (int i = 0; i < pillars.Length; i++)
            {
                Vector2 t_new = pillars[i].GetComponent<RectTransform>().rect.size;

                t_new.y = Mathf.Clamp(Mathf.Lerp(t_new.y, minHeight + (spectrumData[i] * (maxHeight - minHeight) * 5.0f), sens), minHeight, maxHeight);
                pillars[i].GetComponent<RectTransform>().sizeDelta = t_new;

                pillars[i].GetComponent<Image>().color = m_color;
            }
        }
    }
}