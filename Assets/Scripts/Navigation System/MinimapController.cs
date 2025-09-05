using SLC.SpaceHorror;
using SLC.SpaceHorror.Input;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SLC
{
    public class MinimapController : MonoBehaviour, IScrollHandler, IPointerClickHandler
    {
        [Header("Data")]
        [SerializeField] private InputReader inputReader;

        [Header("References")]
        public RectTransform mapContent;
        public RectTransform containerRect;
        public RectTransform cursor;
        public MinimapBoundsData bounds;

        [Header("Zoom Settings")]
        public float minZoom = 0.5f;
        public float maxZoom = 2f;
        public float zoomSpeed = 0.1f;

        [Header("Pan Settings")]
        public float panSpeed = 500f;

        private Vector2 currentScale = Vector2.one;
        private float maxX, maxY;

        public bool IsInteracting { get; set; }

        // Store zoom input from InputReader (e.g. gamepad triggers or mouse scroll axes)
        private Vector2 monitorZoomDelta;

        private void Start()
        {
            UpdateBounds();
        }

        private void Update()
        {
            if (!IsInteracting || inputReader == null) return;

            Vector2 input = inputReader.NavigateInput;
            if (input != Vector2.zero)
                ApplyPanInput(input.normalized);

            monitorZoomDelta = inputReader.MonitorZoomDelta; // Assumes public getter from InputReader
            float zoomDelta = monitorZoomDelta.y * zoomSpeed;

            if (Mathf.Abs(zoomDelta) > 0.0001f)
            {
                ApplyZoom(zoomDelta);
            }
        }

        private void ApplyZoom(float delta)
        {
            Vector2 newScale = currentScale + Vector2.one * delta;
            newScale = Vector2.Max(Vector2.one * minZoom, Vector2.Min(Vector2.one * maxZoom, newScale));

            currentScale = mapContent.localScale = newScale;

            UpdateBounds();
            AdjustCursorForZoom();
            ClampMapPosition();
        }

        private void ApplyPanInput(Vector2 input)
        {
            Vector2 delta = panSpeed * Time.deltaTime * input;

            Vector2 mapPos = mapContent.anchoredPosition;
            Vector2 cursorPos = cursor.anchoredPosition;
            Vector2 cursorRange = (containerRect.rect.size - cursor.rect.size) * 0.5f;

            ApplyPanAxis(ref mapPos.x, ref cursorPos.x, delta.x, maxX, cursorRange.x, input.x);
            ApplyPanAxis(ref mapPos.y, ref cursorPos.y, delta.y, maxY, cursorRange.y, input.y);

            mapContent.anchoredPosition = mapPos;
            cursor.anchoredPosition = cursorPos;
        }

        private void ApplyPanAxis(ref float mapAxis, ref float cursorAxis, float delta, float maxAxis, float cursorRange, float inputAxis)
        {
            if (inputAxis == 0) return;

            bool mapWithin = (inputAxis < 0 && mapAxis < maxAxis) || (inputAxis > 0 && mapAxis > -maxAxis);
            bool cursorNeedsSnap = (inputAxis < 0 && cursorAxis > 0) || (inputAxis > 0 && cursorAxis < 0);

            if (mapWithin)
            {
                if (cursorNeedsSnap)
                    cursorAxis = Mathf.MoveTowards(cursorAxis, 0, Mathf.Abs(delta));
                else
                {
                    mapAxis = Mathf.Clamp(mapAxis - delta, -maxAxis, maxAxis);
                    cursorAxis = 0;
                }
            }
            else
            {
                cursorAxis = Mathf.Clamp(cursorAxis + delta, -cursorRange, cursorRange);
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            float delta = eventData.scrollDelta.y * zoomSpeed;
            ApplyZoom(delta);
        }

        private void UpdateBounds()
        {
            maxX = Mathf.Max(0f, (mapContent.rect.width * currentScale.x * 0.5f) - (containerRect.rect.width * 0.5f));
            maxY = Mathf.Max(0f, (mapContent.rect.height * currentScale.y * 0.5f) - (containerRect.rect.height * 0.5f));
        }

        private void AdjustCursorForZoom()
        {
            Vector2 oldRange = (containerRect.rect.size - cursor.rect.size) * 0.5f;
            Vector2 normalizedCursorPos = new(
                Mathf.Clamp(cursor.anchoredPosition.x / oldRange.x, -1f, 1f),
                Mathf.Clamp(cursor.anchoredPosition.y / oldRange.y, -1f, 1f)
            );

            Vector2 newRange = (containerRect.rect.size - cursor.rect.size) * 0.5f;
            cursor.anchoredPosition = new Vector2(
                normalizedCursorPos.x * newRange.x,
                normalizedCursorPos.y * newRange.y
            );
        }

        private void ClampMapPosition()
        {
            Vector2 pos = mapContent.anchoredPosition;
            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
            mapContent.anchoredPosition = pos;
        }

        public Vector3 MapToWorld(Vector2 mapLocalPos)
        {
            Vector2 normalized = new(
                (mapLocalPos.x / mapContent.rect.width) + 0.5f,
                (mapLocalPos.y / mapContent.rect.height) + 0.5f
            );

            float worldX = Mathf.Lerp(bounds.worldMin.x, bounds.worldMax.x, normalized.x);
            float worldZ = Mathf.Lerp(bounds.worldMin.y, bounds.worldMax.y, normalized.y);

            return new Vector3(worldX, 0f, worldZ);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_EDITOR
            Debug.Log($"MinimapContainer clicked at screen position: {eventData.position}");
#endif
        }
    }
}