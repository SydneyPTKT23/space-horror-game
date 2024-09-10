using SLC.SpaceHorror.Input;
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

        public InteractableBase m_interactable;

        private void Awake()
        {
            m_inputHandler = GetComponent<InputHandler>();
            m_camera = GetComponentInChildren<Camera>();            
        }

        private void Start()
        {
            m_inputHandler.OnInteractClicked += OnInteract;
        }

        private void Update()
        {
            CheckForInteractables();
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

        private void OnInteract()
        {
            if (m_interactable == null || !m_interactable.IsInteractable)
                return;

            Interact();
        }

        private void ResetInteractable()
        {
            m_interactable = null;
        }
    }
}