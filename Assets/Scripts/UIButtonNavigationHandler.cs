using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SLC.SpaceHorror.UI
{
    public class UIButtonNavigationHandler : MonoBehaviour
    {
        [Tooltip("First button to select when UI button navigation activates")]
        public GameObject firstSelectedButton;
        private GameObject previouslySelectedButton;

        private Color normalButtonColor = Color.white;
        private Color highlightButtonColor = Color.yellow;

        private bool isActive = false;

        private SLC.SpaceHorror.Input.InputReader inputReader;

        private void Awake()
        {
            inputReader = FindObjectOfType<SLC.SpaceHorror.Input.InputReader>();
            if (inputReader == null)
                Debug.LogWarning("UIButtonNavigationHandler: InputReader not found in scene.");
        }

        private void OnEnable()
        {
            ActivateNavigation();
        }

        private void OnDisable()
        {
            ClearSelection();
        }

        private void Update()
        {
            if (!isActive) return;

            UpdateSelectionHighlight();

            if (inputReader != null && inputReader.MonitorActionPressedThisFrame)
            {
                InvokeCurrentSelectedButton();
            }
        }

        private void ActivateNavigation()
        {
            if (firstSelectedButton != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedButton);
            else
                EventSystem.current.SetSelectedGameObject(null);

            isActive = true;
        }

        private void UpdateSelectionHighlight()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != previouslySelectedButton)
            {
                ResetButtonColor(previouslySelectedButton);
                HighlightButton(currentSelected);
                previouslySelectedButton = currentSelected;
            }
        }

        private void ResetButtonColor(GameObject buttonObj)
        {
            if (buttonObj == null) return;

            var img = buttonObj.GetComponent<Image>();
            if (img != null)
                img.color = normalButtonColor;
        }

        private void HighlightButton(GameObject buttonObj)
        {
            if (buttonObj == null) return;

            var img = buttonObj.GetComponent<Image>();
            if (img != null)
                img.color = highlightButtonColor;
        }

        private void InvokeCurrentSelectedButton()
        {
            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected == null) return;

            var button = currentSelected.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }
        }

        private void ClearSelection()
        {
            ResetButtonColor(previouslySelectedButton);
            previouslySelectedButton = null;
            EventSystem.current.SetSelectedGameObject(null);
            isActive = false;
        }
    }
}
