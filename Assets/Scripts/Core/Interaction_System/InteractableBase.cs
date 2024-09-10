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

        public virtual void OnInteract()
        {
            Debug.Log("Interacted with: " + gameObject.name);
        }
    }
}