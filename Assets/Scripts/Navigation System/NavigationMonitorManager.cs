using SLC.SpaceHorror.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Core
{
    public class NavigationMonitorManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MonitorViewController viewController;
        [SerializeField] private MinimapController minimap;
        [SerializeField] private UICursorWaypointSystem waypointSystem;

        public bool IsInteracting { get; private set; } = false;

        public void Initialize(InputManager inputManager)
        {
            // You can add initialization logic here if needed in future
        }

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
            viewController?.BeginInteraction();

            if (minimap != null) minimap.IsInteracting = true;
            if (waypointSystem != null) waypointSystem.IsInteracting = true;
        }

        public void ExitInteraction()
        {
            if (!IsInteracting) return;

            IsInteracting = false;
            viewController?.EndInteraction();

            if (minimap != null) minimap.IsInteracting = false;
            if (waypointSystem != null) waypointSystem.IsInteracting = false;
        }
    }
}
