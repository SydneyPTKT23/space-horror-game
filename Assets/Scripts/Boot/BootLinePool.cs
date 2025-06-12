using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class BootLinePool : MonoBehaviour
    {
        [Header("Prefab & Pool Size")]
        [Tooltip("Prefab for boot line UI elements")]
        public GameObject bootLinePrefab;

        [Tooltip("Initial number of boot lines to instantiate in pool")]
        public int initialPoolSize = 30;

        // Internal pool storage
        private readonly Queue<GameObject> pool = new();

        // Singleton instance for easy access
        public static BootLinePool Instance { get; private set; }

        private void Awake()
        {
            // Enforce singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Optional: Uncomment if you want this object to persist between scenes
            // DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Pre-instantiates the pool with inactive boot line objects.
        /// Call once at startup.
        /// </summary>
        public void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = Instantiate(bootLinePrefab, transform);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        /// <summary>
        /// Gets an inactive boot line from the pool or instantiates a new one if empty.
        /// Sets its parent and activates it.
        /// </summary>
        /// <param name="parent">Transform to parent the boot line under</param>
        /// <returns>Active boot line GameObject</returns>
        public GameObject GetLine(Transform parent)
        {
            GameObject line;

            if (pool.Count > 0)
            {
                line = pool.Dequeue();
            }
            else
            {
                line = Instantiate(bootLinePrefab);
            }

            line.transform.SetParent(parent, false);
            line.SetActive(true);
            return line;
        }

        /// <summary>
        /// Returns a boot line back to the pool and deactivates it.
        /// </summary>
        public void ReturnLine(GameObject line)
        {
            line.SetActive(false);
            pool.Enqueue(line);
        }

        /// <summary>
        /// Destroys all objects currently in the pool and clears it.
        /// Use with caution.
        /// </summary>
        public void ClearPool()
        {
            foreach (var obj in pool)
            {
                if (obj != null)
                    Destroy(obj);
            }
            pool.Clear();
        }
    }
}