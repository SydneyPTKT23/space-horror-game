using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MonitorUIRaycaster : MonoBehaviour
{
    [SerializeField] private Camera uiCamera; // Camera rendering the UI to RenderTexture
    [SerializeField] private Canvas uiCanvas; // Canvas rendering to RenderTexture
    [SerializeField] private RenderTexture renderTexture; // Target texture of the UI camera

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    Vector2 localHit = GetLocalHit(hit.textureCoord);
                    PointerEventData pointerData = new PointerEventData(EventSystem.current)
                    {
                        position = localHit
                    };

                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointerData, results);

                    foreach (RaycastResult result in results)
                    {
                        ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                    }
                }
            }
        }
    }

    private Vector2 GetLocalHit(Vector2 textureCoord)
    {
        // Convert from normalized UV (0–1) to RenderTexture pixel coordinates
        return new Vector2(textureCoord.x * renderTexture.width, textureCoord.y * renderTexture.height);
    }
}