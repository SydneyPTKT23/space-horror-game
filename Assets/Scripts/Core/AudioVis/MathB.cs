using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SLC.SpaceHorror.Core
{
    public static class MathB
    {
        public static List<GameObject> ShapesOfGameObjects(GameObject pf, float radius, int amount)
        {
            List<GameObject> objects = new(amount);

            for (int i = 0; i < amount; i++)
            {
                float wallPos = -radius + i * radius / amount * 2;
                var pos = new Vector3(wallPos, 0, 1);
                // Instantiate
                GameObject obj = Object.Instantiate(pf, pos, Quaternion.identity) as GameObject;
                objects.Add(obj);
            }

            return objects;
        }
    }
}