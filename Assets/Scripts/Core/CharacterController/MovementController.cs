using SLC.SpaceHorror.Input;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7.0f;
        [SerializeField] private float crouchSpeed = 2.0f;
        [Range(0f, 1f)][SerializeField] private float moveBackwardsSpeedPercent = 0.5f;
        [Range(0f, 1f)][SerializeField] private float moveSideSpeedPercent = 0.75f;
        [SerializeField] private float jumpForce = 10.0f;

        [Header("Ground Settings")]
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float stickToGroundForce = 5.0f;
        [SerializeField] private LayerMask groundLayer = ~0;
        [Space]
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private float raySphereRadius = 0.1f;

        [Header("Smoothing")]
        [SerializeField] private float smoothInputSpeed = 10.0f;
        [SerializeField] private float smoothVelocitySpeed = 10.0f;
        [SerializeField] private float smoothFinalDirectionSpeed = 10.0f;

        [Header("Debug (Read-Only)")]
        [SerializeField] private Vector2 m_inputVector;
        [SerializeField] private Vector2 m_smoothInputVector;
        [Space]
        [SerializeField] private Vector3 m_smoothFinalMoveDir;
        [SerializeField] private Vector3 m_finalMoveVector;
        [Space]
        [SerializeField] private float m_currentSpeed;
        [SerializeField] private float m_smoothCurrentSpeed;
        [Space]
        [SerializeField] private bool m_isGrounded;
        [SerializeField] private float m_inAirTimer;

        private CharacterController m_characterController;
        private InputManager m_inputManager;
        private Health m_health;
        private RaycastHit m_hitInfo;

        private float m_finalRayLength;
        private bool m_previouslyGrounded;
        private readonly float killHeight = -50.0f;

        public bool IsDead { get; private set; }

        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();
            m_inputManager = GetComponent<InputManager>();
            m_health = GetComponent<Health>();

            m_health.OnDie += OnDie;
            m_inputManager.OnJumpClicked += HandleJump;

            m_finalRayLength = rayLength + m_characterController.center.y;
        }

        private void Update()
        {
            if (IsDead) return;

            SmoothMovementParameters();
            CalculateSpeed();
            CheckIfGrounded();
            HandleMovement();
            ApplyGravity();
            ApplyMovement();
        }

        private void FixedUpdate()
        {
            if (!IsDead && transform.position.y < killHeight)
            {
                m_health.Kill();
            }
        }

        private void OnDie() => IsDead = true;

        private void CheckIfGrounded()
        {
            Vector3 origin = transform.position + m_characterController.center;
            m_isGrounded = Physics.SphereCast(origin, raySphereRadius, Vector3.down, out m_hitInfo, m_finalRayLength, groundLayer);

#if UNITY_EDITOR
            Debug.DrawRay(origin, Vector3.down * rayLength, Color.red);
#endif
        }

        private void SmoothMovementParameters()
        {
            m_inputVector = m_inputManager.InputVector;
            m_smoothInputVector = Vector2.Lerp(m_smoothInputVector, m_inputVector, Time.deltaTime * smoothInputSpeed);
            m_smoothCurrentSpeed = Mathf.Lerp(m_smoothCurrentSpeed, m_currentSpeed, Time.deltaTime * smoothVelocitySpeed);
            m_smoothFinalMoveDir = Vector3.Lerp(m_smoothFinalMoveDir, m_finalMoveVector, Time.deltaTime * smoothFinalDirectionSpeed);
        }

        private void CalculateSpeed()
        {
            if (!m_inputManager.InputDetected)
            {
                m_currentSpeed = 0f;
                return;
            }

            m_currentSpeed = moveSpeed;

            if (m_inputVector.y < 0)
                m_currentSpeed *= moveBackwardsSpeedPercent;
            else if (m_inputVector.x != 0 && m_inputVector.y == 0)
                m_currentSpeed *= moveSideSpeedPercent;
        }

        private void HandleMovement()
        {
            if (!m_isGrounded) return;

            Vector3 t_moveDir = Vector3.ProjectOnPlane(
                (transform.forward * m_smoothInputVector.y) + (transform.right * m_smoothInputVector.x),
                m_hitInfo.normal
            );

            m_finalMoveVector = new Vector3(t_moveDir.x * m_smoothCurrentSpeed, m_finalMoveVector.y, t_moveDir.z * m_smoothCurrentSpeed);
            m_inAirTimer = 0.0f;
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
            if (m_isGrounded || m_finalMoveVector.y <= Physics.gravity.y) return;

            m_inAirTimer += Time.deltaTime;
            m_finalMoveVector += gravityMultiplier * Time.deltaTime * Physics.gravity;
        }

        private void ApplyMovement()
        {
            m_characterController.Move(m_finalMoveVector * Time.deltaTime);
        }
    }
}