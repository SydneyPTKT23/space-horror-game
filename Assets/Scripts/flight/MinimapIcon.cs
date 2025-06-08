using SLC.SpaceHorror;
using UnityEngine;

namespace SLC
{
    public class MinimapIcon : MonoBehaviour
    {
        public Transform target; // The world object this icon represents
        public float worldRadius = 10f;
        public RectTransform iconUI;

        private MinimapProjector projector;

        void Start()
        {
            projector = FindFirstObjectByType<MinimapProjector>();
        }

        void Update()
        {
            if (target == null || projector == null) return;

            Vector2 pos = projector.ProjectWorldToMinimap(target.position);
            float size = projector.ProjectRadiusToMinimap(worldRadius);

            iconUI.anchoredPosition = pos;
            iconUI.sizeDelta = Vector2.one * size;

            iconUI.gameObject.SetActive(projector.IsOnMinimap(target.position));
        }
    }
}