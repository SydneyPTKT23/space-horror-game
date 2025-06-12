using UnityEngine;
using TMPro;
using System.Collections.Generic;
using static SLC.SpaceHorror.ShipPowerSystem;

namespace SLC.SpaceHorror
{
    public class PowerMonitorUI : MonoBehaviour
    {
        [Header("References")]
        public ShipPowerSystem powerSystem;

        public TMP_Text reactorOutputText;
        public TMP_Text activeLoadText;
        public TMP_Text availableReserveText;
        public TMP_Text gridUtilizationText;
        public TMP_Text stabilityIndexText;
        public TMP_Text emergencyFeedText;

        [Header("UI - Subsystem List")]
        public GameObject subsystemEntryPrefab;
        public Transform subsystemListContainer;

        [Header("Warning UI Elements")]
        public GameObject warningPanel;
        public TMP_Text warningText;
        public Color warningColor = Color.red;
        public Color normalColor = Color.white;

        public float blinkFrequency = 2f;

        private float blinkTimer = 0f;
        private bool isWarningActive = false;

        private readonly List<GameObject> activeSubsystemUIEntries = new();

        private void OnEnable()
        {
            if (powerSystem != null)
            {
                BuildSubsystemEntries();
                UpdateSummaryTexts();
            }
        }

        private void Update()
        {
            if (powerSystem == null) return;

            powerSystem.Tick(Time.deltaTime);

            UpdateSummaryTexts();
            UpdateWarnings();

            if (isWarningActive)
            {
                blinkTimer += Time.deltaTime * blinkFrequency * 2 * Mathf.PI;
                float alpha = (Mathf.Sin(blinkTimer) + 1f) / 2f;
                Color c = warningText.color;
                c.a = Mathf.Lerp(0.3f, 1f, alpha);
                warningText.color = c;
            }
            else
            {
                blinkTimer = 0f;
                warningText.color = warningColor;
            }
        }


        public void BuildSubsystemEntries()
        {
            ClearSubsystemList();

            foreach (var subsystem in powerSystem.subsystems)
            {
                GameObject entryGO = Instantiate(subsystemEntryPrefab, subsystemListContainer);
                entryGO.SetActive(true);

                var entryUI = entryGO.GetComponent<SubsystemEntryUI>();
                if (entryUI != null)
                {
                    entryUI.Initialize(subsystem, powerSystem, this);
                }

                activeSubsystemUIEntries.Add(entryGO);
            }
        }

        public void UpdateSummaryTexts()
        {
            reactorOutputText.text = $"REACTOR OUTPUT     {powerSystem.totalCapacityKW:0.00} kW";
            activeLoadText.text = $"ACTIVE LOAD        {powerSystem.GetUsedPowerkW():0.00} kW";
            availableReserveText.text = $"AVAILABLE RESERVE  {powerSystem.GetAvailableReserveKW():0.00} kW";
            gridUtilizationText.text = $"GRID UTILIZATION   {powerSystem.GetGridUtilizationPercent():0.0}%";
            stabilityIndexText.text = $"STABILITY INDEX    {powerSystem.GetStabilityIndex()}";
            emergencyFeedText.text = $"EMERGENCY FEED     {(powerSystem.EmergencyFeedEngaged ? "ENGAGED" : "DISENGAGED")}";
        }

        public void UpdateWarnings()
        {
            if (powerSystem == null || warningPanel == null || warningText == null)
                return;

            var warnings = powerSystem.GetActiveWarnings();

            isWarningActive = warnings.Count > 0;

            if (!isWarningActive)
            {
                warningPanel.SetActive(false);
                warningText.text = "";
                warningText.color = normalColor;
                return;
            }

            warningPanel.SetActive(true);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (var warning in warnings)
            {
                switch (warning)
                {
                    case PowerWarningType.Overload:
                        sb.AppendLine("WARNING: Power Overload Imminent");
                        break;
                    case PowerWarningType.EmergencyFeedActive:
                        sb.AppendLine("EMERGENCY FEED ENGAGED");
                        break;
                    case PowerWarningType.SubsystemFailure:
                        sb.AppendLine("CRITICAL SUBSYSTEM OFFLINE");
                        break;
                    case PowerWarningType.UnstableGrid:
                        sb.AppendLine("GRID STABILITY AT RISK");
                        break;
                }
            }
            warningText.text = sb.ToString();
        }


        public void RefreshSubsystemEntries()
        {
            foreach (var entryGO in activeSubsystemUIEntries)
            {
                var entryUI = entryGO.GetComponent<SubsystemEntryUI>();
                if (entryUI != null)
                {
                    entryUI.UpdateEntry();
                }
            }
        }

        private void ClearSubsystemList()
        {
            foreach (var go in activeSubsystemUIEntries)
            {
                Destroy(go);
            }
            activeSubsystemUIEntries.Clear();
        }
    }
}
