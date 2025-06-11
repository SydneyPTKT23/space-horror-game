using UnityEngine;

namespace SLC.SpaceHorror
{
    [CreateAssetMenu(fileName = "MinimapBounds", menuName = "SLC/Minimap Bounds")]
    public class MinimapBoundsData : ScriptableObject
    {
        public Vector2 worldMin = new(-100, -100);
        public Vector2 worldMax = new(100, 100);

        public Vector2 WorldSize => worldMax - worldMin;
        public Vector2 WorldCenter => (worldMin + worldMax) * 0.5f;
    }
}