using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
    public class InputReader : ScriptableObject, Controls.IPlayerActions, Controls.IUIActions
    {
        public enum InputContext
        {
            Ship,
            Monitor
        }

        public InputContext CurrentContext { get; private set; }

        [Header("Player Input States")]
        private Vector2 inputVector;
        public Vector2 InputVector => inputVector;

        private Vector2 mouseDelta;
        public Vector2 MouseDelta => mouseDelta;
        
        public bool HasInput => inputVector.sqrMagnitude > 0.01f;

        public bool JumpPressedThisFrame { get; private set; }
        public bool JumpReleasedThisFrame { get; private set; }
        public bool JumpHeld { get; private set; }

        public bool CrouchPressedThisFrame { get; private set; }
        public bool CrouchReleasedThisFrame { get; private set; }
        public bool CrouchHeld { get; private set; }

        public bool InteractPressedThisFrame { get; private set; }
        public bool InteractReleasedThisFrame { get; private set; }
        public bool InteractHeld { get; private set; }

        public bool PausePressedThisFrame { get; private set; }

        [Header("Toggle Settings")]
        [Tooltip("If true, crouch will toggle on press instead of requiring hold.")]
        [SerializeField] private bool crouchToggleMode = false;

        private bool crouchToggled = false;

        [Header("Events")]
        public UnityEvent JumpEvent = new();
        public UnityEvent InteractEvent = new();
        public UnityEvent CrouchEvent = new();
        public UnityEvent PauseEvent = new();

        // UI events
        public UnityEvent SubmitEvent = new();
        public UnityEvent CancelEvent = new();

        [Header("UI Input States")]
        private Vector2 navigateInput;
        public Vector2 NavigateInput => navigateInput;

        private Vector2 pointerPosition;
        public Vector2 PointerPosition => pointerPosition;

        private Vector2 scrollDelta;
        public Vector2 ScrollDelta => scrollDelta;

        public bool ClickPressedThisFrame { get; private set; }
        public bool RightClickPressedThisFrame { get; private set; }

        public UnityEvent ClickEvent = new();
        public UnityEvent RightClickEvent = new();
        public UnityEvent ScrollEvent = new();

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

        /// <summary>
        /// Call this every frame after processing input to reset one-frame flags.
        /// </summary>
        public void ClearOneFrameInputFlags()
        {
            JumpPressedThisFrame = false;
            JumpReleasedThisFrame = false;

            CrouchPressedThisFrame = false;
            CrouchReleasedThisFrame = false;

            InteractPressedThisFrame = false;
            InteractReleasedThisFrame = false;

            PausePressedThisFrame = false;

            ClickPressedThisFrame = false;
            RightClickPressedThisFrame = false;
        }

        public void ResetValues()
        {
            inputVector = Vector2.zero;
            mouseDelta = Vector2.zero;

            JumpPressedThisFrame = false;
            JumpReleasedThisFrame = false;
            JumpHeld = false;

            CrouchPressedThisFrame = false;
            CrouchReleasedThisFrame = false;
            CrouchHeld = false;
            crouchToggled = false;

            InteractPressedThisFrame = false;
            InteractReleasedThisFrame = false;
            InteractHeld = false;

            PausePressedThisFrame = false;

            navigateInput = Vector2.zero;
            pointerPosition = Vector2.zero;
            scrollDelta = Vector2.zero;

            ClickPressedThisFrame = false;
            RightClickPressedThisFrame = false;
        }

        #region Player Actions

        public void OnMove(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
                inputVector = Vector2.zero;
            else
                inputVector = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
                mouseDelta = Vector2.zero;
            else
                mouseDelta = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                JumpHeld = true;
                JumpPressedThisFrame = true;
                JumpEvent?.Invoke();
            }
            else if (context.canceled)
            {
                JumpHeld = false;
                JumpReleasedThisFrame = true;
            }
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            if (crouchToggleMode)
            {
                if (context.performed)
                {
                    crouchToggled = !crouchToggled;
                    CrouchHeld = crouchToggled;
                    CrouchPressedThisFrame = crouchToggled;
                    CrouchReleasedThisFrame = !crouchToggled;
                    CrouchEvent?.Invoke();
                }
            }
            else
            {
                if (context.performed)
                {
                    CrouchHeld = true;
                    CrouchPressedThisFrame = true;
                    CrouchEvent?.Invoke();
                }
                else if (context.canceled)
                {
                    CrouchHeld = false;
                    CrouchReleasedThisFrame = true;
                }
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                InteractHeld = true;
                InteractPressedThisFrame = true;
                InteractEvent?.Invoke();
            }
            else if (context.canceled)
            {
                InteractHeld = false;
                InteractReleasedThisFrame = true;
            }
        }

        public void OnPause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                PausePressedThisFrame = true;
                PauseEvent?.Invoke();
            }
        }

        #endregion

        #region UI Actions

        public void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
                navigateInput = Vector2.zero;
            else
                navigateInput = context.ReadValue<Vector2>();
        }

        public void OnSubmit(InputAction.CallbackContext context)
        {
            if (context.performed)
                SubmitEvent?.Invoke();
        }

        public void OnCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
                CancelEvent?.Invoke();
        }

        public void OnPoint(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
                pointerPosition = Vector2.zero;
            else
                pointerPosition = context.ReadValue<Vector2>();
        }

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ClickPressedThisFrame = true;
                ClickEvent?.Invoke();
            }
        }

        public void OnRightClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                RightClickPressedThisFrame = true;
                RightClickEvent?.Invoke();
            }
        }

        public void OnScrollWheel(InputAction.CallbackContext context)
        {
            scrollDelta = context.ReadValue<Vector2>();
            if (scrollDelta != Vector2.zero)
                ScrollEvent?.Invoke();
        }
        #endregion
    }
}
