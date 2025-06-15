using SLC.SpaceHorror.Core;
using SLC.SpaceHorror;
using SLC;
using UnityEngine;

public class NavigationMonitorManager : MonoBehaviour, IMonitorHandler
{
    [Header("References")]
    [SerializeField] private MinimapController minimap;
    [SerializeField] private UICursorWaypointSystem waypointSystem;
    [SerializeField] private UICursorWaypointSystem.InputMode defaultInputMode = UICursorWaypointSystem.InputMode.MinimapControl;

    public bool IsInteracting { get; private set; } = false;
    public bool IsInButtonMode => CurrentInputMode == UICursorWaypointSystem.InputMode.UIButtonControl;

    public UICursorWaypointSystem.InputMode CurrentInputMode { get; private set; }
    private bool hasInteractedBefore = false;

    public void EnterInteraction()
    {
        if (IsInteracting) return;

        IsInteracting = true;

        // Use the last mode or default
        UICursorWaypointSystem.InputMode inputModeToUse = hasInteractedBefore ? CurrentInputMode : defaultInputMode;

        if (waypointSystem != null)
        {
            waypointSystem.SetInputMode(inputModeToUse);
        }

        if (minimap != null)
        {
            minimap.IsInteracting = inputModeToUse == UICursorWaypointSystem.InputMode.MinimapControl;
        }

        hasInteractedBefore = true;
    }

    public void ExitInteraction()
    {
        if (!IsInteracting) return;

        IsInteracting = false;

        if (waypointSystem != null)
        {
            CurrentInputMode = waypointSystem.CurrentInputMode;

            waypointSystem.SetInputMode(UICursorWaypointSystem.InputMode.UIButtonControl);
        }

        if (minimap != null)
        {
            minimap.IsInteracting = false;
        }
    }

    public void ResetToDefaultView()
    {
        CurrentInputMode = defaultInputMode;
        hasInteractedBefore = false;
    }
}