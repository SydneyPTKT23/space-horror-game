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
        public RectTransform cursor;                        // Cursor under minimapContainer
        public MinimapController minimapController;         // Reference to your map controller
        public GameObject waypointPrefab;
        public GameObject worldWaypointPrefab;
        public UILineRenderer uiLineRenderer;

        [Header("Settings")]
        public float removeDistance = 20.0f;

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

        private bool uiLineDirty = false;

        private void Update()
        {
            Vector2 cursorLocalPos = GetCursorLocalMapPosition();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                int closeIndex = FindClosestWaypointIndex(cursorLocalPos, removeDistance);
                if (closeIndex >= 0)
                {
                    RemoveWaypointAt(closeIndex);
                }
                else
                {
                    PlaceWaypoint(cursorLocalPos);
                }
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.C))
            {
                ClearAllWaypoints();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Backspace))
            {
                RemoveLastWaypoint();
            }

            HighlightClosestWaypoint(cursorLocalPos, removeDistance);
            UpdateUILineRenderer();
            UpdateWorldWaypointPositions();
        }

        private Vector2 GetCursorLocalMapPosition()
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                minimapController.mapContent,
                cursor.position,
                null,
                out localPoint
            );
            return localPoint;
        }

        private void PlaceWaypoint(Vector2 localCursorPos)
        {
            GameObject wp = Instantiate(waypointPrefab, minimapController.mapContent);
            RectTransform wpRect = wp.GetComponent<RectTransform>();
            wpRect.anchoredPosition = localCursorPos;

            Vector3 worldPos = minimapController.MapToWorld(localCursorPos);

            GameObject worldWP = null;
            if (worldWaypointPrefab != null)
                worldWP = Instantiate(worldWaypointPrefab, worldPos, Quaternion.identity);

            TextMeshProUGUI numberText = wp.GetComponentInChildren<TextMeshProUGUI>();
            Image image = wp.GetComponent<Image>();

            WaypointData data = new()
            {
                uiRect = wpRect,
                numberText = numberText,
                image = image,
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

            foreach (var wp in waypoints)
            {
                float distSqr = (cursorLocalPos - wp.uiRect.anchoredPosition).sqrMagnitude;
                if (wp.image != null)
                {
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

        private void RemoveLastWaypoint()
        {
            int lastIndex = waypoints.Count - 1;
            if (lastIndex >= 0)
                RemoveWaypointAt(lastIndex);
        }

        private void ClearAllWaypoints()
        {
            foreach (var wp in waypoints)
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
            int closestIndex = -1;
            float closestDistSqr = maxDistance * maxDistance;

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
                {
                    waypoints[i].numberText.text = (i + 1).ToString();
                }
            }
        }

        private void UpdateUILineRenderer()
        {
            if (!uiLineDirty)
                return;

            if (waypoints.Count < 2)
            {
                uiLineRenderer.Points = new Vector2[0];
                uiLineRenderer.SetAllDirty();
                uiLineDirty = false;
                return;
            }

            Vector2[] points = new Vector2[waypoints.Count];
            for (int i = 0; i < waypoints.Count; i++)
            {
                points[i] = waypoints[i].uiRect.anchoredPosition;
            }

            uiLineRenderer.Points = points;
            uiLineRenderer.SetAllDirty();
            uiLineDirty = false;
        }

        private void UpdateWorldWaypointPositions()
        {
            foreach (var wp in waypoints)
            {
                if (wp.worldObject != null)
                {
                    Vector3 newWorldPos = minimapController.MapToWorld(wp.uiRect.anchoredPosition);
                    wp.worldObject.transform.position = newWorldPos;
                    wp.worldPosition = newWorldPos;
                }
            }
        }

        public IReadOnlyList<Vector3> GetWorldWaypoints()
        {
            cachedWorldPositions.Clear();
            foreach (var wp in waypoints)
                cachedWorldPositions.Add(wp.worldPosition);

            return cachedWorldPositions;
        }
    }
}