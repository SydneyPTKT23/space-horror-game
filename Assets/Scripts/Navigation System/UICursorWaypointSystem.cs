using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace SLC.SpaceHorror
{
    public class UICursorWaypointSystem : MonoBehaviour
    {
        [Header("References")]
        public RectTransform cursor;
        public MinimapController minimapController;
        public GameObject waypointPrefab;
        public GameObject worldWaypointPrefab;
        public UILineRenderer uiLineRenderer;

        [Header("Settings")]
        public float removeDistance = 20f;
        public bool IsInteracting { get; set; }

        private class WaypointData
        {
            public RectTransform uiRect;
            public TextMeshProUGUI numberText;
            public Image image;
            public GameObject worldObject;
            public Vector3 worldPosition;
        }

        private readonly List<WaypointData> waypoints = new();
        private readonly List<Vector3> cachedWorldPositions = new();
        private bool uiLineDirty;

        private void Update()
        {
            if (!IsInteracting) return;

            Vector2 cursorLocal = GetCursorLocalMapPosition();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                int closeIndex = FindClosestWaypointIndex(cursorLocal, removeDistance);
                if (closeIndex >= 0)
                    RemoveWaypointAt(closeIndex);
                else
                    PlaceWaypoint(cursorLocal);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.C)) ClearAllWaypoints();

            HighlightClosestWaypoint(cursorLocal, removeDistance);
            UpdateUILineRenderer();
            UpdateWorldWaypointPositions();
        }

        private Vector2 GetCursorLocalMapPosition()
        {
            _ = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                minimapController.mapContent,
                cursor.position,
                null,
                out Vector2 localPoint
            );
            return localPoint;
        }

        private void PlaceWaypoint(Vector2 localCursorPos)
        {
            GameObject wp = Instantiate(waypointPrefab, minimapController.mapContent);
            RectTransform wpRect = wp.GetComponent<RectTransform>();
            wpRect.anchoredPosition = localCursorPos;

            Vector3 worldPos = minimapController.MapToWorld(localCursorPos);
            GameObject worldWP = worldWaypointPrefab != null
                ? Instantiate(worldWaypointPrefab, worldPos, Quaternion.identity)
                : null;

            WaypointData data = new()
            {
                uiRect = wpRect,
                numberText = wp.GetComponentInChildren<TextMeshProUGUI>(),
                image = wp.GetComponent<Image>(),
                worldObject = worldWP,
                worldPosition = worldPos
            };

            waypoints.Add(data);
            RenumberWaypoints();
            uiLineDirty = true;
        }

        private void HighlightClosestWaypoint(Vector2 cursorLocalPos, float maxDistance)
        {
            float maxDistSqr = maxDistance * maxDistance;

            foreach (WaypointData wp in waypoints)
            {
                if (wp.image != null)
                {
                    float distSqr = (cursorLocalPos - wp.uiRect.anchoredPosition).sqrMagnitude;
                    wp.image.color = distSqr < maxDistSqr ? Color.red : Color.white;
                }
            }
        }

        private void RemoveWaypointAt(int index)
        {
            if (index < 0 || index >= waypoints.Count) return;

            Destroy(waypoints[index].uiRect.gameObject);
            if (waypoints[index].worldObject != null)
                Destroy(waypoints[index].worldObject);

            waypoints.RemoveAt(index);
            RenumberWaypoints();
            uiLineDirty = true;
        }

        private void ClearAllWaypoints()
        {
            foreach (WaypointData wp in waypoints)
            {
                Destroy(wp.uiRect.gameObject);
                if (wp.worldObject != null)
                    Destroy(wp.worldObject);
            }

            waypoints.Clear();
            uiLineDirty = true;
        }

        private int FindClosestWaypointIndex(Vector2 pos, float maxDistance)
        {
            float closestDistSqr = maxDistance * maxDistance;
            int closestIndex = -1;

            for (int i = 0; i < waypoints.Count; i++)
            {
                float distSqr = (pos - waypoints[i].uiRect.anchoredPosition).sqrMagnitude;
                if (distSqr < closestDistSqr)
                {
                    closestDistSqr = distSqr;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void RenumberWaypoints()
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i].numberText != null)
                    waypoints[i].numberText.text = (i + 1).ToString();
            }
        }

        private void UpdateUILineRenderer()
        {
            if (!uiLineDirty) return;

            if (waypoints.Count < 2)
            {
                uiLineRenderer.Points = System.Array.Empty<Vector2>();
            }
            else
            {
                Vector2[] points = new Vector2[waypoints.Count];
                for (int i = 0; i < waypoints.Count; i++)
                    points[i] = waypoints[i].uiRect.anchoredPosition;

                uiLineRenderer.Points = points;
            }

            uiLineRenderer.SetAllDirty();
            uiLineDirty = false;
        }

        private void UpdateWorldWaypointPositions()
        {
            foreach (WaypointData wp in waypoints)
            {
                if (wp.worldObject == null) continue;

                Vector3 newPos = minimapController.MapToWorld(wp.uiRect.anchoredPosition);
                wp.worldObject.transform.position = newPos;
                wp.worldPosition = newPos;
            }
        }

        public IReadOnlyList<Vector3> GetWorldWaypoints()
        {
            cachedWorldPositions.Clear();
            foreach (WaypointData wp in waypoints)
                cachedWorldPositions.Add(wp.worldPosition);

            return cachedWorldPositions;
        }
    }
}