using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class ShipRouteFollower : MonoBehaviour
    {
        [Header("References")]
        public UICursorWaypointSystem waypointSystem;
        public float speed = 5f;
        public float stoppingDistance = 0.5f;

        private List<Vector3> route = new List<Vector3>();
        private int currentWaypointIndex = 0;
        private bool isFollowingRoute = false;

        void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                StartFollowingRoute();
            }

            if (isFollowingRoute)
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
            Vector3 moveDir = (target - transform.position).normalized;
            transform.position += moveDir * speed * Time.deltaTime;

            if (Vector3.Distance(transform.position, target) <= stoppingDistance)
            {
                currentWaypointIndex++;
            }
        }
    }
}