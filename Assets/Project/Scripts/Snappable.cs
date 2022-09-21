using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using DG.Tweening;

namespace MyScripts
{
    /// <summary>
    /// Rest state of the snappable.
    /// </summary>
    public class Snappable_State_Rest : IState
    {
        /// <summary>
        /// <para>Changes material color to rest color.</para>
        /// <para>Disables kinematic(useGravity = true, isKinemactic = false).</para>
        /// <para>Disables Trigger(isTrigger = false).</para>
        /// </summary>
        /// <param name="targetObject">Typically the owner instance of this state.</param>
        public void EnterState(object targetObject)
        {
            var targetSnappable = targetObject as Snappable;

            targetSnappable.ChangeColorToRest();
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


    /// <summary>
    /// Snapped state of the snappable.
    /// </summary>
    public class Snappable_State_Snapped : IState
    {
        /// <summary>
        /// <para>Changes material color to snapped color.</para>
        /// <para>Disables kinematic(useGravity = false, isKinemactic = true).</para>
        /// <para>Disables Trigger(isTrigger = true).</para>
        /// </summary>
        /// <param name="targetObject">Typically the owner instance of this state.</param>
        public void EnterState(object targetObject)
        {
            var targetSnappable = targetObject as Snappable;

            targetSnappable.ChangeColorToSnapped();
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

    /// <summary>
    /// FollowMouse state of the snappable. Follows the mouse point on a flat plane.
    /// </summary>
    public class Snappable_State_FollowMouse : IState
    {
        /// <summary>
        /// <para>Changes material color to selected color.</para>
        /// </summary>
        /// <param name="targetObject">Typically the owner instance of this state.</param>
        public void EnterState(object targetObject)
        {
            var targetSnappable = targetObject as Snappable;

            targetSnappable.ChangeColorToSelected();
        }

        public void ExitState(object targetObject)
        {
        }

        /// <summary>
        /// Updates the bounds as new objects are snapped and makes the snappable follow the mouse pointer.
        /// </summary>
        /// <param name="targetObject">Typically the owner instance of this state.</param>
        public void UpdateState(object targetObject)
        {
            var targetSnappable = targetObject as Snappable;

            targetSnappable.UpdateBounds();
            targetSnappable.FollowMouse();
        }
    }

    /// <summary>
    /// Represents the snappables in our application. At the moment there are 4 types of snappables(Cube, Sphere, Cyclinder, Capsule).
    /// </summary>
    public class Snappable : MonoBehaviour
    {
        // Consts for state names.
        public const string STATE_REST = "Rest";
        public const string STATE_SNAPPED = "Snapped";
        public const string STATE_FOLLOW_MOUSE = "Follow Mouse";

        // Lerp speed for following the mouse.
        public float lerpSpeed = 5f;

        // Respective colors for the snappable to change according to the state it is in.
        public Color selectedColor;
        public Color restColor;
        public Color snappedColor;

        private Vector3 targetPosition;
        private RaycastHit hit;

        // Layer of the table
        public LayerMask tableLayer;

        // Layer of the snappable
        public LayerMask snappableLayer;

        // My present bounds. Get updated if any object gets snapped.
        private Bounds myBounds;

        // Accessor for knowing whether the object is snapped.
        public bool IsSnapped 
        {
            get
            {
                return currentStateName.Equals(STATE_SNAPPED);
            }
        }

        // Accessor for knowing whether the object is following mouse.
        public bool IsFollowingMouse
        {
            get
            {
                return currentStateName.Equals(STATE_FOLLOW_MOUSE);
            }
        }


        // Our state machine. Manages our states.
        private StateMachine stateMachine;

        // Name of the current state we are in.
        [SerializeField]
        private string currentStateName;

        // References of our Rigidbody, collider and renderer.
        private Rigidbody myRigidBody;
        private Collider myCollider;
        private Renderer myRenderer;

        // List of snappables attached to this snappable.
        private List<Snappable> childSnappables = new List<Snappable>();

        private void Awake()
        {
            // Populating the references.
            myRigidBody = GetComponent<Rigidbody>();
            myCollider = GetComponent<Collider>();  
            myRenderer = GetComponentInChildren<Renderer>();

            // Initializing the statemachine
            InitStateMachine();
        }

        private void Update()
        {
            // Update our state machine
            stateMachine.UpdateCurrentState();
        }

        #region STATE MACHINE

        /// <summary>
        /// Creates a new statemachine instance for this object and adds the states rest, snapped and followmouse to its list.
        /// </summary>
        private void InitStateMachine()
        {
            stateMachine = new StateMachine(this);
            stateMachine.AddState(STATE_REST, new Snappable_State_Rest());
            stateMachine.AddState(STATE_SNAPPED, new Snappable_State_Snapped());
            stateMachine.AddState(STATE_FOLLOW_MOUSE, new Snappable_State_FollowMouse());

            // Set the initial state to rest.
            ChangeState(STATE_REST);
        }

        /// <summary>
        /// Changes the state of the object.
        /// </summary>
        /// <param name="stateName">Name of the state to change to.</param>
        private void ChangeState(string stateName)
        {
            currentStateName = stateMachine.ChangeState(stateName);
        }

        #endregion

        #region SNAPPABLES

        /// <summary>
        /// Snaps to the closest point on the bounds of the parent snappable and changes state to snapped.
        /// </summary>
        /// <param name="parentSnappable">Parent Snappable to snap to.</param>
        private void SnapToSnappable(Snappable parentSnappable)
        {
            // Update our bounds.
            UpdateBounds();

            // Get the max point and min point on bounds.
            var positionFromMaxBoundPos = parentSnappable.myBounds.ClosestPoint(myBounds.max);
            var positionFromMinBoundPos = parentSnappable.myBounds.ClosestPoint(myBounds.min);
            var distancefromMaxBoundPos = Vector3.SqrMagnitude(positionFromMaxBoundPos - parentSnappable.myBounds.center);
            var distancefromMinBoundPos = Vector3.SqrMagnitude(positionFromMinBoundPos - parentSnappable.myBounds.center);

            // Snap to the closest position to the parent snappable.
            transform.parent = parentSnappable.transform;
            transform.position = distancefromMaxBoundPos < distancefromMinBoundPos ? positionFromMaxBoundPos : positionFromMinBoundPos;

            // Change state to snapped.
            ChangeState(STATE_SNAPPED);
        }

        /// <summary>
        /// Get the closest snappable from the list of colliders hit.
        /// </summary>
        /// <param name="colliders">List of colliders hit using OverlapBox.</param>
        /// <returns>returns closest snappable if found or else null. </returns>
        private Snappable GetClosestSnappable(Collider[] colliders)
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

        /// <summary>
        /// OverlapBox(AABB) using our bounds on the snappable layer.
        /// </summary>
        public void SnapSnappables()
        {
            //DebugExtension.DebugBounds(myBounds, Color.yellow);
            Collider[] colliders = Physics.OverlapBox(transform.position, myBounds.extents, Quaternion.identity, snappableLayer);
            
            if (colliders.Length > 0)
            {
                // Find the closest snappable.
                var closestSnappable = GetClosestSnappable(colliders);

                // If found, snap it to us and add it to the child snappables list.
                if (closestSnappable != null)
                {
                    closestSnappable.SnapToSnappable(this);
                    childSnappables.Add(closestSnappable);
                }
            }
        }

        /// <summary>
        /// Clear all child snappables from the list.
        /// </summary>
        public void ClearChildSnappables()
        {
            childSnappables.Clear();
        }

        /// <summary>
        /// Release us if we are snapped to any parent and make our state to rest.
        /// </summary>
        public void ReleaseFromSnappedObject()
        {
            transform.parent = null;

            ChangeState(STATE_REST);
        }

        #endregion

        #region KINEMATIC AND TRIGGERS

        /// <summary>
        /// Make the rigibody kinematic and doesn't obey gravity.
        /// </summary>
        public void EnableKinematic()
        {
            myRigidBody.useGravity = false;
            myRigidBody.isKinematic = true;
        }

        /// <summary>
        /// Make the rigibody dynamic and obey gravity.
        /// </summary>
        public void DisableKinemactic()
        {
            myRigidBody.useGravity = true;
            myRigidBody.isKinematic = false;
        }

        /// <summary>
        /// Sets isTrigger to true.
        /// </summary>
        public void EnableTrigger()
        {
            myCollider.isTrigger = true;
        }

        /// <summary>
        /// Sets isTrigger to false.
        /// </summary>
        public void DisableTrigger()
        {
            myCollider.isTrigger = false;
        }
        #endregion

        #region BOUNDS

        /// <summary>
        /// Returns our bounds from the renderer.
        /// </summary>
        /// <returns>Bounds of the renderer.</returns>
        public Bounds GetRendererBounds()
        {
            return myRenderer.bounds;   
        }

        /// <summary>
        /// If no snappable is attached to us, then just returns to bounds of the renderer or 
        /// else calculate the bounds including the child snappables using GeometryUtility.CalculateBounds function.
        /// </summary>
        public void UpdateBounds()
        {
            if(childSnappables.Count == 0)
            {
                myBounds = GetRendererBounds();
            }
            else
            {
                var mainBounds = myRenderer.bounds;
                //DebugExtension.DrawBounds(mainBounds, Color.red);

                List<Vector3> points = new List<Vector3>();
                points.Add(mainBounds.center);
                points.Add(mainBounds.min);
                points.Add(mainBounds.max);

                foreach (var childSnappable in childSnappables)
                {
                    var childBounds = childSnappable.myRenderer.bounds;

                    // Add min and max point of the child bounds.
                    points.Add(childBounds.max);
                    points.Add(childBounds.min);
                }

                // Calculate new bounds.
                myBounds = GeometryUtility.CalculateBounds(points.ToArray(), Matrix4x4.identity);
                //DebugExtension.DrawBounds(myBounds, Color.blue);
            }
        }

        #endregion

        #region FOLLOW MOUSE AND MOUSE EVENTS

        /// <summary>
        /// Ray cast to the table layer(flat plane) and put the current snappable on it.
        /// </summary>
        public void FollowMouse()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 1000f, tableLayer))
            {
                targetPosition = hit.point;

                LerpToTargetPosition();
            }
        }

        /// <summary>
        /// Lerps the snappable to the targetPosition from its current position.
        /// </summary>
        private void LerpToTargetPosition()
        {
            var distancetocover = (targetPosition - transform.position).sqrMagnitude;
            if (distancetocover < 0.01f)
                return;

            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Monobehaviour event on mouse down.
        /// </summary>
        private void OnMouseDown()
        {
            // If already snapped, raise event "Mouse Down Snapped".
            if (IsSnapped)
            {
                EventManager.Instance.RaiseLogicEvent("Mouse Down Snapped");
                return;
            }

            // If not snapped, raise event "Mouse Down Snappable".
            EventManager.Instance.RaiseLogicEvent("Mouse Down Snappable");

            // If not snapped, make us the current selected snappable.
            EventManager.Instance.RaiseLogicEvent("Selected Snappable", this);

            // Change state to follow mouse.
            ChangeState(STATE_FOLLOW_MOUSE);
        }

        /// <summary>
        /// Monobehaviour event on mouse down.
        /// </summary>
        private void OnMouseUp()
        {
            // If already snapped, raise event "Mouse Up Snapped".
            if (IsSnapped)
            {
                EventManager.Instance.RaiseLogicEvent("Mouse Up Snapped");
                return;
            }

            // If not snapped, raise event "Mouse Up Snappable".
            EventManager.Instance.RaiseLogicEvent("Mouse Up Snappable");

            // If not snapped, raise event "Deselect Current Snappable".
            EventManager.Instance.RaiseLogicEvent("Deselect Current Snappable");

            // Change state to rest.
            ChangeState(STATE_REST);
        }

        /// <summary>
        /// Monobehaviour event on mouse over.
        /// </summary>
        private void OnMouseOver()
        {
            // If following mouse, return.
            if (IsFollowingMouse)
            {
                return;
            }

            // If Snapped, raise event "Mouse Hover Snapped".
            if (IsSnapped)
            {
                EventManager.Instance.RaiseLogicEvent("Mouse Hover Snapped");
                return;
            }
            // If not Snapped, raise event "Mouse Hover Snappable".
            else
            {
                EventManager.Instance.RaiseLogicEvent("Mouse Hover Snappable");
               
            }
        }

        /// <summary>
        /// Monobehaviour event on mouse exit.
        /// </summary>
        private void OnMouseExit()
        {
            // If following mouse, return.
            if (IsFollowingMouse)
            {
                return;
            }

            // If not following mouse, raise event "Mouse Exit".
            EventManager.Instance.RaiseLogicEvent("Mouse Exit");
        }

        #endregion

        #region OBJECT COLORS

        /// <summary>
        /// Change Material Color to rest Color
        /// </summary>
        public void ChangeColorToRest()
        {
            myRenderer.material.DOKill();
            myRenderer.material.DOColor(restColor, 0.5f);
        }

        /// <summary>
        /// Change Material Color to snapped Color
        /// </summary>
        public void ChangeColorToSnapped()
        {
            myRenderer.material.DOKill();
            myRenderer.material.DOColor(snappedColor, 0.5f);
        }

        /// <summary>
        /// Change Material Color to selected Color
        /// </summary>
        public void ChangeColorToSelected()
        {
            myRenderer.material.DOKill();
            myRenderer.material.DOColor(selectedColor, 0.5f);
        }

        #endregion
    }
}
