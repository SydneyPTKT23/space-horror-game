using UnityEngine;
using UnityEngine.Events;

namespace SLC.SpaceHorror.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;

        [Header("Player Input Events")]
        public UnityEvent OnJumpPressed;
        public UnityEvent OnCrouchPressed;
        public UnityEvent OnInteractPressed;
        public UnityEvent OnPausePressed;

        [Header("UI Input Events")]
        public UnityEvent OnUISubmit;
        public UnityEvent OnUICancel;
        public UnityEvent OnClickPressed;
        public UnityEvent OnRightClickPressed;
        public UnityEvent OnScroll;

        [Header("Monitor Input Events")]
        public UnityEvent OnMonitorTogglePressed;
        public UnityEvent OnMonitorNextPressed;
        public UnityEvent OnMonitorPreviousPressed;
        public UnityEvent OnMonitorActionPressed;
        public UnityEvent OnMonitorResetPressed;

        private void Awake()
        {
            if (inputReader != null)
            {
                inputReader.Initialize();

                // Player input events
                inputReader.JumpEvent.AddListener(() => OnJumpPressed?.Invoke());
                inputReader.CrouchEvent.AddListener(() => OnCrouchPressed?.Invoke());
                inputReader.InteractEvent.AddListener(() => OnInteractPressed?.Invoke());
                inputReader.PauseEvent.AddListener(() => OnPausePressed?.Invoke());

                // UI input events
                inputReader.SubmitEvent.AddListener(() => OnUISubmit?.Invoke());
                inputReader.CancelEvent.AddListener(() => OnUICancel?.Invoke());
                inputReader.ClickEvent.AddListener(() => OnClickPressed?.Invoke());
                inputReader.RightClickEvent.AddListener(() => OnRightClickPressed?.Invoke());
                inputReader.ScrollEvent.AddListener(() => OnScroll?.Invoke());

                // Monitor input events
                inputReader.MonitorToggleEvent.AddListener(() => OnMonitorTogglePressed?.Invoke());
                inputReader.MonitorNextEvent.AddListener(() => OnMonitorNextPressed?.Invoke());
                inputReader.MonitorPreviousEvent.AddListener(() => OnMonitorPreviousPressed?.Invoke());
                inputReader.MonitorActionEvent.AddListener(() => OnMonitorActionPressed?.Invoke());
                inputReader.MonitorResetEvent.AddListener(() => OnMonitorResetPressed?.Invoke());
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.EnablePlayerInput();
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.DisablePlayerInput();
            }
        }

        private void LateUpdate()
        {
            if (inputReader != null)
                inputReader.ClearOneFrameInputFlags();
        }

        private void OnDestroy()
        {
            if (inputReader != null)
                inputReader.ResetValues();
        }
    }
}