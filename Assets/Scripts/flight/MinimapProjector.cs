using UnityEngine;

namespace SLC.SpaceHorror
{
    public class MinimapProjector : MonoBehaviour
    {
        [Header("Minimap Area Settings")]
        public Vector2 worldCenter = Vector2.zero;
        public MinimapBoundsData minimapBoundsData;

        [Header("Minimap UI")]
        public RectTransform minimapRect; // The UI container for the minimap

        private Vector2 worldMin;
        private Vector2 worldMax;
        private Vector2 worldSize;
        private Vector2 minimapSize;

        private void OnValidate()
        {
            CacheBounds();
            CacheMinimapSize();
        }

        private void Start()
        {
            CacheBounds();
            CacheMinimapSize();
        }

        private void CacheBounds()
        {
            if (minimapBoundsData != null)
            {
                worldMin = minimapBoundsData.worldMin;
                worldMax = minimapBoundsData.worldMax;
                worldSize = worldMax - worldMin;
            }
            else
            {
                worldMin = Vector2.zero;
                worldMax = Vector2.one;
                worldSize = Vector2.one;
            }
        }

        private void CacheMinimapSize()
        {
            if (minimapRect != null)
            {
                minimapSize = minimapRect.rect.size;
            }
            else
            {
                minimapSize = Vector2.one;
            }
        }

        /// <summary>
        /// Projects a world position (XZ) to a minimap UI position.
        /// </summary>
        public Vector2 ProjectWorldToMinimap(Vector3 worldPosition)
        {
            if (worldSize.x == 0 || worldSize.y == 0)
                return Vector2.zero;

            Vector2 pos = new(worldPosition.x, worldPosition.z);
            Vector2 offset = pos - worldMin;

            // Normalized coordinates [0..1]
            Vector2 normalized = new(offset.x / worldSize.x, offset.y / worldSize.y);

            // Map to minimap rect size
            return Vector2.Scale(normalized, minimapSize);
        }

        /// <summary>
        /// Converts world radius to minimap icon radius (in pixels).
        /// </summary>
        public float ProjectRadiusToMinimap(float worldRadius)
        {
            if (worldSize.x == 0 && worldSize.y == 0)
                return 0f;

            float averageSize = (worldSize.x + worldSize.y) * 0.5f;
            return (worldRadius / averageSize) * minimapSize.x;
        }

        /// <summary>
        /// Determines if the object would be on-screen in the minimap.
        /// </summary>
        public bool IsOnMinimap(Vector3 worldPosition)
        {
            Vector2 pos = new(worldPosition.x, worldPosition.z);
            return pos.x >= worldMin.x && pos.x <= worldMax.x &&
                   pos.y >= worldMin.y && pos.y <= worldMax.y;
        }
    }
}