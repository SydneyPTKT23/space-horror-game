using SLC.SpaceHorror;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour, IScrollHandler, IPointerClickHandler
{
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

    void Update()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input != Vector2.zero)
        {
            Pan(input.normalized);
        }
        else
        {
            Pan(Vector2.zero);
        }
    }

    void Pan(Vector2 input)
    {
        Vector2 delta = input * panSpeed * Time.deltaTime;

        float maxX = (mapContent.rect.width * currentScale.x * 0.5f) - (containerRect.rect.width * 0.5f);
        float maxY = (mapContent.rect.height * currentScale.y * 0.5f) - (containerRect.rect.height * 0.5f);
        maxX = Mathf.Max(0, maxX);
        maxY = Mathf.Max(0, maxY);

        Vector2 mapPos = mapContent.anchoredPosition;
        Vector2 cursorPos = cursor.anchoredPosition;
        Vector2 cursorRange = (containerRect.rect.size - cursor.rect.size) * 0.5f;

        // X Axis
        if (input.x != 0)
        {
            if ((input.x < 0 && mapPos.x < maxX) || (input.x > 0 && mapPos.x > -maxX))
            {
                if ((input.x < 0 && cursorPos.x > 0) || (input.x > 0 && cursorPos.x < 0))
                {
                    cursorPos.x = Mathf.MoveTowards(cursorPos.x, 0, Mathf.Abs(delta.x));
                }
                else
                {
                    mapPos.x = Mathf.Clamp(mapPos.x - delta.x, -maxX, maxX);
                    cursorPos.x = 0;
                }
            }
            else
            {
                cursorPos.x = Mathf.Clamp(cursorPos.x + delta.x, -cursorRange.x, cursorRange.x);
            }
        }

        // Y Axis
        if (input.y != 0)
        {
            if ((input.y < 0 && mapPos.y < maxY) || (input.y > 0 && mapPos.y > -maxY))
            {
                if ((input.y < 0 && cursorPos.y > 0) || (input.y > 0 && cursorPos.y < 0))
                {
                    cursorPos.y = Mathf.MoveTowards(cursorPos.y, 0, Mathf.Abs(delta.y));
                }
                else
                {
                    mapPos.y = Mathf.Clamp(mapPos.y - delta.y, -maxY, maxY);
                    cursorPos.y = 0;
                }
            }
            else
            {
                cursorPos.y = Mathf.Clamp(cursorPos.y + delta.y, -cursorRange.y, cursorRange.y);
            }
        }

        mapContent.anchoredPosition = mapPos;
        cursor.anchoredPosition = cursorPos;
    }

    public void OnScroll(PointerEventData eventData)
    {
        float delta = eventData.scrollDelta.y * zoomSpeed;
        Vector2 newScale = currentScale + new Vector2(delta, delta);
        newScale = Vector2.Max(Vector2.one * minZoom, Vector2.Min(Vector2.one * maxZoom, newScale));
        mapContent.localScale = newScale;
        currentScale = newScale;

        AdjustCursorForZoom();
        ClampMapPosition();

        Pan(Vector2.zero);
    }

    void AdjustCursorForZoom()
    {
        Vector2 cursorRangeOld = (containerRect.rect.size - cursor.rect.size) * 0.5f;

        Vector2 normalizedCursorPos = new Vector2(
            cursor.anchoredPosition.x / cursorRangeOld.x,
            cursor.anchoredPosition.y / cursorRangeOld.y
        );

        normalizedCursorPos.x = Mathf.Clamp(normalizedCursorPos.x, -1f, 1f);
        normalizedCursorPos.y = Mathf.Clamp(normalizedCursorPos.y, -1f, 1f);

        Vector2 cursorRangeNew = (containerRect.rect.size - cursor.rect.size) * 0.5f;

        cursor.anchoredPosition = new Vector2(
            normalizedCursorPos.x * cursorRangeNew.x,
            normalizedCursorPos.y * cursorRangeNew.y
        );
    }

    public Vector3 MapToWorld(Vector2 mapLocalPos)
    {
        Vector2 normalized = new Vector2(
            (mapLocalPos.x / mapContent.rect.width) + 0.5f,
            (mapLocalPos.y / mapContent.rect.height) + 0.5f
        );

        float worldX = Mathf.Lerp(bounds.worldMin.x, bounds.worldMax.x, normalized.x);
        float worldZ = Mathf.Lerp(bounds.worldMin.y, bounds.worldMax.y, normalized.y);
        return new Vector3(worldX, 0, worldZ);
    }

    void ClampMapPosition()
    {
        float maxX = (mapContent.rect.width * currentScale.x * 0.5f) - (containerRect.rect.width * 0.5f);
        float maxY = (mapContent.rect.height * currentScale.y * 0.5f) - (containerRect.rect.height * 0.5f);
        maxX = Mathf.Max(0, maxX);
        maxY = Mathf.Max(0, maxY);

        Vector2 mapPos = mapContent.anchoredPosition;
        mapPos.x = Mathf.Clamp(mapPos.x, -maxX, maxX);
        mapPos.y = Mathf.Clamp(mapPos.y, -maxY, maxY);

        mapContent.anchoredPosition = mapPos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("MinimapContainer clicked at screen position: " + eventData.position);
    }
}
