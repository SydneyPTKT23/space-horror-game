using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/InputReader")]
    public class InputReader : ScriptableObject, Controls.IPlayerActions, Controls.IUIActions, Controls.IMonitorActions
    {
        #region Player Input States

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

        [Header("Player Events")]
        public UnityEvent JumpEvent = new();
        public UnityEvent InteractEvent = new();
        public UnityEvent CrouchEvent = new();
        public UnityEvent PauseEvent = new();

        #endregion

        #region UI Input States

        private Vector2 navigateInput;
        public Vector2 NavigateInput => navigateInput;

        private Vector2 pointerPosition;
        public Vector2 PointerPosition => pointerPosition;

        private Vector2 scrollDelta;
        public Vector2 ScrollDelta => scrollDelta;

        public bool ClickPressedThisFrame { get; private set; }
        public bool RightClickPressedThisFrame { get; private set; }

        [Header("UI Events")]
        public UnityEvent SubmitEvent = new();
        public UnityEvent CancelEvent = new();
        public UnityEvent ClickEvent = new();
        public UnityEvent RightClickEvent = new();
        public UnityEvent ScrollEvent = new();

        #endregion

        #region Monitor Input States

        private Vector2 monitorNavigateInput;
        public Vector2 MonitorNavigateInput => monitorNavigateInput;

        private Vector2 monitorZoomDelta;
        public Vector2 MonitorZoomDelta => monitorZoomDelta;

        public bool MonitorTogglePressedThisFrame { get; private set; }
        public bool MonitorNextPressedThisFrame { get; private set; }
        public bool MonitorPreviousPressedThisFrame { get; private set; }
        public bool MonitorActionPressedThisFrame { get; private set; }
        public bool MonitorResetPressedThisFrame { get; private set; }

        [Header("Monitor Events")]
        public UnityEvent MonitorToggleEvent = new();
        public UnityEvent MonitorNextEvent = new();
        public UnityEvent MonitorPreviousEvent = new();
        public UnityEvent MonitorActionEvent = new();
        public UnityEvent MonitorResetEvent = new();

        #endregion

        private Controls controls;

        public void Initialize()
        {
            if (controls != null) return;

            controls = new Controls();

            controls.Player.SetCallbacks(this);
            controls.UI.SetCallbacks(this);
            controls.Monitor.SetCallbacks(this);

            controls.Enable();
            controls.UI.Disable();
            controls.Monitor.Disable(); // Default to Player mode
        }

        #region Input Mode Control

        public void EnablePlayerInput() => controls?.Player.Enable();
        public void DisablePlayerInput() => controls?.Player.Disable();

        public void EnableUIInput() => controls?.UI.Enable();
        public void DisableUIInput() => controls?.UI.Disable();

        public void EnableMonitorInput() => controls?.Monitor.Enable();
        public void DisableMonitorInput() => controls?.Monitor.Disable();

        #endregion

        public void ClearOneFrameInputFlags()
        {
            JumpPressedThisFrame = JumpReleasedThisFrame = false;
            CrouchPressedThisFrame = CrouchReleasedThisFrame = false;
            InteractPressedThisFrame = InteractReleasedThisFrame = false;
            PausePressedThisFrame = false;
            ClickPressedThisFrame = RightClickPressedThisFrame = false;

            MonitorTogglePressedThisFrame = MonitorNextPressedThisFrame =
                MonitorPreviousPressedThisFrame = MonitorActionPressedThisFrame =
                MonitorResetPressedThisFrame = false;
        }

        public void ResetValues()
        {
            inputVector = mouseDelta = Vector2.zero;
            JumpPressedThisFrame = JumpReleasedThisFrame = JumpHeld = false;
            CrouchPressedThisFrame = CrouchReleasedThisFrame = CrouchHeld = false;
            crouchToggled = false;
            InteractPressedThisFrame = InteractReleasedThisFrame = InteractHeld = false;
            PausePressedThisFrame = false;

            navigateInput = pointerPosition = scrollDelta = Vector2.zero;
            ClickPressedThisFrame = RightClickPressedThisFrame = false;

            monitorNavigateInput = monitorZoomDelta = Vector2.zero;
            MonitorTogglePressedThisFrame = MonitorNextPressedThisFrame =
                MonitorPreviousPressedThisFrame = MonitorActionPressedThisFrame =
                MonitorResetPressedThisFrame = false;
        }

        #region Player Actions

        public void OnMove(InputAction.CallbackContext context)
        {
            inputVector = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context)
        {
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
            if (context.performed)
            {
                if (crouchToggleMode)
                {
                    crouchToggled = !crouchToggled;
                    CrouchHeld = crouchToggled;
                    CrouchPressedThisFrame = crouchToggled;
                    CrouchReleasedThisFrame = !crouchToggled;
                }
                else
                {
                    CrouchHeld = true;
                    CrouchPressedThisFrame = true;
                }

                CrouchEvent?.Invoke();
            }
            else if (!crouchToggleMode && context.canceled)
            {
                CrouchHeld = false;
                CrouchReleasedThisFrame = true;
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

        #region Monitor Actions

        public void OnZoom(InputAction.CallbackContext context)
        {
            monitorZoomDelta = context.ReadValue<Vector2>();
        }

        public void OnMonitorNavigate(InputAction.CallbackContext context)
        {
            monitorNavigateInput = context.ReadValue<Vector2>();
        }

        public void OnToggle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MonitorTogglePressedThisFrame = true;
                MonitorToggleEvent?.Invoke();
            }
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MonitorNextPressedThisFrame = true;
                MonitorNextEvent?.Invoke();
            }
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MonitorPreviousPressedThisFrame = true;
                MonitorPreviousEvent?.Invoke();
            }
        }

        public void OnAction(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MonitorActionPressedThisFrame = true;
                MonitorActionEvent?.Invoke();
            }
        }

        public void OnReset(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                MonitorResetPressedThisFrame = true;
                MonitorResetEvent?.Invoke();
            }
        }

        #endregion
    }
}