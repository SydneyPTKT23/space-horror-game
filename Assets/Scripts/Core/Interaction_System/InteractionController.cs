using SLC.SpaceHorror.Input;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class InteractionController : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private InputReader inputReader;

        [Header("Detection Settings")]
        [SerializeField] private float rayDistance = 2.0f;
        [SerializeField] private float raySphereRadius = 0.1f;

        [Tooltip("Layers considered interactable. Defaults to everything.")]
        [SerializeField] private LayerMask interactableLayer = ~0;

        [Header("References")]
        [SerializeField] private Camera m_camera;

        public InteractableBase m_interactable;

        private void OnEnable()
        {
            if (inputReader != null)
                inputReader.InteractEvent.AddListener(HandleInteractInput);
        }

        private void OnDisable()
        {
            if (inputReader != null)
                inputReader.InteractEvent.RemoveListener(HandleInteractInput);
        }

        private void Update()
        {
            CheckForInteractables();
        }

        private void CheckForInteractables()
        {
            Ray ray = new(m_camera.transform.position, m_camera.transform.forward);
            bool hit = Physics.SphereCast(ray, raySphereRadius, out RaycastHit hitInfo, rayDistance, interactableLayer);

            if (hit && hitInfo.transform.TryGetComponent(out InteractableBase foundInteractable))
            {
                if (m_interactable != foundInteractable)
                    m_interactable = foundInteractable;
            }
            else if (m_interactable != null)
            {
                ResetInteractable();
            }

            Debug.DrawRay(ray.origin, ray.direction * rayDistance, hit ? Color.green : Color.red);
        }

        private void HandleInteractInput()
        {
            if (m_interactable != null)
                m_interactable.OnInteracted();

            ResetInteractable();
        }

        private void ResetInteractable()
        {
            m_interactable = null;
        }
    }
}
