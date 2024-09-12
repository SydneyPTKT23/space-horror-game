using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class SpectrumAnalyzer : MonoBehaviour
    {
        public AnalyzerSettings settings;

        private float[] spectrum;
        private List<GameObject> pillars;
        private GameObject folder;
        private bool isBuilding;

        private void Start()
        {
            isBuilding = true;
            CreatePillars();
        }

        private void CreatePillars()
        {

            GameObject t_prefab = settings.Prefabs.BoxPrefab;

            pillars = MathB.ShapesOfGameObjects(t_prefab, settings.pillar.radius, (int)settings.pillar.amount);

            folder = new GameObject("Pillars-" + pillars.Count);
            folder.transform.SetParent(transform);

            foreach (var pillar in pillars)
            {
                pillar.transform.SetParent(folder.transform);
            }

            isBuilding = false;
        }

        private void Update()
        {
            spectrum = AudioListener.GetSpectrumData((int)settings.spectrum.sampleRate, 0, FFTWindow.Blackman);

            for (int i = 0; i < pillars.Count; i++)
            {
                float level = spectrum[i] * settings.pillar.sensitivity * Time.deltaTime * 1000;

                Vector3 t_previousScale = pillars[i].transform.localScale;
                t_previousScale.y = Mathf.Lerp(t_previousScale.y, level, settings.pillar.speed * Time.deltaTime);
                //Add delta time please.
                pillars[i].transform.localScale = t_previousScale;

                //Move pillars up by scale / 2.
                Vector3 t_posisiton = pillars[i].transform.position;
                t_posisiton.y = t_previousScale.y * .5f;
                pillars[i].transform.position = t_posisiton;
            }
        }
    }
}
