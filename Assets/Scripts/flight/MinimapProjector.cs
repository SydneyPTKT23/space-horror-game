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

        /// <summary>
        /// Projects a world position (XZ) to a minimap UI position.
        /// </summary>
        public Vector2 ProjectWorldToMinimap(Vector3 worldPosition)
        {
            Vector2 worldMin = minimapBoundsData.worldMin;
            Vector2 worldMax = minimapBoundsData.worldMax;
            Vector2 worldSize = worldMax - worldMin;

            Vector2 offset = new Vector2(worldPosition.x, worldPosition.z) - worldMin;
            Vector2 normalized = new Vector2(offset.x / worldSize.x, offset.y / worldSize.y);
            Vector2 minimapSize = minimapRect.rect.size;

            return Vector2.Scale(normalized, minimapSize);
        }

        /// <summary>
        /// Converts world radius to minimap icon radius (in pixels).
        /// </summary>
        public float ProjectRadiusToMinimap(float worldRadius)
        {
            Vector2 worldSize = minimapBoundsData.worldMax - minimapBoundsData.worldMin;
            // Use average of x and y scale for radius scaling (or pick one axis)
            float averageSize = (worldSize.x + worldSize.y) * 0.5f;
            return (worldRadius / averageSize) * minimapRect.rect.width;
        }

        /// <summary>
        /// Determines if the object would be on-screen in the minimap.
        /// </summary>
        public bool IsOnMinimap(Vector3 worldPosition)
        {
            Vector2 worldMin = minimapBoundsData.worldMin;
            Vector2 worldMax = minimapBoundsData.worldMax;

            Vector2 pos = new Vector2(worldPosition.x, worldPosition.z);
            return pos.x >= worldMin.x && pos.x <= worldMax.x &&
                   pos.y >= worldMin.y && pos.y <= worldMax.y;
        }
    }
}