using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SLC.SpaceHorror
{
    public class SubsystemEntryUI : MonoBehaviour
    {
        [Header("References")]
        public TMP_Text subsystemNameText;
        public TMP_Text statusText;
        public TMP_Text powerDrawText;
        public Button toggleButton;
        public TMP_Text toggleButtonLabel;

        private Subsystem subsystem;
        private ShipPowerSystem powerSystem;
        private PowerMonitorUI monitor;

        public void Initialize(Subsystem s, ShipPowerSystem ps, PowerMonitorUI monitorUI)
        {
            subsystem = s;
            powerSystem = ps;
            monitor = monitorUI;

            toggleButton.onClick.RemoveAllListeners();
            toggleButton.onClick.AddListener(OnTogglePressed);

            UpdateEntry();
        }

        private void OnTogglePressed()
        {
            if (subsystem.isOnline)
            {
                powerSystem.DisableSubsystem(subsystem);
            }
            else
            {
                powerSystem.TryEnableSubsystem(subsystem);
            }

            UpdateEntry();
            monitor.RefreshSubsystemEntries();  // Refresh button colors/texts etc.
        }


        public void UpdateEntry()
        {
            if (subsystem == null) return;

            subsystemNameText.text = subsystem.displayName.ToUpper();
            statusText.text = subsystem.isOnline ? "ONLINE" : "OFFLINE";
            statusText.color = subsystem.isOnline ? Color.green : Color.gray;

            powerDrawText.text = $"{subsystem.GetCurrentPowerDraw():0.0} kW";

            toggleButtonLabel.text = subsystem.isOnline ? "TURN OFF" : "TURN ON";
        }
    }
}