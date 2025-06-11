using TMPro;
using UnityEngine;

namespace SLC.SpaceHorror
{
    public class RouteStatusDisplay : MonoBehaviour
    {
        [Header("References")]
        public ShipRouteFollower routeFollower;
        public TextMeshProUGUI distanceText;
        public TextMeshProUGUI timeText;

        private void Update()
        {
            if (routeFollower == null || !routeFollower.IsFollowingRoute)
            {
                distanceText.text = "Distance: --";
                timeText.text = "ETA: --";
                return;
            }

            float remainingDistance = routeFollower.GetRemainingDistance();
            float currentSpeed = routeFollower.GetCurrentSpeed();

            distanceText.text = $"Distance: {remainingDistance:F1} m";

            if (currentSpeed > 0.1f)
            {
                float estimatedTime = remainingDistance / currentSpeed;
                timeText.text = $"ETA: {FormatTime(estimatedTime)}";
            }
            else
            {
                timeText.text = "ETA: --";
            }
        }

        private string FormatTime(float timeInSeconds)
        {
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}
