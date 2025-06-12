using SLC.SpaceHorror.Input;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class CameraController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private InputReader inputReader;

        [Header("Camera Settings")]
        [SerializeField] private float sensitivity = 10f;
        [SerializeField] private Vector2 lookAngleMinMax = Vector2.zero;

        [Header("References")]
        [SerializeField] private Camera cam;

        private float _desiredPitch = 0f;

        private void Awake()
        {
            if (cam == null)
                cam = GetComponentInChildren<Camera>();

            LockCursor();
        }

        private void Update()
        {
            Vector2 lookDelta = inputReader != null ? inputReader.MouseDelta : Vector2.zero;

            float yaw = lookDelta.x * sensitivity * Time.deltaTime;
            float pitch = lookDelta.y * sensitivity * Time.deltaTime;

            _desiredPitch -= pitch;
            _desiredPitch = Mathf.Clamp(_desiredPitch, lookAngleMinMax.x, lookAngleMinMax.y);

            if (cam != null)
                cam.transform.localRotation = Quaternion.Euler(_desiredPitch, 0f, 0f);

            transform.Rotate(Vector3.up * yaw);
        }

        private void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}