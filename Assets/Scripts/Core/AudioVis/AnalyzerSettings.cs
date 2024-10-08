using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public enum SampleRates
    {
        Hz128 = 128,
        Hz256 = 256,
        Hz512 = 512,
        Hz1024 = 1024,
        Hz2048 = 2048
    }

    [System.Serializable]
    public struct AnalyzerSettings
    {
        public PillarSettings pillar;
        public SpectrumSettings spectrum;
    }

    [System.Serializable]
    public struct SpectrumSettings
    {
        public FFTWindow FffWindowType;
        public SampleRates sampleRate;

        public void Reset()
        {
            FffWindowType = FFTWindow.BlackmanHarris;
            sampleRate = SampleRates.Hz2048;
        }
    }

    [System.Serializable]
    public struct PillarSettings
    {
        public float sensitivity;
        public float speed;

        public void Reset()
        {
            //sensitivity = 40;
            //speed = 5;
        }
    }
}