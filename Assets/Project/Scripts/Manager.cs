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

        private Vector3 simpleObjecttargetPosition;
        private RaycastHit hit;

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
            var numSnappableSpawnPoints = snappableSpawnPoints.Length;

            snappableObjectPool = new Snappable[numSnappableSpawnPoints];

            var numSnappableObjectPrefabs = snappablePrefabs.Length;

            Random.InitState((int)System.DateTime.Now.Ticks);

            for (int i = 0; i < numSnappableSpawnPoints; i++)
            {
                var randomSnappable = snappablePrefabs[Random.Range(0, numSnappableObjectPrefabs - 1)];

                var randomRotation = Quaternion.Euler(Random.Range(-180f, 180f), Random.Range(-180f, 180f), Random.Range(-180f, 180f));

                snappableObjectPool[i] = Snappable.Instantiate(randomSnappable, snappableSpawnPoints[i].position, randomRotation);
            }
        }

        public void Update()
        {
            var ray = targetCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f, hittableLayer))
            {
                simpleObjecttargetPosition.x = hit.point.x;
                simpleObjecttargetPosition.y = hit.point.y + YOffsetHitPosition;
                simpleObjecttargetPosition.z = hit.point.z;

                SimpleSphereObject.Instance.SetTargetPosition(simpleObjecttargetPosition);
            }

            // Shift Input
            if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
            {
                SimpleSphereObject.Instance.SnapSnappables();
            }

            // Alt Input
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                SimpleSphereObject.Instance.ReleaseAllSnappables();
            }
        }

    }
}
