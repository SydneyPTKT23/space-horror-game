using UnityEngine;

namespace SLC.SpaceHorror
{
    [CreateAssetMenu(fileName = "MinimapBounds", menuName = "SLC/Minimap Bounds")]
    public class MinimapBoundsData : ScriptableObject
    {
        public Vector2 worldMin = new Vector2(-100, -100);  // XZ space
        public Vector2 worldMax = new Vector2(100, 100);
    }
}