using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class CameraController : MonoBehaviour
    {
        private int sensitivity = 10;

        private float m_mouseX;
        private float m_mouseY;

        private float m_desiredPitch;

        public Transform player;
        public Camera m_cam;

        private void Awake()
        {
            ChangeCursorState();
        }

        private void Update()
        {
            m_mouseX = Input.GetAxis("Mouse X") * sensitivity;
            m_mouseY = Input.GetAxis("Mouse Y") * sensitivity;

            m_desiredPitch -= m_mouseY;
            m_desiredPitch = Mathf.Clamp(m_desiredPitch, -90, 90);

            m_cam.transform.localRotation = Quaternion.Euler(m_desiredPitch, 0f, 0f);
            player.Rotate(Vector3.up * m_mouseX);
        }

        private void ChangeCursorState()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}