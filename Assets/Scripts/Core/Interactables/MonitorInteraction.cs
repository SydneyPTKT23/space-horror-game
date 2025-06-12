using UnityEngine;
using SLC.SpaceHorror.Input;

namespace SLC.SpaceHorror.Core
{
    public class MonitorInteraction : InteractableBase
    {
        [Header("Monitor Interaction Settings")]
        [SerializeField] private Canvas monitorCanvas;
        [SerializeField] private Camera uiCamera;
        [SerializeField] private MovementController playerMovement;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CameraController playerCameraController;
        [SerializeField] private Transform cameraViewPoint;

        [Header("Lerp Settings")]
        [SerializeField] private float lerpSpeed = 5f;

        [Header("Parallax Settings")]
        [SerializeField] private float maxParallaxAngle = 5f; // degrees max offset
        private Vector2 currentRotation = Vector2.zero; // pitch (x), yaw (y)
        [SerializeField, Range(0f, 1f)] private float cursorToCenterWeight = 0.5f; // 0=cursor only, 1=center only (no rotation)

        private bool isActive = false;

        private Vector3 originalCamPosition;
        private Quaternion originalCamRotation;

        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private Vector2 parallaxInput;
        private InputManager inputManager;

        private void Start()
        {
            inputManager = FindFirstObjectByType<InputManager>();

            if (uiCamera != null)
            {
                uiCamera.gameObject.SetActive(true);
                uiCamera.enabled = true;
            }

            // Keep monitor canvas active but disable drawing until interaction
            if (monitorCanvas != null)
            {
                monitorCanvas.gameObject.SetActive(true);
            }
        }

        public override void OnInteracted()
        {
            if (!IsInteractable) return;

            isActive = !isActive;

            if (isActive)
                StartInteraction();
            else
                StopInteraction();
        }

        private void StartInteraction()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (monitorCanvas != null)
                monitorCanvas.enabled = true;  // enable rendering

            if (playerMovement != null)
                playerMovement.SetMovementEnabled(false);

            if (playerCameraController != null)
                playerCameraController.enabled = false;

            // Store camera original transform
            originalCamPosition = playerCamera.transform.position;
            originalCamRotation = playerCamera.transform.rotation;

            targetPosition = cameraViewPoint.position;
            targetRotation = cameraViewPoint.rotation;
        }

        private void StopInteraction()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            if (playerMovement != null)
                playerMovement.SetMovementEnabled(true);

            if (playerCameraController != null)
                playerCameraController.enabled = true;

            if (playerCamera != null)
            {
                playerCamera.transform.position = originalCamPosition;
                playerCamera.transform.rotation = originalCamRotation;
            }

            parallaxInput = Vector2.zero;
        }        

        private void Update()
        {
            if (!isActive || playerCamera == null) return;

            // Smoothly lerp position
            playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, targetPosition, Time.deltaTime * lerpSpeed);

            if (inputManager != null)
            {
                Vector2 mousePos = UnityEngine.Input.mousePosition; // mouse position in pixels

                Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

                // Calculate weighted point between cursor and center
                Vector2 targetPoint = Vector2.Lerp(mousePos, screenCenter, cursorToCenterWeight);

                // Convert targetPoint to normalized coords: center= (0,0), left=-1, right=1, top=1, bottom=-1
                Vector2 normalized = new Vector2(
                    (targetPoint.x - screenCenter.x) / (Screen.width / 2f),
                    (targetPoint.y - screenCenter.y) / (Screen.height / 2f)
                );

                // Clamp normalized coords to [-1,1]
                normalized = Vector2.ClampMagnitude(normalized, 1f);

                // Convert normalized coords to rotation angles in degrees (invert Y for pitch)
                float targetYaw = normalized.x * maxParallaxAngle;
                float targetPitch = -normalized.y * maxParallaxAngle;

                // Smoothly lerp current rotation toward target rotation
                currentRotation.x = Mathf.Lerp(currentRotation.x, targetPitch, Time.deltaTime * lerpSpeed);
                currentRotation.y = Mathf.Lerp(currentRotation.y, targetYaw, Time.deltaTime * lerpSpeed);

                // Compose final rotation from targetRotation + currentRotation offsets
                Quaternion rotationOffset = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
                Quaternion finalRotation = targetRotation * rotationOffset;

                playerCamera.transform.rotation = finalRotation;
            }
            else
            {
                // Fallback: smoothly rotate back to fixed rotation
                playerCamera.transform.rotation = Quaternion.Slerp(playerCamera.transform.rotation, targetRotation, Time.deltaTime * lerpSpeed);
            }
        }

    }
}
