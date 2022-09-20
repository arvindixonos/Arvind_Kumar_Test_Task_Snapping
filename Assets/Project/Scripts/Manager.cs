using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


namespace MyScripts
{
    public class Manager : Singleton<Manager>
    {
        public Camera targetCamera;

        public float YOffsetHitPosition = 1f;

        public Snappable[] snappablePrefabs;
        private Snappable[] snappableObjectPool;

        public Transform[] snappableSpawnPoints;

        public Transform simpleObject;

        public LayerMask hittableLayer;

        public override void Awake()
        {
            base.Awake();

            if(targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            PopulateSnappablesOnTable();
        }

        public void PopulateSnappablesOnTable()
        {
            int numSnappableSpawnPoints = snappableSpawnPoints.Length;

            snappableObjectPool = new Snappable[numSnappableSpawnPoints];

            int numSnappableObjectPrefabs = snappablePrefabs.Length;

            Random.InitState((int)System.DateTime.Now.Ticks);

            for (int i = 0; i < numSnappableSpawnPoints; i++)
            {
                Snappable randomSnappable = snappablePrefabs[Random.Range(0, numSnappableObjectPrefabs - 1)];

                Quaternion randomRotation = Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

                snappableObjectPool[i] = Snappable.Instantiate(randomSnappable, snappableSpawnPoints[i].position, randomRotation);
            }
        }

        public void Update()
        {
            RaycastHit hit;
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f))
            {
                if(hittableLayer == (hittableLayer | (1 << hit.transform.gameObject.layer)))
                {
                    simpleObject.position = new Vector3(hit.point.x, hit.point.y + YOffsetHitPosition, hit.point.z);

                    //print("Hit Table Only");
                }
                else
                {
                    //print("Hit Object: " + hit.transform.name);
                }
            }
            else
            {
                //print("NOT HIT ME");
            }
        }

    }
}
