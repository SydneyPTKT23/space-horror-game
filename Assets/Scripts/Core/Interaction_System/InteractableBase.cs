using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Settings")]
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private string tooltipMessage = "Interact";

        public bool IsInteractable => isInteractable;
        public string TooltipMessage => tooltipMessage;

        public virtual void OnInteracted()
        {
            Debug.Log("Interacted with: " + gameObject.name);
        }
    }
}