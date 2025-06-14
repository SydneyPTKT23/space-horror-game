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

        private void Update()
        {
            if (IsInteracting && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ExitInteraction();
            }
        }

        public void EnterInteraction()
        {
            if (IsInteracting) return;

            IsInteracting = true;

            if (minimap != null) minimap.IsInteracting = true;
            if (waypointSystem != null) waypointSystem.IsInteracting = true;
        }

        public void ExitInteraction()
        {
            if (!IsInteracting) return;

            IsInteracting = false;

            if (minimap != null) minimap.IsInteracting = false;
            if (waypointSystem != null) waypointSystem.IsInteracting = false;
        }
    }
}