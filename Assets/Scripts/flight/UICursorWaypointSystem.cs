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
        public RectTransform cursor;            // The moving cursor UI

        public MinimapBoundsData minimapBoundsData;
        public RectTransform canvasRect;        // The Canvas RectTransform
        public GameObject waypointPrefab;       // The prefab for a waypoint (UI Image)
        public GameObject worldWaypointPrefab;  // Prefab for the 3D world waypoint marker
        public UILineRenderer uiLineRenderer;   // The UI Line Renderer

        [Header("Settings")]
        public float cursorMoveSpeed = 300f;
        public float removeDistance = 20f;      // Distance threshold to remove waypoint on press

        private List<RectTransform> waypointUIList = new List<RectTransform>();
        private List<Vector3> waypointWorldList = new List<Vector3>();
        private List<GameObject> waypointWorldObjects = new List<GameObject>();

        void Update()
        {
            HandleCursorMovement();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                // Try to remove a nearby waypoint first
                int closeIndex = FindClosestWaypointIndex(cursor.anchoredPosition, removeDistance);
                if (closeIndex >= 0)
                {
                    RemoveWaypointAt(closeIndex);
                }
                else
                {
                    // If no waypoint close, place a new one
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

            HighlightClosestWaypoint(cursor.anchoredPosition, removeDistance);
            UpdateUILineRenderer();

            // Optionally, update world waypoint positions if cursor can move them dynamically
            // (If waypoints are static after placement, you can remove this)
            UpdateWorldWaypointPositions();
        }

        void HandleCursorMovement()
        {
            float x = UnityEngine.Input.GetAxisRaw("Horizontal");
            float y = UnityEngine.Input.GetAxisRaw("Vertical");

            Vector2 delta = new Vector2(x, y) * cursorMoveSpeed * Time.deltaTime;
            Vector2 newPos = cursor.anchoredPosition + delta;

            Vector2 min = canvasRect.rect.min;
            Vector2 max = canvasRect.rect.max;

            newPos.x = Mathf.Clamp(newPos.x, min.x, max.x);
            newPos.y = Mathf.Clamp(newPos.y, min.y, max.y);

            cursor.anchoredPosition = newPos;
        }

        void PlaceWaypoint()
        {
            // Spawn UI waypoint
            GameObject wp = Instantiate(waypointPrefab, canvasRect);
            RectTransform wpRect = wp.GetComponent<RectTransform>();
            wpRect.anchoredPosition = cursor.anchoredPosition;
            waypointUIList.Add(wpRect);

            // Calculate world position
            Vector3 worldPos = UIToWorldPosition(wpRect.anchoredPosition);
            waypointWorldList.Add(worldPos);

            // Spawn 3D world waypoint object
            if (worldWaypointPrefab != null)
            {
                GameObject worldWP = Instantiate(worldWaypointPrefab, worldPos, Quaternion.identity);
                waypointWorldObjects.Add(worldWP);
            }
            else
            {
                waypointWorldObjects.Add(null); // keep indexes aligned
            }

            // Set waypoint number text on UI waypoint
            TextMeshProUGUI numberText = wp.GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.text = waypointUIList.Count.ToString();
            }
        }

        void HighlightClosestWaypoint(Vector2 pos, float maxDistance)
        {
            for (int i = 0; i < waypointUIList.Count; i++)
            {
                float dist = Vector2.Distance(pos, waypointUIList[i].anchoredPosition);
                Image img = waypointUIList[i].GetComponent<Image>();
                if (img != null)
                {
                    img.color = dist < maxDistance ? Color.red : Color.white;
                }
            }
        }

        void RemoveWaypointAt(int index)
        {
            if (index >= 0 && index < waypointUIList.Count)
            {
                Destroy(waypointUIList[index].gameObject);
                waypointUIList.RemoveAt(index);

                waypointWorldList.RemoveAt(index);

                if (waypointWorldObjects[index] != null)
                    Destroy(waypointWorldObjects[index]);
                waypointWorldObjects.RemoveAt(index);

                RenumberWaypoints();
            }
        }

        void RemoveLastWaypoint()
        {
            int lastIndex = waypointUIList.Count - 1;
            if (lastIndex >= 0)
            {
                Destroy(waypointUIList[lastIndex].gameObject);
                waypointUIList.RemoveAt(lastIndex);

                waypointWorldList.RemoveAt(lastIndex);

                if (waypointWorldObjects[lastIndex] != null)
                    Destroy(waypointWorldObjects[lastIndex]);
                waypointWorldObjects.RemoveAt(lastIndex);

                RenumberWaypoints();
            }
        }

        void ClearAllWaypoints()
        {
            foreach (var wp in waypointUIList)
                Destroy(wp.gameObject);
            waypointUIList.Clear();

            waypointWorldList.Clear();

            foreach (var wp in waypointWorldObjects)
                if (wp != null) Destroy(wp);
            waypointWorldObjects.Clear();
        }

        int FindClosestWaypointIndex(Vector2 pos, float maxDistance)
        {
            int closestIndex = -1;
            float closestDist = maxDistance;

            for (int i = 0; i < waypointUIList.Count; i++)
            {
                float dist = Vector2.Distance(pos, waypointUIList[i].anchoredPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        void RenumberWaypoints()
        {
            for (int i = 0; i < waypointUIList.Count; i++)
            {
                TextMeshProUGUI numberText = waypointUIList[i].GetComponentInChildren<TextMeshProUGUI>();
                if (numberText != null)
                {
                    numberText.text = (i + 1).ToString();
                }
            }
        }

        void UpdateUILineRenderer()
        {
            if (waypointUIList.Count < 2)
            {
                uiLineRenderer.Points = new Vector2[0];
                return;
            }

            Vector2[] points = new Vector2[waypointUIList.Count];
            for (int i = 0; i < waypointUIList.Count; i++)
            {
                points[i] = waypointUIList[i].anchoredPosition;
            }

            uiLineRenderer.Points = points;
            uiLineRenderer.SetAllDirty();
        }

        Vector3 UIToWorldPosition(Vector2 uiPos)
        {
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            // Normalize UI pos from (-canvasWidth/2, canvasWidth/2) to (0,1)
            Vector2 normalized = new Vector2(
                (uiPos.x + canvasWidth / 2f) / canvasWidth,
                (uiPos.y + canvasHeight / 2f) / canvasHeight
            );

            // Map normalized 0-1 to worldMin - worldMax
            float worldX = Mathf.Lerp(minimapBoundsData.worldMin.x, minimapBoundsData.worldMax.x, normalized.x);
            float worldZ = Mathf.Lerp(minimapBoundsData.worldMin.y, minimapBoundsData.worldMax.y, normalized.y);

            return new Vector3(worldX, 0f, worldZ);
        }

        Vector2 WorldToUIPosition(Vector3 worldPos)
        {
            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            float normalizedX = Mathf.InverseLerp(minimapBoundsData.worldMin.x, minimapBoundsData.worldMax.x, worldPos.x);
            float normalizedY = Mathf.InverseLerp(minimapBoundsData.worldMin.y, minimapBoundsData.worldMax.y, worldPos.z);

            float uiX = normalizedX * canvasWidth - canvasWidth / 2f;
            float uiY = normalizedY * canvasHeight - canvasHeight / 2f;

            return new Vector2(uiX, uiY);
        }

        public IReadOnlyList<Vector3> GetWorldWaypoints()
        {
            return waypointWorldList.AsReadOnly();
        }

        void UpdateWorldWaypointPositions()
        {
            // Optional: if UI waypoints can move after placement,
            // keep world waypoint objects synced with UI waypoints
            for (int i = 0; i < waypointUIList.Count; i++)
            {
                if (waypointWorldObjects[i] != null)
                {
                    Vector3 newWorldPos = UIToWorldPosition(waypointUIList[i].anchoredPosition);
                    waypointWorldObjects[i].transform.position = newWorldPos;
                    waypointWorldList[i] = newWorldPos;
                }
            }
        }
    }
}