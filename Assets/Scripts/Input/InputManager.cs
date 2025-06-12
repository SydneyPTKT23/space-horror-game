using UnityEngine;
using UnityEngine.Events;

namespace SLC.SpaceHorror.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;

        [Header("Input Events")]
        public UnityEvent OnJumpPressed;
        public UnityEvent OnCrouchPressed;
        public UnityEvent OnInteractPressed;
        public UnityEvent OnPausePressed;
        public UnityEvent OnUISubmit;
        public UnityEvent OnUICancel;

        private void Awake()
        {
            if (inputReader != null)
            {
                inputReader.Initialize();

                inputReader.JumpEvent.AddListener(() => OnJumpPressed?.Invoke());
                inputReader.CrouchEvent.AddListener(() => OnCrouchPressed?.Invoke());
                inputReader.InteractEvent.AddListener(() => OnInteractPressed?.Invoke());
                inputReader.PauseEvent.AddListener(() => OnPausePressed?.Invoke());
                inputReader.SubmitEvent.AddListener(() => OnUISubmit?.Invoke());
                inputReader.CancelEvent.AddListener(() => OnUICancel?.Invoke());
            }
        }

        private void OnEnable()
        {
            if (inputReader != null)
                inputReader.EnablePlayerInput();
        }

        private void OnDisable()
        {
            if (inputReader != null)
                inputReader.DisablePlayerInput();
        }

        private void Update()
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