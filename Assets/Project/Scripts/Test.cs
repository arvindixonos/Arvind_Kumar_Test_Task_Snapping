using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    public class Test : MonoBehaviour
    {
        public Renderer myRenderer;

        public Renderer[] childs;

        public void Awake()
        {
            
        }

        public void OnDrawGizmos()
        {
            Bounds mainBounds = myRenderer.bounds;
            DebugExtension.DrawBounds(mainBounds, Color.red);

            List<Vector3> points = new List<Vector3>();
            points.Add(mainBounds.center);
            points.Add(mainBounds.min);
            points.Add(mainBounds.max);


            foreach (Renderer child in childs)
            {
                Bounds childBounds = child.bounds;

                points.Add(childBounds.max);
                points.Add(childBounds.min);
            }

            Bounds calcBounds = GeometryUtility.CalculateBounds(points.ToArray(), Matrix4x4.identity);
            DebugExtension.DrawBounds(calcBounds, Color.blue);
        }

        public void Update()
        {
            
        }
    }
}
