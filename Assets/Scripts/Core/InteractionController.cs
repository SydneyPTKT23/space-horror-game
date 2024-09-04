using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class InteractionController : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float rayDistance = 2.0f;
        [SerializeField] private float raySphereRadius = 0.1f;
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Space, Header("UI")]

        private InputHandler m_inputHandler;
        private Camera m_camera;

        private bool isInteracting;
        private float holdingTimer = 0.0f;

        public InteractableBase m_interactable;

        private void Awake()
        {
            m_camera = GetComponentInChildren<Camera>();
            m_inputHandler = GetComponent<InputHandler>();
        }

        private void Update()
        {
            CheckForInteractables();
            CheckForInput();
        }

        private void CheckForInteractables()
        {
            Ray t_ray = new(m_camera.transform.position, m_camera.transform.forward);
            bool t_hitSomething = Physics.SphereCast(t_ray, raySphereRadius, out RaycastHit t_hitInfo, rayDistance, interactableLayer);

            if (t_hitSomething)
            {
                InteractableBase t_interactable = t_hitInfo.transform.GetComponent<InteractableBase>();

                if (t_interactable != null)
                {
                    m_interactable = t_interactable;
                }
            }
            else
            {
                ResetInteractable();
            }

            Debug.DrawRay(t_ray.origin, t_ray.direction * rayDistance, t_hitSomething ? Color.green : Color.red);
        }

        private void Interact()
        {
            m_interactable.OnInteract();
            ResetInteractable();
        }

        private void CheckForInput()
        {
            if (m_interactable == null)
                return;

            if (m_inputHandler.InteractClicked)
            {
                isInteracting = true;
                holdingTimer = 0.0f;
            }

            if (m_inputHandler.InteractedReleased)
            {
                isInteracting = false;
                holdingTimer = 0.0f;
            }

            if (isInteracting)
            {
                if (!m_interactable.IsInteractable)
                    return;

                if (m_interactable.HoldToInteract)
                {
                    holdingTimer += Time.deltaTime;

                    float t_heldPercent = holdingTimer / m_interactable.HoldDuration;

                    if (t_heldPercent > 1.0f)
                    {
                        Interact();
                        isInteracting = false;
                    }
                }
                else
                {
                    Interact();
                    isInteracting = false;
                }
            }
        }

        private void ResetInteractable()
        {
            holdingTimer = 0.0f;
            m_interactable = null;
        }
    }
}