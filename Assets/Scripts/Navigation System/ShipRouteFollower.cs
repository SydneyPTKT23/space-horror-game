using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipRouteFollower : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UICursorWaypointSystem waypointSystem;

        [Header("Movement Settings")]
        [SerializeField] private float maxSpeed = 5.0f;
        [SerializeField] private float acceleration = 2.0f;
        [SerializeField] private float rotationSpeed = 2.0f;
        [SerializeField] private float stoppingDistance = 0.5f;
        [SerializeField] private float decelerationDistance = 3.0f;

        private readonly List<Vector3> route = new();
        private int currentWaypointIndex = 0;
        private bool isFollowingRoute = false;
        private bool isPaused = false;
        private float currentSpeed = 0f;

        private void Update()
        {
            if (waypointSystem == null) return;

            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
                StartFollowingRoute();

            if (UnityEngine.Input.GetKeyDown(KeyCode.P))
                TogglePause();

            if (isFollowingRoute && !isPaused)
                FollowRoute();
        }

        private void StartFollowingRoute()
        {
            IReadOnlyList<Vector3> waypoints = waypointSystem.GetWorldWaypoints();
            if (waypoints == null || waypoints.Count == 0)
            {
                isFollowingRoute = false;
                return;
            }

            route.Clear();
            route.AddRange(waypoints);
            currentWaypointIndex = 0;
            currentSpeed = 0f;
            isFollowingRoute = true;
            isPaused = false;
        }

        private void TogglePause()
        {
            if (isFollowingRoute)
                isPaused = !isPaused;
        }

        private void FollowRoute()
        {
            if (currentWaypointIndex >= route.Count)
            {
                StopRoute();
                return;
            }

            Vector3 currentPos = transform.position;
            Vector3 target = route[currentWaypointIndex];
            Vector3 toTarget = target - currentPos;
            float distance = toTarget.magnitude;

            if (distance < Mathf.Epsilon)
            {
                AdvanceToNextWaypoint();
                return;
            }

            float dt = Time.deltaTime;
            Vector3 direction = toTarget / distance;

            RotateToward(direction, dt);
            AdjustSpeed(distance, dt);
            MoveForward(dt);

            if (distance <= stoppingDistance)
                AdvanceToNextWaypoint();
        }

        private void RotateToward(Vector3 direction, float dt)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * dt);
        }

        private void AdjustSpeed(float distance, float dt)
        {
            float targetSpeed = (distance <= decelerationDistance)
                ? Mathf.Lerp(0f, maxSpeed, distance / decelerationDistance)
                : maxSpeed;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * dt);
        }

        private void MoveForward(float dt)
        {
            transform.position += currentSpeed * dt * transform.forward;
        }

        private void AdvanceToNextWaypoint()
        {
            currentWaypointIndex++;
        }

        private void StopRoute()
        {
            isFollowingRoute = false;
            currentSpeed = 0f;
        }

        public bool IsFollowingRoute => isFollowingRoute && !isPaused;
        public float GetCurrentSpeed() => currentSpeed;

        public float GetRemainingDistance()
        {
            if (!isFollowingRoute || currentWaypointIndex >= route.Count)
                return 0f;

            float total = Vector3.Distance(transform.position, route[currentWaypointIndex]);
            for (int i = currentWaypointIndex; i < route.Count - 1; i++)
                total += Vector3.Distance(route[i], route[i + 1]);

            return total;
        }
    }
}