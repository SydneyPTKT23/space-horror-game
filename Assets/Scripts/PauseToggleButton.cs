using SLC.SpaceHorror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseToggleButtonText : MonoBehaviour
{
    [SerializeField] private Button pauseToggleButton;
    [SerializeField] private TMP_Text buttonText; // UnityEngine.UI.Text component on the button

    // Reference to your ship movement controller or pause manager
    [SerializeField] private ShipRouteFollower shipMovement;

    private void Start()
    {
        if (pauseToggleButton == null)
            pauseToggleButton = GetComponent<Button>();

        if (buttonText == null && pauseToggleButton != null)
            buttonText = pauseToggleButton.GetComponentInChildren<TMP_Text>();

        UpdateButtonText();
    }

    public void TogglePause()
    {
        if (shipMovement == null)
        {
            Debug.LogWarning("ShipMovementController reference is missing.");
            return;
        }

        // Toggle pause state
        shipMovement.TogglePause();

        UpdateButtonText();
    }

    private void UpdateButtonText()
    {
        if (shipMovement == null || buttonText == null) return;

        //buttonText.text = shipMovement.IsPaused ? "Resume" : "Pause";
    }
}