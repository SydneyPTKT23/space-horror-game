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
        public MinimapBoundsData minimapBoundsData;
        public RectTransform canvasRect;
        public GameObject waypointPrefab;
        public GameObject worldWaypointPrefab;
        public UILineRenderer uiLineRenderer;

        [Header("Settings")]
        public float cursorMoveSpeed = 300.0f;
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

        private float canvasWidth;
        private float canvasHeight;

        private bool uiLineDirty = false;

        private void Start()
        {
            canvasWidth = canvasRect.rect.width;
            canvasHeight = canvasRect.rect.height;
        }

        private void Update()
        {
            Vector2 cursorPos = cursor.anchoredPosition;

            HandleCursorMovement();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                int closeIndex = FindClosestWaypointIndex(cursorPos, removeDistance);
                if (closeIndex >= 0)
                {
                    RemoveWaypointAt(closeIndex);
                }
                else
                {
                    PlaceWaypoint();
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

            HighlightClosestWaypoint(cursorPos, removeDistance);
            UpdateUILineRenderer();

            UpdateWorldWaypointPositions();
        }

        private void HandleCursorMovement()
        {
            float x = UnityEngine.Input.GetAxisRaw("Horizontal");
            float y = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector2 delta = cursorMoveSpeed * Time.deltaTime * new Vector2(x, y);
            Vector2 newPos = cursor.anchoredPosition + delta;

            Vector2 min = canvasRect.rect.min;
            Vector2 max = canvasRect.rect.max;

            newPos.x = Mathf.Clamp(newPos.x, min.x, max.x);
            newPos.y = Mathf.Clamp(newPos.y, min.y, max.y);

            cursor.anchoredPosition = newPos;
        }

        private void PlaceWaypoint()
        {
            GameObject wp = Instantiate(waypointPrefab, canvasRect);
            RectTransform wpRect = wp.GetComponent<RectTransform>();
            wpRect.anchoredPosition = cursor.anchoredPosition;

            Vector3 worldPos = UIToWorldPosition(wpRect.anchoredPosition);

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

        private void HighlightClosestWaypoint(Vector2 pos, float maxDistance)
        {
            float maxDistSqr = maxDistance * maxDistance;

            for (int i = 0; i < waypoints.Count; i++)
            {
                float distSqr = (pos - waypoints[i].uiRect.anchoredPosition).sqrMagnitude;
                if (waypoints[i].image != null)
                {
                    waypoints[i].image.color = distSqr < maxDistSqr ? Color.red : Color.white;
                }
            }
        }

        private void RemoveWaypointAt(int index)
        {
            if (index >= 0 && index < waypoints.Count)
            {
                Destroy(waypoints[index].uiRect.gameObject);

                if (waypoints[index].worldObject != null)
                    Destroy(waypoints[index].worldObject);

                waypoints.RemoveAt(index);

                RenumberWaypoints();
                uiLineDirty = true;
            }
        }

        private void RemoveLastWaypoint()
        {
            int lastIndex = waypoints.Count - 1;
            if (lastIndex >= 0)
            {
                Destroy(waypoints[lastIndex].uiRect.gameObject);

                if (waypoints[lastIndex].worldObject != null)
                    Destroy(waypoints[lastIndex].worldObject);

                waypoints.RemoveAt(lastIndex);

                RenumberWaypoints();
                uiLineDirty = true;
            }
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

        private Vector3 UIToWorldPosition(Vector2 uiPos)
        {
            Vector2 normalized = new(
                (uiPos.x + canvasWidth / 2.0f) / canvasWidth,
                (uiPos.y + canvasHeight / 2.0f) / canvasHeight
            );

            float worldX = Mathf.Lerp(minimapBoundsData.worldMin.x, minimapBoundsData.worldMax.x, normalized.x);
            float worldZ = Mathf.Lerp(minimapBoundsData.worldMin.y, minimapBoundsData.worldMax.y, normalized.y);

            return new Vector3(worldX, 0f, worldZ);
        }

        private Vector2 WorldToUIPosition(Vector3 worldPos)
        {
            float normalizedX = Mathf.InverseLerp(minimapBoundsData.worldMin.x, minimapBoundsData.worldMax.x, worldPos.x);
            float normalizedY = Mathf.InverseLerp(minimapBoundsData.worldMin.y, minimapBoundsData.worldMax.y, worldPos.z);

            float uiX = normalizedX * canvasWidth - canvasWidth / 2.0f;
            float uiY = normalizedY * canvasHeight - canvasHeight / 2.0f;

            return new Vector2(uiX, uiY);
        }

        public IReadOnlyList<Vector3> GetWorldWaypoints()
        {
            cachedWorldPositions.Clear();
            for (int i = 0; i < waypoints.Count; i++)
                cachedWorldPositions.Add(waypoints[i].worldPosition);

            return cachedWorldPositions;
        }

        private void UpdateWorldWaypointPositions()
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                if (waypoints[i].worldObject != null)
                {
                    Vector3 newWorldPos = UIToWorldPosition(waypoints[i].uiRect.anchoredPosition);
                    waypoints[i].worldObject.transform.position = newWorldPos;
                    waypoints[i].worldPosition = newWorldPos;
                }
            }
        }
    }
}