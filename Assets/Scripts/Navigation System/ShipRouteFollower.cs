using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SLC.SpaceHorror
{
    [System.Serializable]
    public struct RoutePoint
    {
        public Vector3 position;
        public float waitTime;
    }

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
        [SerializeField] private float rotationDeadZone = 2f;

        private Vector3 driftVelocity;
        [SerializeField] private float driftDampTime = 3.0f; // seconds to come to a stop
        private float driftTimer = 0f;

        [Header("Events")]
        public UnityEvent OnRouteStarted;
        public UnityEvent OnRoutePaused;
        public UnityEvent OnRouteResumed;
        public UnityEvent OnRouteStopped;
        public UnityEvent<Vector3> OnWaypointReached;

        public bool IsFollowingRoute => isFollowingRoute && !isPaused;

        private readonly Queue<RoutePoint> routeQueue = new();
        private RoutePoint? currentTarget;
        private bool isFollowingRoute = false;
        private bool isPaused = false;
        private float currentSpeed = 0f;
        private float idleDriftPhase = 0f;

        private void Update()
        {
            if (!isFollowingRoute) return;

            if (isPaused)
            {
                SimulateDriftWhilePaused(Time.deltaTime);
            }
            else if (currentTarget.HasValue)
            {
                FollowToTarget(currentTarget.Value);
            }
            else if (routeQueue.Count > 0)
            {
                currentTarget = routeQueue.Dequeue();
            }
            else
            {
                StopRoute();
            }
        }

        private void FollowToTarget(RoutePoint target)
        {
            Vector3 toTarget = target.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance <= stoppingDistance)
            {
                OnWaypointReached?.Invoke(target.position);
                currentTarget = null;
                StartCoroutine(WaitAtWaypoint(target.waitTime));
                return;
            }

            Vector3 direction = toTarget.normalized;
            RotateToward(direction, Time.deltaTime);

            AdjustSpeed(distance, Time.deltaTime, direction);
            MoveForward(Time.deltaTime);
        }

        private void RotateToward(Vector3 direction, float dt)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float angleDiff = Quaternion.Angle(transform.rotation, targetRotation);

            if (angleDiff > rotationDeadZone)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * dt);
        }

        private void AdjustSpeed(float distance, float dt, Vector3 toTarget)
        {
            float angleToTarget = Vector3.Angle(transform.forward, toTarget.normalized);
            float angleFactor = Mathf.Clamp01(1f - angleToTarget / 90f); // reduce speed on sharp angles

            float targetSpeed = (distance <= decelerationDistance)
                ? Mathf.Lerp(0f, maxSpeed * angleFactor, distance / decelerationDistance)
                : maxSpeed * angleFactor;

            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * dt);
        }

        private void MoveForward(float dt)
        {
            transform.position += currentSpeed * dt * transform.forward;
        }

        private IEnumerator WaitAtWaypoint(float duration)
        {
            if (duration > 0f)
                yield return new WaitForSeconds(duration);
        }

        public void StartFollowingRoute()
        {
            var worldPoints = waypointSystem.GetWorldWaypoints();
            if (worldPoints == null || worldPoints.Count == 0) return;

            StopRoute();

            foreach (var wp in worldPoints)
                routeQueue.Enqueue(new RoutePoint { position = wp, waitTime = 0 });

            currentTarget = null;
            isPaused = false;
            isFollowingRoute = true;
            currentSpeed = 0f;
            idleDriftPhase = 0f;

            OnRouteStarted?.Invoke();
        }

        public void TogglePause()
        {
            if (!isFollowingRoute) return;

            isPaused = !isPaused;

            if (isPaused)
            {
                driftVelocity = transform.forward * currentSpeed;
                driftTimer = 0f;
                OnRoutePaused?.Invoke();
            }
            else
            {
                currentSpeed = driftVelocity.magnitude;
                driftVelocity = Vector3.zero;
                OnRouteResumed?.Invoke();
            }
        }

        private void StopRoute()
        {
            isFollowingRoute = false;
            isPaused = false;
            currentTarget = null;
            routeQueue.Clear();
            currentSpeed = 0f;
            idleDriftPhase = 0f;

            OnRouteStopped?.Invoke();
        }

        private void LateUpdate()
        {
            if (!isFollowingRoute && Mathf.Approximately(currentSpeed, 0f))
                SimulateIdleDrift(Time.deltaTime);
        }

        private void SimulateIdleDrift(float dt)
        {
            idleDriftPhase += dt;
            float amplitude = 0.02f;
            float frequency = 0.3f;
            transform.position += amplitude * dt * Mathf.Sin(idleDriftPhase * frequency) * transform.up;
        }

        private void SimulateDriftWhilePaused(float dt)
        {
            if (driftVelocity.sqrMagnitude <= 0.0001f) return;

            transform.position += driftVelocity * dt;

            driftTimer += dt;
            float t = Mathf.Clamp01(driftTimer / driftDampTime);
            driftVelocity = Vector3.Lerp(driftVelocity, Vector3.zero, t);
        }

        public float GetCurrentSpeed() => currentSpeed;

        public float GetRemainingDistance()
        {
            float total = 0f;

            if (currentTarget.HasValue)
                total += Vector3.Distance(transform.position, currentTarget.Value.position);

            Vector3 last = currentTarget?.position ?? transform.position;

            foreach (var point in routeQueue)
            {
                total += Vector3.Distance(last, point.position);
                last = point.position;
            }

            return total;
        }
    }
}
