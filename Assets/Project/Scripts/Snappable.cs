using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyScripts
{
    public class Snappable_State_Rest : IState
    {
        public void EnterState(object targetObject)
        {
            Snappable targetSnappable = targetObject as Snappable;

            targetSnappable.DisableKinemactic();
            targetSnappable.DisableTrigger();
        }

        public void ExitState(object targetObject)
        {
        }

        public void UpdateState(object targetObject)
        {
        }
    }

    public class Snappable_State_Snapped : IState
    {
        public void EnterState(object targetObject)
        {
            Snappable targetSnappable = targetObject as Snappable;

            targetSnappable.EnableKinematic();
            targetSnappable.EnableTrigger();
        }

        public void ExitState(object targetObject)
        {
        }

        public void UpdateState(object targetObject)
        {
        }
    }

    public class Snappable_State_FollowMouse : IState
    {
        public void EnterState(object targetObject)
        {
        }

        public void ExitState(object targetObject)
        {
        }

        public void UpdateState(object targetObject)
        {
            Snappable targetSnappable = targetObject as Snappable;

            targetSnappable.UpdateBounds();
            targetSnappable.FollowMouse();
        }
    }


    public class Snappable : MonoBehaviour
    {
        public const string STATE_REST = "Rest";
        public const string STATE_SNAPPED = "Snapped";
        public const string STATE_FOLLOW_MOUSE = "Follow Mouse";

        public float lerpSpeed = 5f;

        private Vector3 targetPosition;
        private RaycastHit hit;
        public LayerMask hittableLayer;
        public LayerMask snappableLayer;

        private Bounds myBounds;

        public bool IsSnapped 
        {
            get
            {
                return currentStateName.Equals(STATE_SNAPPED);
            }
        }

        public StateMachine stateMachine;

        [SerializeField]
        private string currentStateName;

        private Rigidbody myRigidBody;
        private Collider myCollider;
        private Renderer myRenderer;

        private List<Snappable> childSnappables = new List<Snappable>();

        private void Awake()
        {
            myRigidBody = GetComponent<Rigidbody>();
            myCollider = GetComponent<Collider>();  
            myRenderer = GetComponentInChildren<Renderer>();

            InitStateMachine();
        }

        public void InitStateMachine()
        {
            stateMachine = new StateMachine(this);
            stateMachine.AddState(STATE_REST, new Snappable_State_Rest());
            stateMachine.AddState(STATE_SNAPPED, new Snappable_State_Snapped());
            stateMachine.AddState(STATE_FOLLOW_MOUSE, new Snappable_State_FollowMouse());

            ChangeState(STATE_REST);
        }

        private void ChangeState(string stateName)
        {
            currentStateName = stateMachine.ChangeState(stateName);
        }

        public void SnapToSnappable(Snappable parentSnappable)
        {
            UpdateBounds();

            var positionFromMaxBoundPos = parentSnappable.myBounds.ClosestPoint(myBounds.max);
            var positionFromMinBoundPos = parentSnappable.myBounds.ClosestPoint(myBounds.min);
            var distancefromMaxBoundPos = Vector3.SqrMagnitude(positionFromMaxBoundPos - parentSnappable.myBounds.center);
            var distancefromMinBoundPos = Vector3.SqrMagnitude(positionFromMinBoundPos - parentSnappable.myBounds.center);

            transform.parent = parentSnappable.transform;
            transform.position = distancefromMaxBoundPos < distancefromMinBoundPos ? positionFromMaxBoundPos : positionFromMinBoundPos;

            ChangeState(STATE_SNAPPED);
        }

        public Snappable GetClosestSnappable(Collider[] colliders)
        {
            Snappable closestSnappable = null;
            var closestLength = float.MaxValue;

            foreach (var currentCollider in colliders)
            {
                if (currentCollider == myCollider)
                    continue;

                var currentSnappable = currentCollider.GetComponent<Snappable>();

                if (currentSnappable == null || currentSnappable.IsSnapped)
                    continue;

                var currentLength = Vector3.Distance(currentCollider.transform.position, myBounds.center);

                if (currentLength < closestLength)
                {
                    closestLength = currentLength;
                    closestSnappable = currentSnappable;
                }
            }

            return closestSnappable;
        }


        public void SnapSnappables()
        {
            //DebugExtension.DebugBounds(myBounds, Color.yellow);
            Collider[] colliders = Physics.OverlapBox(transform.position, myBounds.extents, Quaternion.identity, snappableLayer);

            if (colliders.Length > 0)
            {
                var closestPointOnBounds = Vector3.zero;
                var closestSnappable = GetClosestSnappable(colliders);

                if (closestSnappable != null)
                {
                    closestSnappable.SnapToSnappable(this);
                    childSnappables.Add(closestSnappable);
                }
            }
        }

        public void ClearChildSnappables()
        {
            childSnappables.Clear();
        }

        public void ReleaseFromSnappedObject()
        {
            transform.parent = null;

            ChangeState(STATE_REST);
        }

        public void Update()
        {
            stateMachine.UpdateCurrentState();
        }

        public void EnableKinematic()
        {
            myRigidBody.useGravity = false;
            myRigidBody.isKinematic = true;
        }

        public void DisableKinemactic()
        {
            myRigidBody.useGravity = true;
            myRigidBody.isKinematic = false;
        }

        public void EnableTrigger()
        {
            myCollider.isTrigger = true;
        }

        public void DisableTrigger()
        {
            myCollider.isTrigger = false;
        }

        public Bounds GetRendererBounds()
        {
            return myRenderer.bounds;   
        }

        public void UpdateBounds()
        {
            if(childSnappables.Count == 0)
            {
                myBounds = GetRendererBounds();
            }
            else
            {
                Bounds mainBounds = myRenderer.bounds;
                //DebugExtension.DrawBounds(mainBounds, Color.red);

                List<Vector3> points = new List<Vector3>();
                points.Add(mainBounds.center);
                points.Add(mainBounds.min);
                points.Add(mainBounds.max);

                foreach (Snappable childSnappable in childSnappables)
                {
                    Bounds childBounds = childSnappable.myRenderer.bounds;

                    points.Add(childBounds.max);
                    points.Add(childBounds.min);
                }

                myBounds = GeometryUtility.CalculateBounds(points.ToArray(), Matrix4x4.identity);
                //DebugExtension.DrawBounds(myBounds, Color.blue);
            }
        }

        public void FollowMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f, hittableLayer))
            {
                targetPosition = hit.point;

                LerpToTargetPosition();
            }
        }

        public void OnMouseDown()
        {
            if (currentStateName.Equals(STATE_SNAPPED))
                return;

            Manager.Instance.currentSelectedSnappable = this;

            ChangeState(STATE_FOLLOW_MOUSE);
        }

        public void OnMouseUp()
        {
            if (currentStateName.Equals(STATE_SNAPPED))
                return;

            Manager.Instance.currentSelectedSnappable = null;

            ChangeState(STATE_REST);
        }

        public void LerpToTargetPosition()
        {
            float distancetocover = (targetPosition - transform.position).sqrMagnitude;
            if (distancetocover < 0.01f)
                return;

            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        }
    }
}
