using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;

namespace MyScripts
{
    public class SimpleSphereObject : Singleton<SimpleSphereObject>
    {
        public float lerpSpeed = 5f;

        public LayerMask snappableLayer;

        private float maxScanRadius = 5f;
        private Vector3 targetPosition = Vector3.zero;

        private SphereCollider mySphereCollider;
        private Bounds myBounds;


        public override void Awake()
        {
            base.Awake();

            mySphereCollider = GetComponentInChildren<SphereCollider>();
            myBounds = GetComponentInChildren<Renderer>().bounds;

            targetPosition = transform.position;
        }

        public void SetTargetPosition(Vector3 targetPosition)
        {
            this.targetPosition = targetPosition;
        }

        public void LerpToTargetPosition()
        {
            float distancetocover = (targetPosition - transform.position).sqrMagnitude;
            if (distancetocover < 0.01f)
                return;

            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        }

        public Snappable GetClosestSnappable(Collider[] colliders, out Vector3 closestPointOnBounds)
        {
            Snappable closestSnappable = null;
            float closestLength = float.MaxValue;
            closestPointOnBounds = Vector3.zero;

            foreach (var currentCollider in colliders)
            {
                var currentSnappable = currentCollider.GetComponent<Snappable>();

                if (currentSnappable == null || currentSnappable.IsSnapped)
                    continue;

                var currentClosestPointOnBounds = mySphereCollider.ClosestPointOnBounds(currentCollider.transform.position);

                float currentLength = Vector3.Distance(currentCollider.transform.position, currentClosestPointOnBounds);

                if(currentLength < closestLength)
                {
                    closestLength = currentLength;
                    closestSnappable = currentSnappable;
                    closestPointOnBounds = currentClosestPointOnBounds;
                }
            }

            return closestSnappable;
        }

        public void ReleaseAllSnappables()
        {
            Snappable[] snappables = transform.GetComponentsInChildren<Snappable>();

            foreach(Snappable snappable in snappables)
            {
                if(snappable.IsSnapped)
                {
                    snappable.ReleaseFromSnappedObject();
                }
            }
        }

        public void SnapSnappables()
        {
            DebugExtension.DebugWireSphere(transform.position, Color.yellow, maxScanRadius);
            Collider[] colliders = Physics.OverlapSphere(transform.position, maxScanRadius, snappableLayer);

            if (colliders.Length > 0)
            {
                var closestPointOnBounds = Vector3.zero;
                var closestSnappable = GetClosestSnappable(colliders, out closestPointOnBounds);

                if(closestSnappable != null)
                {
                    closestSnappable.SnapToSimpleObject(transform, transform.InverseTransformPoint(closestPointOnBounds));
                }
            }
        }

        public void Update()
        {
            LerpToTargetPosition();
        }
    }
}
