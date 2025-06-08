using UnityEngine;

namespace SLC.SpaceHorror
{
    public class MinimapShipIcon : MonoBehaviour
    {
        [Header("References")]
        public Transform shipTransform;
        public RectTransform iconRect;
        public RectTransform minimapRect;

        [Header("Map Bounds")]
        public MinimapBoundsData boundsData;

        private Vector2 worldMin;
        private Vector2 worldMax;
        private Vector2 minimapSize;
        private bool validReferences;

        private void OnValidate()
        {
            CacheReferences();
        }

        private void Start()
        {
            CacheReferences();
        }

        private void CacheReferences()
        {
            validReferences = shipTransform != null && iconRect != null && minimapRect != null && boundsData != null;
            if (validReferences)
            {
                worldMin = boundsData.worldMin;
                worldMax = boundsData.worldMax;
                minimapSize = minimapRect.rect.size;
            }
        }

        void Update()
        {
            if (!validReferences)
                return;

            Vector2 worldPos = new(shipTransform.position.x, shipTransform.position.z);

            // Normalize position to 0-1 range within bounds
            float normalizedX = Mathf.InverseLerp(worldMin.x, worldMax.x, worldPos.x);
            float normalizedY = Mathf.InverseLerp(worldMin.y, worldMax.y, worldPos.y);

            // Convert normalized position to minimap coordinates centered at (0,0)
            float posX = (normalizedX - 0.5f) * minimapSize.x;
            float posY = (normalizedY - 0.5f) * minimapSize.y;

            iconRect.anchoredPosition = new Vector2(posX, posY);

            // Rotate icon to match ship's yaw, inverted for minimap convention
            iconRect.localRotation = Quaternion.Euler(0f, 0f, -shipTransform.eulerAngles.y);
        }
    }
}