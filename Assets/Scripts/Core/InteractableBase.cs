using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public class InteractableBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Settings")]
        [SerializeField] private bool holdToInteract = false;
        [SerializeField] private float holdDuration = 1.0f;

        [Space]
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private string tooltipMessage = "Interact";


        public float HoldDuration => holdDuration;

        public bool HoldToInteract => holdToInteract;
        public bool IsInteractable => isInteractable;

        public string TooltipMessage => tooltipMessage;

        public virtual void OnInteract()
        {
            Debug.Log("Interacted with: " + gameObject.name);
        }
    }
}