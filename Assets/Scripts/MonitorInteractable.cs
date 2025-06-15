using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using SLC.SpaceHorror.Input;
using System.Collections;

namespace SLC.SpaceHorror.Core
{
    public class MonitorInteractable : InteractableBase
    {
        [Header("Monitor Interaction")]
        [SerializeField] private MonoBehaviour monitorHandler;
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

        private IMonitorHandler monitor;
        private GameObject lastSelectedUIElement;

        private void Awake()
        {
            monitor = monitorHandler as IMonitorHandler;
        }

        private void Update()
        {
            if (!isInteracting || playerCamera == null || cameraViewPoint == null)
                return;

            UpdateCameraPositionAndRotation();
        }

        private void OnCancel()
        {
            if (isInteracting)
                EndInteraction();
        }

        private void UpdateCameraPositionAndRotation()
        {
            float step = Time.deltaTime * lerpSpeed;

            playerCamera.transform.position = Vector3.Lerp(
                playerCamera.transform.position,
                targetPosition,
                step
            );

            Vector2 screenCenter = new(Screen.width * 0.5f, Screen.height * 0.5f);
            Vector2 mousePos = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : screenCenter;

            Vector2 targetPoint = Vector2.Lerp(mousePos, screenCenter, cursorToCenterWeight);

            Vector2 normalized = new(
                (targetPoint.x - screenCenter.x) / screenCenter.x,
                (targetPoint.y - screenCenter.y) / screenCenter.y
            );

            normalized = Vector2.ClampMagnitude(normalized, 1f);

            float targetYaw = normalized.x * maxParallaxAngle;
            float targetPitch = -normalized.y * maxParallaxAngle;

            currentRotation.x = Mathf.Lerp(currentRotation.x, targetPitch, step);
            currentRotation.y = Mathf.Lerp(currentRotation.y, targetYaw, step);

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

            monitor?.EnterInteraction();

            inputReader.CancelEvent.AddListener(OnCancel);
            inputReader.DisablePlayerInput();
            inputReader.DisableUIInput();
            inputReader.EnableMonitorInput();

            if (playerMovement != null)
                playerMovement.SetMovementEnabled(false);

            if (playerCamera != null && cameraViewPoint != null)
            {
                originalCamPosition = playerCamera.transform.position;
                originalCamRotation = playerCamera.transform.rotation;
                targetPosition = cameraViewPoint.position;
                targetRotation = cameraViewPoint.rotation;
            }

            if (IsInButtonMode() && lastSelectedUIElement != null)
                StartCoroutine(RestoreLastSelectedNextFrame());
        }

        private void EndInteraction()
        {
            isInteracting = false;

            monitor?.ExitInteraction();

            if (inputReader != null)
                inputReader.CancelEvent.RemoveListener(OnCancel);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            inputReader.DisableMonitorInput();
            inputReader.EnablePlayerInput();
            inputReader.EnableUIInput();

            if (playerMovement != null)
                playerMovement.SetMovementEnabled(true);

            if (playerCamera != null)
                playerCamera.transform.SetPositionAndRotation(originalCamPosition, originalCamRotation);

            currentRotation = Vector2.zero;

            if (IsInButtonMode())
            {
                GameObject current = EventSystem.current?.currentSelectedGameObject;
                if (current != null && current.activeInHierarchy)
                    lastSelectedUIElement = current;
            }
        }

        private IEnumerator RestoreLastSelectedNextFrame()
        {
            yield return null;
            if (EventSystem.current != null && lastSelectedUIElement != null)
            {
                EventSystem.current.SetSelectedGameObject(lastSelectedUIElement);
            }
        }

        private bool IsInButtonMode()
        {
            return monitor != null && monitor.IsInButtonMode;
        }
    }
}
