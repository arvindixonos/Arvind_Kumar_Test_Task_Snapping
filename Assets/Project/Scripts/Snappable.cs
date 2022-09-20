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

        public void SnapToSnappable(Transform parentTransform, Vector3 snapPositionLocal)
        {
            transform.parent = parentTransform;
            transform.localPosition = snapPositionLocal;

            ChangeState(STATE_SNAPPED);
        }

        public Snappable GetClosestSnappable(Collider[] colliders, out Vector3 closestPointOnBounds)
        {
            Snappable closestSnappable = null;
            var closestLength = float.MaxValue;
            closestPointOnBounds = Vector3.zero;

            foreach (var currentCollider in colliders)
            {
                if (currentCollider == myCollider)
                    continue;

                var currentSnappable = currentCollider.GetComponent<Snappable>();

                if (currentSnappable == null || currentSnappable.IsSnapped)
                    continue;

                var currentClosestPointOnBounds = myCollider.ClosestPointOnBounds(currentCollider.transform.position);

                var currentLength = Vector3.Distance(currentCollider.transform.position, currentClosestPointOnBounds);

                if (currentLength < closestLength)
                {
                    closestLength = currentLength;
                    closestSnappable = currentSnappable;
                    closestPointOnBounds = currentClosestPointOnBounds;
                }
            }

            return closestSnappable;
        }


        public void SnapSnappables()
        {
            DebugExtension.DebugBounds(myBounds, Color.yellow);
            Collider[] colliders = Physics.OverlapBox(transform.position, myBounds.extents, Quaternion.identity, snappableLayer);

            if (colliders.Length > 0)
            {
                var closestPointOnBounds = Vector3.zero;
                var closestSnappable = GetClosestSnappable(colliders, out closestPointOnBounds);

                if (closestSnappable != null)
                {
                    closestSnappable.SnapToSnappable(transform, transform.InverseTransformPoint(closestPointOnBounds));
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

        public Bounds GetRendererLocalBounds()
        {
            return myRenderer.localBounds;
        }

        public void UpdateBounds()
        {
            if(childSnappables.Count == 0)
            {
                myBounds = GetRendererBounds();
            }
            else
            {
                List<Vector3> allPoints = new List<Vector3>();

                foreach(var childSnappable in childSnappables)
                {
                    Bounds rendererBounds = childSnappable.GetRendererLocalBounds();
                    allPoints.Add(rendererBounds.max);
                    allPoints.Add(rendererBounds.min);
                }

                myBounds = GeometryUtility.CalculateBounds(allPoints.ToArray(), transform.localToWorldMatrix);
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
