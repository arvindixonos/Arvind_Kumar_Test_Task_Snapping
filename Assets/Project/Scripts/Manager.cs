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

        public Snappable[] snappablePrefabs;
        private Snappable[] snappableObjectPool;

        public Transform[] snappableSpawnPoints;

        private Vector3 simpleObjecttargetPosition;
        private RaycastHit hit;

        public LayerMask hittableLayer;

        public Snappable currentSelectedSnappable;

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

        void ReleaseAllSnappables()
        {
            Snappable[] snappables = FindObjectsOfType<Snappable>();
            
            foreach(Snappable snappable in snappables)
            {
                snappable.ClearChildSnappables();
                snappable.ReleaseFromSnappedObject();
            }
        }

        public void Update()
        {
            // Shift Input
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                if(currentSelectedSnappable != null)
                {
                    currentSelectedSnappable.SnapSnappables();
                }
            }

            // Alt Input
            if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
            {
                ReleaseAllSnappables();
            }
        }

    }
}
