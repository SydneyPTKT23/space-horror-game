using UnityEngine;
using SLC.SpaceHorror.Input;

namespace SLC.SpaceHorror.Core
{
    public class MonitorViewController : MonoBehaviour
    {
        [Header("Monitor View Settings")]
        [SerializeField] private Camera uiCamera;
        [SerializeField] private MovementController playerMovement;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private CameraController playerCameraController;
        [SerializeField] private Transform cameraViewPoint;

        [Header("Lerp Settings")]
        [SerializeField] private float lerpSpeed = 5f;

        [Header("Parallax Settings")]
        [SerializeField] private float maxParallaxAngle = 5f;
        [SerializeField, Range(0f, 1f)] private float cursorToCenterWeight = 0.5f;

        private bool isActive = false;

        private Vector3 originalCamPosition;
        private Quaternion originalCamRotation;
        private Vector3 targetPosition;
        private Quaternion targetRotation;

        private Vector2 currentRotation;

        public void Initialize(InputManager manager)
        {
            if (uiCamera != null)
            {
                uiCamera.gameObject.SetActive(true);
                uiCamera.enabled = true;
            }
        }

        public void BeginInteraction()
        {
            isActive = true;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (playerMovement != null)
                playerMovement.SetMovementEnabled(false);

            if (playerCameraController != null)
                playerCameraController.enabled = false;

            if (playerCamera != null && cameraViewPoint != null)
            {
                originalCamPosition = playerCamera.transform.position;
                originalCamRotation = playerCamera.transform.rotation;

                targetPosition = cameraViewPoint.position;
                targetRotation = cameraViewPoint.rotation;
            }
        }

        public void EndInteraction()
        {
            isActive = false;

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

            currentRotation = Vector2.zero;
        }

        private void Update()
        {
            if (!isActive || playerCamera == null || cameraViewPoint == null) return;

            // Smoothly move camera toward target monitor viewpoint
            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPosition,
                Time.deltaTime * lerpSpeed
            );

            // Calculate parallax rotation based on cursor offset from screen center
            Vector2 mousePos = UnityEngine.Input.mousePosition;
            Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);
            Vector2 targetPoint = Vector2.Lerp(mousePos, screenCenter, cursorToCenterWeight);

            Vector2 normalized = new(
                (targetPoint.x - screenCenter.x) / (Screen.width / 2f),
                (targetPoint.y - screenCenter.y) / (Screen.height / 2f)
            );
            normalized = Vector2.ClampMagnitude(normalized, 1f);

            float targetYaw = normalized.x * maxParallaxAngle;
            float targetPitch = -normalized.y * maxParallaxAngle;

            currentRotation.x = Mathf.Lerp(currentRotation.x, targetPitch, Time.deltaTime * lerpSpeed);
            currentRotation.y = Mathf.Lerp(currentRotation.y, targetYaw, Time.deltaTime * lerpSpeed);

            Quaternion offset = Quaternion.Euler(currentRotation.x, currentRotation.y, 0f);
            playerCamera.transform.rotation = targetRotation * offset;
        }
    }
}