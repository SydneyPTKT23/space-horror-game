using SLC.SpaceHorror.Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace SLC.SpaceHorror
{
    public class UICursorWaypointSystem : MonoBehaviour
    {
        public enum InputMode { MinimapControl, UIButtonControl }

        [Header("Data")]
        [SerializeField] private InputReader inputReader;

        [Header("References")]
        [SerializeField] private RectTransform cursor;
        [SerializeField] private MinimapController minimapController;
        [SerializeField] private GameObject waypointPrefab;
        [SerializeField] private GameObject worldWaypointPrefab;
        [SerializeField] private UILineRenderer uiLineRenderer;

        [Header("UI Button Navigation")]
        [SerializeField] private UI.UIButtonNavigationHandler buttonNavigationHandler;

        [Header("Settings")]
        [SerializeField] private float removeDistance = 20f;

        public bool IsInteracting { get; private set; }
        public InputMode CurrentInputMode { get; private set; } = InputMode.MinimapControl;

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

        private void Start() => SetInputMode(InputMode.MinimapControl);

        private void Update()
        {
            if (inputReader.MonitorTogglePressedThisFrame)
                ToggleInputMode();

            if (!IsInteracting && CurrentInputMode != InputMode.UIButtonControl)
                return;

            if (CurrentInputMode == InputMode.MinimapControl)
                HandleMinimapControl();
        }

        private void ToggleInputMode()
        {
            InputMode newMode = CurrentInputMode == InputMode.MinimapControl
                ? InputMode.UIButtonControl
                : InputMode.MinimapControl;

            SetInputMode(newMode);
        }

        public void SetInputMode(InputMode mode)
        {
            CurrentInputMode = mode;
            IsInteracting = (mode == InputMode.MinimapControl);

            if (minimapController != null)
                minimapController.IsInteracting = IsInteracting;

            if (buttonNavigationHandler != null)
            {
                buttonNavigationHandler.enabled = (mode == InputMode.UIButtonControl);

                if (buttonNavigationHandler.enabled)
                    buttonNavigationHandler.ActivateNavigation();
                else
                    buttonNavigationHandler.Deactivate();
            }
        }

        private void HandleMinimapControl()
        {
            Vector2 cursorLocal = GetCursorLocalMapPosition();

            if (inputReader.MonitorActionPressedThisFrame)
            {
                int index = FindClosestWaypointIndex(cursorLocal, removeDistance);
                if (index >= 0)
                    RemoveWaypointAt(index);
                else
                    PlaceWaypoint(cursorLocal);
            }

            HighlightClosestWaypoint(cursorLocal, removeDistance);
            UpdateUILineRenderer();
            UpdateWorldWaypointPositions();
        }

        private Vector2 GetCursorLocalMapPosition()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                minimapController.mapContent,
                cursor.position,
                null,
                out Vector2 localPoint
            );
            return localPoint;
        }

        private void PlaceWaypoint(Vector2 localCursorPos)
        {
            GameObject wpGO = Instantiate(waypointPrefab, minimapController.mapContent);
            RectTransform rect = wpGO.GetComponent<RectTransform>();
            rect.anchoredPosition = localCursorPos;

            Vector3 worldPos = minimapController.MapToWorld(localCursorPos);
            GameObject worldGO = worldWaypointPrefab != null
                ? Instantiate(worldWaypointPrefab, worldPos, Quaternion.identity)
                : null;

            waypoints.Add(new WaypointData
            {
                uiRect = rect,
                numberText = wpGO.GetComponentInChildren<TextMeshProUGUI>(),
                image = wpGO.GetComponent<Image>(),
                worldObject = worldGO,
                worldPosition = worldPos
            });

            RenumberWaypoints();
            uiLineDirty = true;
        }

        private void HighlightClosestWaypoint(Vector2 cursorLocal, float maxDistance)
        {
            float maxDistSqr = maxDistance * maxDistance;

            foreach (WaypointData wp in waypoints)
            {
                if (wp.image == null) continue;

                float distSqr = (cursorLocal - wp.uiRect.anchoredPosition).sqrMagnitude;
                wp.image.color = distSqr < maxDistSqr ? Color.red : Color.white;
            }
        }

        private int FindClosestWaypointIndex(Vector2 pos, float maxDistance)
        {
            float closestSqr = maxDistance * maxDistance;
            int closestIndex = -1;

            for (int i = 0; i < waypoints.Count; i++)
            {
                float distSqr = (pos - waypoints[i].uiRect.anchoredPosition).sqrMagnitude;
                if (distSqr < closestSqr)
                {
                    closestSqr = distSqr;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private void RemoveWaypointAt(int index)
        {
            if (index < 0 || index >= waypoints.Count) return;

            WaypointData wp = waypoints[index];
            if (wp.uiRect != null)
                Destroy(wp.uiRect.gameObject);
            if (wp.worldObject != null)
                Destroy(wp.worldObject);

            waypoints.RemoveAt(index);
            RenumberWaypoints();
            uiLineDirty = true;
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
            foreach (var wp in waypoints)
                cachedWorldPositions.Add(wp.worldPosition);

            return cachedWorldPositions;
        }

        public void ClearAllWaypoints()
        {
            foreach (var wp in waypoints)
            {
                if (wp.uiRect != null)
                    Destroy(wp.uiRect.gameObject);
                if (wp.worldObject != null)
                    Destroy(wp.worldObject);
            }

            waypoints.Clear();
            uiLineDirty = true;
        }
    }
}