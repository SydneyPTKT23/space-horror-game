using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipPowerSystem : MonoBehaviour
    {
        [Header("Power Settings")]
        [Tooltip("Total available ship power in kilowatts (kW).")]
        public float totalCapacityKW = 10f;

        [Header("Subsystems")]
        public List<Subsystem> subsystems = new();

        /// <summary>
        /// Returns total power usage from all active subsystems.
        /// </summary>
        public float GetUsedPowerkW()
        {
            float total = 0f;
            foreach (var subsystem in subsystems)
            {
                total += subsystem.GetCurrentPowerDraw();
            }
            return total;
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

        public void Tick(float deltaTime)
        {
            foreach (var subsystem in subsystems)
            {
                subsystem.Tick(deltaTime);
            }
        }

        /// <summary>
        /// Returns remaining unused power capacity.
        /// </summary>
        public float GetRemainingCapacity()
        {
            return Mathf.Max(0, totalCapacityKW - GetUsedPowerkW());
        }

        /// <summary>
        /// Checks if a subsystem can be turned on based on available power.
        /// </summary>
        public bool CanEnableSubsystem(Subsystem s)
        {
            if (s == null || s.isOnline) return false;
            return GetUsedPowerkW() + s.requiredKW <= totalCapacityKW;
        }

        /// <summary>
        /// Attempts to enable a subsystem if power permits.
        /// </summary>
        public bool TryEnableSubsystem(Subsystem s)
        {
            if (CanEnableSubsystem(s))
            {
                s.isOnline = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Disables a subsystem.
        /// </summary>
        public void DisableSubsystem(Subsystem s)
        {
            if (s != null)
                s.isOnline = false;
        }

        /// <summary>
        /// Turns off all subsystems (e.g. emergency shutdown).
        /// </summary>
        public void ShutdownAll()
        {
            foreach (var s in subsystems)
            {
                s.isOnline = false;
            }
        }
    }

    [System.Serializable]
    public class Subsystem
    {
        public string displayName;
        public float requiredKW;
        public bool isOnline;

        private float fluctuationTimer;
        private float fluctuationOffset;

        public Subsystem(string name, float powerRequired)
        {
            displayName = name;
            requiredKW = powerRequired;
            isOnline = false;
            fluctuationOffset = Random.Range(0f, 100f); // random offset so not all fluctuate in sync
        }

        // Call this periodically from an Update method or externally
        public void Tick(float deltaTime)
        {
            fluctuationTimer += deltaTime;
        }

        public float GetCurrentPowerDraw()
        {
            if (!isOnline) return 0f;

            float fluctuation = Mathf.PerlinNoise(fluctuationOffset, fluctuationTimer * 0.2f); // smooth change
            float offset = Mathf.Lerp(-0.1f, 0.1f, fluctuation); // ±10% variation
            return requiredKW * (1f + offset);
        }
    }
}