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

        void Update()
        {
            if (!shipTransform || !iconRect || !minimapRect || !boundsData)
                return;

            Vector2 worldPos = new Vector2(shipTransform.position.x, shipTransform.position.z);
            Vector2 normalized = new Vector2(
                Mathf.InverseLerp(boundsData.worldMin.x, boundsData.worldMax.x, worldPos.x),
                Mathf.InverseLerp(boundsData.worldMin.y, boundsData.worldMax.y, worldPos.y)
            );

            Vector2 minimapSize = minimapRect.rect.size;
            Vector2 minimapPos = new Vector2(
                (normalized.x - 0.5f) * minimapSize.x,
                (normalized.y - 0.5f) * minimapSize.y
            );

            iconRect.anchoredPosition = minimapPos;

            float angle = -shipTransform.eulerAngles.y;
            iconRect.localRotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
