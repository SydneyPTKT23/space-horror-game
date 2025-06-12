using UnityEngine;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
    public class InputReader : ScriptableObject, Controls.IPlayerActions, Controls.IUIActions
    {
        [Header("Player Input States")]
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool CrouchPressed { get; private set; }
        public bool InteractPressed { get; private set; }

        [Header("Toggle Settings")]
        [Tooltip("If true, crouch will toggle on press instead of requiring hold.")]
        [SerializeField] private bool crouchToggleMode = false;

        private bool crouchHeld = false;
        private bool crouchToggled = false;

        private Controls controls;

        public void Initialize()
        {
            if (controls == null)
            {
                controls = new Controls();

                controls.Player.SetCallbacks(this);
                controls.UI.SetCallbacks(this);

                controls.Enable();
                controls.UI.Disable(); // default to Player mode
            }
        }

        public void EnablePlayerInput() => controls.Player.Enable();
        public void DisablePlayerInput() => controls.Player.Disable();

        public void EnableUIInput() => controls.UI.Enable();
        public void DisableUIInput() => controls.UI.Disable();

        public void ResetValues()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            JumpPressed = false;
            CrouchPressed = false;
            crouchHeld = false;
            crouchToggled = false;
            InteractPressed = false;
        }

        #region Player Actions
        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
                Move = Vector2.zero;
            else
                Move = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
                Look = Vector2.zero;
            else
                Look = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            JumpPressed = context.performed;
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (crouchToggleMode)
            {
                if (context.performed)
                    crouchToggled = !crouchToggled;

                CrouchPressed = crouchToggled;
            }
            else
            {
                if (context.performed)
                    crouchHeld = true;
                if (context.canceled)
                    crouchHeld = false;

                CrouchPressed = crouchHeld;
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            InteractPressed = context.performed;
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            // Optional: Implement Pause event dispatch here.
        }
        #endregion

        #region UI Actions
        public void OnNavigate(InputAction.CallbackContext context) { }
        public void OnSubmit(InputAction.CallbackContext context) { }
        public void OnCancel(InputAction.CallbackContext context) { }
        public void OnPoint(InputAction.CallbackContext context) { }
        public void OnClick(InputAction.CallbackContext context) { }
        public void OnRightClick(InputAction.CallbackContext context) { }
        public void OnScrollWheel(InputAction.CallbackContext context) { }
        #endregion
    }
}