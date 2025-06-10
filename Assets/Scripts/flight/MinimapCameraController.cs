using UnityEngine;
using UnityEngine.EventSystems;

public class MinimapCameraController : MonoBehaviour, IScrollHandler, IPointerClickHandler
{
    [Header("References")]
    public RectTransform mapContent;
    public RectTransform containerRect;
    public RectTransform cursor;

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
    }

    void Pan(Vector2 input)
    {
        Vector2 delta = input * panSpeed * Time.deltaTime;

        float maxX = (mapContent.rect.width * 0.5f) - (containerRect.rect.width * 0.5f);
        float maxY = (mapContent.rect.height * 0.5f) - (containerRect.rect.height * 0.5f);
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
                // If cursor is not centered in opposite direction, move cursor first
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
        else
        {
            cursorPos.x = Mathf.MoveTowards(cursorPos.x, 0, panSpeed * Time.deltaTime);
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
        else
        {
            cursorPos.y = Mathf.MoveTowards(cursorPos.y, 0, panSpeed * Time.deltaTime);
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

        Pan(Vector2.zero);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("MinimapContainer clicked at screen position: " + eventData.position);
    }
}
