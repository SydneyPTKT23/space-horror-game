using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipRouteFollower : MonoBehaviour
    {
        [Header("References")]
        public UICursorWaypointSystem waypointSystem;

        [Header("Movement Settings")]
        public float maxSpeed = 5f;
        public float acceleration = 2f;
        public float rotationSpeed = 2f;
        public float stoppingDistance = 0.5f;
        public float decelerationDistance = 3f;

        private List<Vector3> route = new List<Vector3>();
        private int currentWaypointIndex = 0;
        private bool isFollowingRoute = false;
        private bool isPaused = false;

        private float currentSpeed = 0f;

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
            currentSpeed = 0f;

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
                currentSpeed = 0f;
                return;
            }

            Vector3 target = route[currentWaypointIndex];
            Vector3 direction = (target - transform.position);
            float distance = direction.magnitude;
            direction.Normalize();

            // Smooth rotation toward target
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            // Adjust speed based on distance
            float targetSpeed = maxSpeed;
            if (distance <= decelerationDistance)
            {
                targetSpeed = Mathf.Lerp(0, maxSpeed, distance / decelerationDistance);
            }

            // Accelerate/decelerate smoothly
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            // Move ship
            transform.position += transform.forward * currentSpeed * Time.deltaTime;

            // Advance waypoint if reached
            if (distance <= stoppingDistance)
            {
                currentWaypointIndex++;
            }
        }
    }
}
