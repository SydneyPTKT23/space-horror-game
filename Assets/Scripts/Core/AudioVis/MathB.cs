using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public static class MathB
    {
        public static List<GameObject> ShapesOfGameObjects(GameObject pf, float radius, int amount, Transform test)
        {
            List<GameObject> objects = new(amount);

            for (int i = 0; i < amount; i++)
            {
                float wallPos = -radius + (i * radius / amount * 2);
                Vector3 pos = new(wallPos, test.position.y, test.position.z);
                // Instantiate
                GameObject obj = Object.Instantiate(pf, pos, Quaternion.identity);
                objects.Add(obj);
            }

            return objects;
        }
    }
}