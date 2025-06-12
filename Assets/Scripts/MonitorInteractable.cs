using UnityEngine;
using SLC.SpaceHorror.Input;

namespace SLC.SpaceHorror.Core
{
    public class MonitorInteractable : InteractableBase
    {
        [SerializeField] private NavigationMonitorManager monitorHandler;

        private InputManager inputManager;

        private void Awake()
        {
            inputManager = FindFirstObjectByType<InputManager>();
            if (monitorHandler != null && inputManager != null)
            {
                monitorHandler.Initialize(inputManager);
            }
            else
            {
                Debug.LogWarning("MonitorInteractable: Missing InputManager or MonitorHandler reference.");
            }
        }

        public override void OnInteracted()
        {
            if (!IsInteractable || monitorHandler == null) return;

            if (!monitorHandler.IsInteracting)
                monitorHandler.EnterInteraction();
            else
                monitorHandler.ExitInteraction(); // Optional toggle behavior
        }
    }
}