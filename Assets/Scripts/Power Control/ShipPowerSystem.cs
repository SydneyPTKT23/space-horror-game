using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipPowerSystem : MonoBehaviour
    {
        public enum PowerWarningType
        {
            Overload,
            EmergencyFeedActive,
            SubsystemFailure,
            UnstableGrid
        }

        [Header("Power Settings")]
        [Tooltip("Total available ship power in kilowatts (kW).")]
        public float totalCapacityKW = 10f;

        [Header("Subsystems")]
        public List<Subsystem> subsystems = new();

        private bool emergencyFeedEngaged = false;
        public bool EmergencyFeedEngaged => emergencyFeedEngaged;

        public float GetUsedPowerkW()
        {
            float total = 0f;
            foreach (var subsystem in subsystems)
            {
                total += subsystem.GetCurrentPowerDraw();
            }
            return total;
        }

        public float GetAvailableReserveKW()
        {
            return Mathf.Max(0f, totalCapacityKW - GetUsedPowerkW());
        }

        public float GetGridUtilizationPercent()
        {
            return Mathf.Clamp01(GetUsedPowerkW() / totalCapacityKW) * 100f;
        }

        public void Tick(float deltaTime)
        {
            foreach (var subsystem in subsystems)
            {
                subsystem.Tick(deltaTime);
            }

            UpdateEmergencyFeedStatus();
        }

        public float GetUsedPowerRTG()
        {
            float used = 0f;
            foreach (var subsystem in subsystems)
            {
                if (subsystem.isOnline)
                    used += subsystem.GetCurrentPowerDraw();
            }
            return used;
        }

        public float GetRemainingCapacity()
        {
            return Mathf.Max(0, totalCapacityKW - GetUsedPowerkW());
        }

        public string GetStabilityIndex()
        {
            float utilization = GetGridUtilizationPercent();

            if (emergencyFeedEngaged) return "CRITICAL";

            if (utilization < 70f) return "NOMINAL";
            if (utilization < 90f) return "HIGH";
            return "OVERLOAD";
        }

        public bool CanEnableSubsystem(Subsystem s)
        {
            if (s == null || s.isOnline) return false;

            float maxExpectedDraw = s.requiredKW * 1.1f; // account for +10% fluctuation
            return GetUsedPowerkW() + maxExpectedDraw <= totalCapacityKW;
        }

        public bool TryEnableSubsystem(Subsystem s)
        {
            if (CanEnableSubsystem(s))
            {
                s.isOnline = true;
                return true;
            }
            return false;
        }

        public void DisableSubsystem(Subsystem s)
        {
            if (s != null)
                s.isOnline = false;
        }

        public void ShutdownAll()
        {
            foreach (var s in subsystems)
            {
                s.isOnline = false;
            }
        }

        public void UpdateEmergencyFeedStatus()
        {
            if (GetGridUtilizationPercent() > 100f)
            {
                emergencyFeedEngaged = true;
            }
            else
            {
                emergencyFeedEngaged = false;
            }
        }

        public List<PowerWarningType> GetActiveWarnings()
        {
            List<PowerWarningType> warnings = new();

            // Overload or dangerously close
            if (GetGridUtilizationPercent() >= 95f)
            {
                warnings.Add(PowerWarningType.Overload);
            }

            // Emergency feed engaged
            if (EmergencyFeedEngaged)
            {
                warnings.Add(PowerWarningType.EmergencyFeedActive);
            }

            // Subsystem failure (e.g. marked offline or in fault state - placeholder for now)
            foreach (var subsystem in subsystems)
            {
                if (subsystem.isCritical && !subsystem.isOnline)
                {
                    warnings.Add(PowerWarningType.SubsystemFailure);
                    break;
                }
            }

            // Stability index degraded
            string stability = GetStabilityIndex();
            if (stability != "NOMINAL")
            {
                warnings.Add(PowerWarningType.UnstableGrid);
            }

            return warnings;
        }

    }

    [System.Serializable]
    public class Subsystem
    {
        public string displayName;
        public float requiredKW;
        public bool isOnline;
        public bool isCritical = false;

        private float fluctuationTimer;
        private readonly float fluctuationOffset;

        public Subsystem(string name, float powerRequired)
        {
            displayName = name;
            requiredKW = powerRequired;
            isOnline = false;
            fluctuationOffset = Random.Range(0f, 100f); // random offset so not all fluctuate in sync
        }

        public void Tick(float deltaTime)
        {
            fluctuationTimer += deltaTime;
        }

        public float GetCurrentPowerDraw()
        {
            if (!isOnline) return 0f;

            float baseDraw = requiredKW;

            // Low-frequency stable hum
            float stableOscillation = Mathf.PerlinNoise(fluctuationOffset, fluctuationTimer * 0.1f);
            float stableOffset = Mathf.Lerp(-0.02f, 0.02f, stableOscillation); // ±2%

            // Tiny high-frequency jitter
            float jitter = Mathf.PerlinNoise(fluctuationOffset + 100f, fluctuationTimer * 5f);
            float jitterOffset = Mathf.Lerp(-0.005f, 0.005f, jitter); // ±0.5%

            return baseDraw * (1f + stableOffset + jitterOffset);
        }
    }
}