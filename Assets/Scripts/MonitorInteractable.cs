using UnityEngine;
using SLC.SpaceHorror.Input;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Core
{
    public class MonitorInteractable : InteractableBase
    {
        [Header("Monitor Interaction")]
        [SerializeField] private NavigationMonitorManager monitorHandler;
        [SerializeField] private InputReader inputReader;

        [Header("Camera & Movement")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private MovementController playerMovement;
        [SerializeField] private Transform cameraViewPoint;

        [Header("Camera Lerp Settings")]
        [SerializeField, Min(0f)] private float lerpSpeed = 5f;

        [Header("Parallax Settings")]
        [SerializeField, Range(0f, 1f)] private float cursorToCenterWeight = 0.5f;
        [SerializeField, Min(0f)] private float maxParallaxAngle = 5f;

        private Vector2 currentRotation;
        private bool isInteracting;

        private Vector3 originalCamPosition;
        private Quaternion originalCamRotation;

        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private void Update()
        {
            if (!isInteracting || playerCamera == null || cameraViewPoint == null)
                return;

            UpdateCameraPositionAndRotation();
        }

        private void OnCancel()
        {
            if (isInteracting)
            {
                EndInteraction();
            }
        }

        private void UpdateCameraPositionAndRotation()
        {
            // Smoothly lerp camera position to target point
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPosition,
                Time.deltaTime * lerpSpeed);

            // Calculate cursor offset relative to screen center with weight
            Vector2 mousePos = UnityEngine.Input.mousePosition;
            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 targetPoint = Vector2.Lerp(mousePos, screenCenter, cursorToCenterWeight);

            Vector2 normalized = new(
                (targetPoint.x - screenCenter.x) / (screenCenter.x),
                (targetPoint.y - screenCenter.y) / (screenCenter.y));

            normalized = Vector2.ClampMagnitude(normalized, 1f);

            // Calculate rotation target angles based on cursor position
            float targetYaw = normalized.x * maxParallaxAngle;
            float targetPitch = -normalized.y * maxParallaxAngle;

            // Smoothly interpolate current rotation towards target angles
            currentRotation.x = Mathf.Lerp(currentRotation.x, targetPitch, Time.deltaTime * lerpSpeed);
            currentRotation.y = Mathf.Lerp(currentRotation.y, targetYaw, Time.deltaTime * lerpSpeed);

            // Apply the rotation offset to the camera's original rotation
            Quaternion offsetRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
            playerCamera.transform.rotation = targetRotation * offsetRotation;
        }

        public override void OnInteracted()
        {
            if (!IsInteractable || monitorHandler == null)
                return;

            base.OnInteracted();

            if (!isInteracting)
                BeginInteraction();
            else
                EndInteraction();
        }

        private void BeginInteraction()
        {
            isInteracting = true;
            monitorHandler.EnterInteraction();

            if (inputReader != null)
                inputReader.CancelEvent.AddListener(OnCancel);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            inputReader.DisablePlayerInput();
            inputReader.DisableUIInput();
            inputReader.EnableMonitorInput();

            if (playerMovement != null) playerMovement.SetMovementEnabled(false);

            if (playerCamera != null && cameraViewPoint != null)
            {
                originalCamPosition = playerCamera.transform.position;
                originalCamRotation = playerCamera.transform.rotation;
                targetPosition = cameraViewPoint.position;
                targetRotation = cameraViewPoint.rotation;
            }
        }

        private void EndInteraction()
        {
            isInteracting = false;
            monitorHandler.ExitInteraction();

            if (inputReader != null)
                inputReader.CancelEvent.RemoveListener(OnCancel);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            inputReader.DisableMonitorInput();
            inputReader.EnablePlayerInput();

            if (playerMovement != null) playerMovement.SetMovementEnabled(true);

            if (playerCamera != null)
                playerCamera.transform.SetPositionAndRotation(originalCamPosition, originalCamRotation);

            currentRotation = Vector2.zero;
        }
    }
}