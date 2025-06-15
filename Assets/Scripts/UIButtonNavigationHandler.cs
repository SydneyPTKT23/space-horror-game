using SLC.SpaceHorror.Input;
using UnityEngine;
using UnityEngine.UI;

namespace SLC.SpaceHorror.UI
{
    public class UIButtonNavigationHandler : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputReader inputReader;

        [Header("Navigation Settings")]
        [SerializeField] private GameObject firstSelectedButton;
        [SerializeField] private Color normalButtonColor = Color.white;
        [SerializeField] private Color highlightButtonColor = Color.yellow;
        [SerializeField] private float inputCooldown = 0.2f;

        public bool IsActive { get; private set; }

        private GameObject virtualSelectedButton;
        private GameObject lastHighlightedButton;

        private float lastInputTime;
        private Button[] cachedButtons;

        private void OnEnable()
        {
            CacheButtons();
            ActivateNavigation();
        }

        private void OnDisable()
        {
            Deactivate();
        }

        private void Update()
        {
            if (!IsActive) return;

            HandleNavigationInput();

            if (inputReader.MonitorActionPressedThisFrame)
            {
                InvokeCurrentSelectedButton();
            }

            UpdateSelectionHighlight();
        }

        public void ActivateNavigation()
        {
            IsActive = true;

            if (virtualSelectedButton == null && firstSelectedButton != null)
                virtualSelectedButton = firstSelectedButton;

            UpdateSelectionHighlight();
        }

        public void Deactivate()
        {
            IsActive = false;
            ClearSelection();
        }

        private void HandleNavigationInput()
        {
            if (Time.time - lastInputTime < inputCooldown)
                return;

            Vector2 nav = inputReader.NavigateInput;
            if (nav == Vector2.zero) return;

            if (!virtualSelectedButton.TryGetComponent(out Selectable current)) return;

            Selectable next = null;

            if (nav.y > 0) next = current.FindSelectableOnUp();
            else if (nav.y < 0) next = current.FindSelectableOnDown();
            else if (nav.x < 0) next = current.FindSelectableOnLeft();
            else if (nav.x > 0) next = current.FindSelectableOnRight();

            if (next != null)
            {
                virtualSelectedButton = next.gameObject;
                lastInputTime = Time.time;
            }
        }

        private void UpdateSelectionHighlight()
        {
            foreach (Button btn in cachedButtons)
            {
                if (btn.TryGetComponent(out Image img))
                    img.color = (btn.gameObject == virtualSelectedButton) ? highlightButtonColor : normalButtonColor;
            }

            lastHighlightedButton = virtualSelectedButton;
        }

        private void InvokeCurrentSelectedButton()
        {
            if (!IsActive || virtualSelectedButton == null) return;

            if (virtualSelectedButton.TryGetComponent(out Button button))
                button.onClick.Invoke();
        }

        private void ClearSelection()
        {
            if (lastHighlightedButton != null)
            {
                if (lastHighlightedButton.TryGetComponent(out Image img))
                    img.color = normalButtonColor;
            }

            virtualSelectedButton = null;
            lastHighlightedButton = null;
        }

        private void CacheButtons() => cachedButtons = GetComponentsInChildren<Button>(true);

        public void SetVirtualSelection(GameObject buttonObj)
        {
            virtualSelectedButton = buttonObj;
            UpdateSelectionHighlight();
        }

        public GameObject GetVirtualSelection() => virtualSelectedButton;
    }
}