using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipRouteFollower : MonoBehaviour
    {
        [Header("References")]
        public UICursorWaypointSystem waypointSystem;

        [Header("Movement Settings")]
        public float speed = 5f;
        public float rotationSpeed = 2f;
        public float stoppingDistance = 0.5f;

        private List<Vector3> route = new List<Vector3>();
        private int currentWaypointIndex = 0;
        private bool isFollowingRoute = false;
        private bool isPaused = false;

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                StartFollowingRoute();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }

            if (isFollowingRoute && !isPaused)
            {
                FollowRoute();
            }
        }

        void StartFollowingRoute()
        {
            route = new List<Vector3>(waypointSystem.GetWorldWaypoints());
            currentWaypointIndex = 0;

            if (route.Count > 0)
            {
                isFollowingRoute = true;
                isPaused = false;
            }
        }

        void TogglePause()
        {
            if (isFollowingRoute)
            {
                isPaused = !isPaused;
            }
        }

        void FollowRoute()
        {
            if (currentWaypointIndex >= route.Count)
            {
                isFollowingRoute = false;
                return;
            }

            Vector3 target = route[currentWaypointIndex];
            Vector3 direction = (target - transform.position).normalized;

            // Smoothly rotate toward the target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Move forward
            transform.position += transform.forward * speed * Time.deltaTime;

            // Check if close enough to target
            if (Vector3.Distance(transform.position, target) <= stoppingDistance)
            {
                currentWaypointIndex++;
            }
        }
    }
}