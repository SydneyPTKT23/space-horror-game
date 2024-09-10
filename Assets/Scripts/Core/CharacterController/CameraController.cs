using SLC.SpaceHorror.Input;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private int sensitivity = 10;

        private float m_desiredPitch;

        private InputHandler m_inputHandler;
        public Camera m_cam;

        private void Awake()
        {
            m_inputHandler = GetComponent<InputHandler>();

            ChangeCursorState();
        }

        private void Update()
        {
            float t_mouseX = m_inputHandler.MouseDelta.x * sensitivity * Time.deltaTime;
            float t_mouseY = m_inputHandler.MouseDelta.y * sensitivity * Time.deltaTime;

            m_desiredPitch -= t_mouseY;
            m_desiredPitch = Mathf.Clamp(m_desiredPitch, -90.0f, 90.0f);

            m_cam.transform.localRotation = Quaternion.Euler(m_desiredPitch, 0f, 0f);
            transform.Rotate(Vector3.up * t_mouseX);
        }

        private void ChangeCursorState()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}