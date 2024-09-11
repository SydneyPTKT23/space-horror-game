using SLC.SpaceHorror.Input;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SLC.SpaceHorror.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class MovementController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 7.0f;
        [SerializeField] private float jumpForce = 10.0f;
        [Range(0.0f, 1.0f), SerializeField] private float moveBackwardsPercent = 0.5f;
        [Range(0.0f, 1.0f), SerializeField] private float moveSidePercent = 0.75f;

        [Header("Ground Settings")]
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float stickToGroundForce = 5.0f;
        [Space]
        [SerializeField] private LayerMask groundLayer = ~0;
        [SerializeField] private float rayLength = 0.1f;
        [SerializeField] private float raySphereRadius = 0.1f;

        private CharacterController m_characterController;
        private InputHandler m_inputHandler;
        private Health m_health;
        private CameraController m_cameraController;

        private RaycastHit m_hitInfo;

        [Space, Header("DEBUG")]
        [SerializeField] private Vector2 m_inputVector;
        [SerializeField] private Vector3 m_finalMoveVector;
        [Space]
        [SerializeField] private float m_currentSpeed;
        [Space]
        [SerializeField] private float m_finalRayLength;
        [SerializeField] private bool m_isGrounded;

        public bool jump;

        public float killHeight = -50.0f;
        public bool IsDead { get; private set; }

        private void Start()
        {
            m_characterController = GetComponent<CharacterController>();
            m_cameraController = GetComponent<CameraController>();
            //m_camTransform = m_cameraController.transform;
            m_inputHandler = GetComponent<InputHandler>();

            m_inputHandler.OnJumpClicked += HandleJump;

            m_health = GetComponent<Health>();
            m_health.OnDie += OnDie;

            m_finalRayLength = rayLength + m_characterController.center.y;
            m_isGrounded = true;
        }

        private void Update()
        {
            // Autokill player if they manage to fall out of the map to prevent softlocking.
            if (!IsDead && transform.position.y < killHeight)
            {
                m_health.Kill();
            }

            if (m_characterController)
            {
                CheckIfGrounded();

                HandleMovement();

                CalculateMovementSpeed();
                ApplyGravity();

                ApplyMovement();
            }
        }

        private void OnDie()
        {
            IsDead = true;
        }

        private void CheckIfGrounded()
        {
            // Manually check for grounded because the CharacterController default is less reliable.
            Vector3 t_origin = transform.position + m_characterController.center;
            bool t_hitGround = Physics.SphereCast(t_origin, raySphereRadius, Vector3.down, out m_hitInfo, m_finalRayLength, groundLayer);

            // Draw the groundcheck for convenience.
            Debug.DrawRay(t_origin, Vector3.down * rayLength, Color.red);
            m_isGrounded = t_hitGround;
        }

        private bool CheckIfRoof()
        {
            Vector3 t_origin = transform.position;
            bool t_hitRoof = Physics.SphereCast(t_origin, raySphereRadius, Vector3.up, out _, rayLength, groundLayer);
            return t_hitRoof;
        }

        private void HandleMovement()
        {
            m_inputVector = m_inputHandler.InputVector.normalized;

            Vector3 t_desiredDirection = transform.forward * m_inputVector.y + transform.right * m_inputVector.x;
            Vector3 t_flatDirection = FlattenVectorOnSlopes(t_desiredDirection);

            Vector3 t_finalVector = m_currentSpeed * t_flatDirection;

            m_finalMoveVector.x = t_finalVector.x;
            m_finalMoveVector.z = t_finalVector.z;

            if (m_characterController.isGrounded)
                m_finalMoveVector.y += t_finalVector.y;
        }

        private Vector3 FlattenVectorOnSlopes(Vector3 t_flattenedVector)
        {
            // Adjust movement on slopes to keep speed consistent.
            if (m_isGrounded)
                t_flattenedVector = Vector3.ProjectOnPlane(t_flattenedVector, m_hitInfo.normal);

            return t_flattenedVector;
        }

        private void CalculateMovementSpeed()
        {
            m_currentSpeed = !m_inputHandler.InputDetected ? 0.0f : moveSpeed;
            m_currentSpeed = m_inputHandler.InputVector.y == -1 ? m_currentSpeed * moveBackwardsPercent : m_currentSpeed;
            m_currentSpeed = m_inputHandler.InputVector.x != 0 && m_inputVector.y == 0 ? m_currentSpeed * moveSidePercent : m_currentSpeed;
        }

        private void HandleJump()
        {
            if (m_isGrounded && jump == false)
            {
                jump = true;

            }
        }

        private void ApplyGravity()
        {
            // If grounded, add a little bit of extra downward force just in case.
            if (m_characterController.isGrounded)
            {
                m_finalMoveVector.y = -stickToGroundForce;

                if (jump)
                {
                    m_finalMoveVector.y = jumpForce;

                    jump = false;
                    m_isGrounded = false;
                }
            }
            else
            {
                if (CheckIfRoof())
                    m_finalMoveVector.y = -stickToGroundForce;

                m_finalMoveVector += gravityMultiplier * Time.deltaTime * Physics.gravity;
            }
        }

        private void ApplyMovement()
        {
            m_characterController.Move(m_finalMoveVector * Time.deltaTime);
        }
    }
}