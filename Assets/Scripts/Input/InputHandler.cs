using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Input
{
    public class InputHandler : MonoBehaviour, PlayerControls.IPlayerMovementActions
    {
        // Mouse
        private Vector2 m_mouseDelta;
        public Vector2 MouseDelta => m_mouseDelta;

        // Keyboard
        private Vector2 m_inputVector;
        public Vector2 InputVector => m_inputVector;
        public bool InputDetected => InputVector != Vector2.zero;

        public Action OnJumpClicked;
        public Action OnInteractClicked;

        public Action OnCrouchClicked;

        public PlayerControls m_controls;

        #region Built-In Functions
        private void OnEnable()
        {
            if (m_controls != null)
                return;

            m_controls = new PlayerControls();
            m_controls.PlayerMovement.SetCallbacks(this);
            m_controls.PlayerMovement.Enable();
        }

        private void OnDisable()
        {
            m_controls.PlayerMovement.Disable();
        }
        #endregion

        public void OnLook(InputAction.CallbackContext t_context)
        {
            m_mouseDelta = t_context.ReadValue<Vector2>();
        }

        public void OnMove(InputAction.CallbackContext t_context)
        {
            m_inputVector = t_context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext t_context)
        {
            if (!t_context.performed)
                return;

            OnJumpClicked?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext t_context)
        {
            if (!t_context.performed)
                return;

            OnCrouchClicked?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext t_context)
        {
            if (!t_context.performed)
                return;

            OnInteractClicked?.Invoke();
        }
    }
}