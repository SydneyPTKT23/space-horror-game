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

    private UICursorWaypointSystem.InputMode lastInputMode = UICursorWaypointSystem.InputMode.MinimapControl;

    public bool IsInteracting { get; private set; } = false;
    public bool IsInButtonMode => CurrentInputMode == UICursorWaypointSystem.InputMode.UIButtonControl;

    public UICursorWaypointSystem.InputMode CurrentInputMode { get; private set; }

    private void Update()
    {
        if (IsInteracting && waypointSystem != null)
            CurrentInputMode = waypointSystem.CurrentInputMode;
    }

    public void EnterInteraction()
    {
        if (IsInteracting) return;

        IsInteracting = true;

        // Use last known input mode, or fall back to default if unset
        UICursorWaypointSystem.InputMode inputModeToUse = lastInputMode;

        if (waypointSystem != null)
        {
            waypointSystem.SetInputMode(inputModeToUse);
            CurrentInputMode = inputModeToUse;
        }

        if (minimap != null)
            minimap.IsInteracting = inputModeToUse == UICursorWaypointSystem.InputMode.MinimapControl;
    }

    public void ExitInteraction()
    {
        if (!IsInteracting) return;

        IsInteracting = false;

        // Save current input mode to restore it later
        if (waypointSystem != null)
            lastInputMode = waypointSystem.CurrentInputMode;

        if (minimap != null)
            minimap.IsInteracting = false;
    }

    public void ResetToDefaultView()
    {
        CurrentInputMode = defaultInputMode;
        lastInputMode = defaultInputMode;
    }
}