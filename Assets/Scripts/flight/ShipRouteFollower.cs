using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipRouteFollower : MonoBehaviour
    {
        [Header("References")]
        public UICursorWaypointSystem waypointSystem;

        [Header("Movement Settings")]
        public float maxSpeed = 5.0f;
        public float acceleration = 2.0f;
        public float rotationSpeed = 2.0f;
        public float stoppingDistance = 0.5f;
        public float decelerationDistance = 3.0f;

        private readonly List<Vector3> route = new();
        private int currentWaypointIndex = 0;
        private bool isFollowingRoute = false;
        private bool isPaused = false;
        private float currentSpeed = 0f;

        private void Update()
        {
            if (waypointSystem == null) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                StartFollowingRoute();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }

            if (!isFollowingRoute || isPaused) return;

            FollowRoute();
        }

        private void StartFollowingRoute()
        {
            /*var waypoints = waypointSystem.GetWorldWaypoints();
            if (waypoints == null || waypoints.Count == 0)
            {
                isFollowingRoute = false;
                return;
            }

            // Cache route once
            route.Clear();
            route.AddRange(waypoints);

            currentWaypointIndex = 0;
            currentSpeed = 0f;
            isFollowingRoute = true;
            isPaused = false;*/
        }

        private void TogglePause()
        {
            if (isFollowingRoute)
            {
                isPaused = !isPaused;
            }
        }

        private void FollowRoute()
        {
            if (currentWaypointIndex >= route.Count)
            {
                isFollowingRoute = false;
                currentSpeed = 0f;
                return;
            }

            Vector3 currentPosition = transform.position;
            Vector3 target = route[currentWaypointIndex];

            Vector3 direction = target - currentPosition;
            float distance = direction.magnitude;
            if (distance == 0f) // Avoid division by zero and rotation issues
            {
                currentWaypointIndex++;
                return;
            }

            Vector3 directionNormalized = direction / distance;

            // Smoothly rotate toward the target
            Quaternion targetRotation = Quaternion.LookRotation(directionNormalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Calculate target speed with smooth deceleration near the waypoint
            float targetSpeed = maxSpeed;
            if (distance <= decelerationDistance)
            {
                targetSpeed = Mathf.Lerp(0f, maxSpeed, distance / decelerationDistance);
            }

            // Accelerate or decelerate toward target speed
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // Move forward
            transform.position += currentSpeed * Time.deltaTime * transform.forward;

            // Advance waypoint if close enough
            if (distance <= stoppingDistance)
            {
                currentWaypointIndex++;
            }
        }
    }
}