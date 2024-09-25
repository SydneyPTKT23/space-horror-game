using UnityEngine;
using UnityEngine.UI;

namespace SLC.SpaceHorror.Core
{
    public class AudioVisualizer : MonoBehaviour
    {
        public Image image;
        public RectTransform rect;

        private void Start()
        {
            image = GetComponent<Image>();
            rect = GetComponent<RectTransform>();
        }
    }
}
