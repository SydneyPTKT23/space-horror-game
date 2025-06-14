using SLC.SpaceHorror.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Core
{
    public class NavigationMonitorManager : MonoBehaviour, IMonitorInteractable
    {
        [Header("References")]
        [SerializeField] private MinimapController minimap;
        [SerializeField] private UICursorWaypointSystem waypointSystem;

        public bool IsInteracting { get; private set; } = false;
        private UICursorWaypointSystem.InputMode lastInputMode = UICursorWaypointSystem.InputMode.MinimapControl;

        public void EnterInteraction()
        {
            if (IsInteracting) return;

            IsInteracting = true;

            // Restore last input mode to the waypoint system
            if (waypointSystem != null)
            {
                waypointSystem.SetInputMode(lastInputMode);
            }
            else
            {
                // fallback
                if (minimap != null) minimap.IsInteracting = true;
            }
        }

        public void ExitInteraction()
        {
            if (!IsInteracting) return;

            IsInteracting = false;

            if (waypointSystem != null)
            {
                // Save current mode before exiting
                lastInputMode = waypointSystem.CurrentInputMode;

                // Disable interaction on both systems
                waypointSystem.IsInteracting = false;
                if (minimap != null)
                    minimap.IsInteracting = false;
            }
            else
            {
                if (minimap != null) minimap.IsInteracting = false;
            }
        }
    }
}