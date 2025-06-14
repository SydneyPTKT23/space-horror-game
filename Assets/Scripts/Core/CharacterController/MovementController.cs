using SLC.SpaceHorror.Input;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private InputReader inputReader;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7f;
        [SerializeField] private float crouchSpeed = 2f;
        [Range(0f, 1f)][SerializeField] private float moveBackwardsSpeedPercent = 0.5f;
        [Range(0f, 1f)][SerializeField] private float moveSideSpeedPercent = 0.75f;
        [SerializeField] private float jumpForce = 10f;

        [Header("Ground Settings")]
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float stickToGroundForce = 5f;
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private float raySphereRadius = 0.1f;

        [Header("Smoothing")]
        [SerializeField] private float smoothInputSpeed = 10f;
        [SerializeField] private float smoothVelocitySpeed = 10f;

        [Header("Debug (Read-Only)")]
        [SerializeField] private Vector2 inputVector;
        [SerializeField] private Vector2 smoothInputVector;
        [SerializeField] private Vector3 m_finalMoveVector;
        [SerializeField] private float m_currentSpeed;
        [SerializeField] private float m_smoothCurrentSpeed;
        [SerializeField] private bool m_isGrounded;

        private bool movementEnabled = true;

        private CharacterController characterController;
        private CameraController cameraController;
        private Health m_health;
        private RaycastHit hitInfo;

        private float m_finalRayLength;
        private readonly float killHeight = -50f;

        public bool IsDead { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            cameraController = GetComponentInChildren<CameraController>();

            m_health = GetComponent<Health>();
            m_health.OnDie += OnDie;

            m_finalRayLength = rayLength + Mathf.Abs(characterController.center.y);
        }

        private void Update()
        {
            if (IsDead || !movementEnabled) return;

            ReadInput();
            SmoothMovementParameters();

            CalculateSpeed();
            CheckIfGrounded();

            HandleMovement();

            ApplyGravity();
            ApplyMovement();

            CheckKillHeight();
        }

        private void ReadInput()
        {
            if (inputReader != null)
            {
                inputVector = inputReader.InputVector;
                if (inputReader.JumpPressedThisFrame)
                {
                    HandleJump();
                }
                // TODO: Add crouch and other inputs if needed
            }
            else
            {
                // Fallback: zero input
                inputVector = Vector2.zero;
            }
        }

        private void CheckKillHeight()
        {
            if (transform.position.y < killHeight && !IsDead)
            {
                m_health.Kill();
            }
        }

        private void CheckIfGrounded()
        {
            Vector3 origin = transform.position + characterController.center;
            m_isGrounded = Physics.SphereCast(origin, raySphereRadius, Vector3.down, out hitInfo, m_finalRayLength, groundLayer);

#if UNITY_EDITOR
            Debug.DrawRay(origin, Vector3.down * m_finalRayLength, Color.red);
#endif
        }

        private void SmoothMovementParameters()
        {
            float delta = Time.deltaTime;

            smoothInputVector = Vector2.Lerp(smoothInputVector, inputVector, delta * smoothInputSpeed);
            m_smoothCurrentSpeed = Mathf.Lerp(m_smoothCurrentSpeed, m_currentSpeed, delta * smoothVelocitySpeed);
        }

        private void CalculateSpeed()
        {
            if (!inputReader.HasInput)
            {
                m_currentSpeed = 0f;
                return;
            }

            m_currentSpeed = moveSpeed;

            if (smoothInputVector.y < 0)
                m_currentSpeed *= moveBackwardsSpeedPercent;
            else if (Mathf.Abs(smoothInputVector.x) > 0 && Mathf.Approximately(smoothInputVector.y, 0f))
                m_currentSpeed *= moveSideSpeedPercent;
        }

        private void HandleMovement()
        {
            if (!m_isGrounded)
                return;

            Vector3 moveDir = Vector3.ProjectOnPlane((transform.forward * smoothInputVector.y) + (transform.right * 
                smoothInputVector.x), hitInfo.normal).normalized;

            m_finalMoveVector.x = moveDir.x * m_smoothCurrentSpeed;
            m_finalMoveVector.z = moveDir.z * m_smoothCurrentSpeed;

            // Keep vertical velocity intact, stick to ground force
            m_finalMoveVector.y = Mathf.Max(m_finalMoveVector.y, -stickToGroundForce);
        }

        private void HandleJump()
        {
            if (!m_isGrounded) return;

            m_finalMoveVector.y = jumpForce;
            m_isGrounded = false;
        }

        private void ApplyGravity()
        {
            if (m_isGrounded && m_finalMoveVector.y <= 0f)
                return;

            // Apply gravity scaled by multiplier
            m_finalMoveVector += gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        private void ApplyMovement()
        {
            _ = characterController.Move(m_finalMoveVector * Time.deltaTime);
        }

        private void OnDie()
        {
            IsDead = true;
        }

        public void SetMovementEnabled(bool enabled)
        {
            movementEnabled = enabled;

            if (cameraController != null)
            {
                cameraController.enabled = enabled;
            }
        }
    }
}