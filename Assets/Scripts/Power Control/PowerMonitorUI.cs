using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace SLC.SpaceHorror
{
    public class PowerMonitorUI : MonoBehaviour
    {
        [Header("References")]
        public ShipPowerSystem powerSystem;

        public TMP_Text totalDrawText;
        public TMP_Text availableRTGText;

        public GameObject subsystemEntryPrefab;
        public Transform subsystemListContainer;

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

            // Update summary texts frequently for fluctuation effect
            UpdateSummaryTexts();
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
            float usedKW = powerSystem.GetUsedPowerkW();
            float totalKW = powerSystem.totalCapacityKW;
            float availableRTG = powerSystem.GetUsedPowerRTG();

            totalDrawText.text = $"LOAD: {usedKW:0.0} kW / {totalKW:0.0} kW";
            availableRTGText.text = $"RESERVE: {availableRTG:F2} RTGs";
        }

        public void RefreshSubsystemEntries()
        {
            // Called when a subsystem status changes to refresh UI entries (buttons, colors, etc.)
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