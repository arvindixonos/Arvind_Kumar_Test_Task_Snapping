using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using DG.Tweening;
using UnityEngine.Analytics;

namespace MyScripts
{
    /// <summary>
    /// Inside Restzone state of the snappable.
    /// </summary>
    public class Snappable_State_In_RestZone : IState
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

            targetSnappable.SetInRestZoneParams();
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

            //targetSnappable.ChangeColorToSnapped();
            //targetSnappable.SetOutRestZoneParams();
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

            targetSnappable.SetFollowMouseParams();
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
        public const string STATE_IN_RESTZONE = "In Rest Zone";
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
        private float seperation = 0f;

        // Layer of the table
        public LayerMask tableLayer;

        // Layer of the snappable
        public LayerMask snappableLayer;

        // Name of the restzone tag
        public string restZoneTagName;
        private Vector3 restZonePosition;

        // My present bounds. Get updated if any object gets snapped.
        private Bounds myBounds;

        // Accessor for knowing whether the object is inside the rest zone.
        public bool IsInsideRestZone
        {
            get
            {
                return currentStateName.Equals(STATE_IN_RESTZONE);
            }
        }

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


        public void InitSnappable(Vector3 startRestZonePosition)
        {
            // Populating the references.
            myRigidBody = GetComponent<Rigidbody>();
            myCollider = GetComponent<Collider>();
            myRenderer = GetComponentInChildren<Renderer>();

            myCollider.contactOffset = 0.5f;

            restZonePosition = startRestZonePosition;

            // Initializing the statemachine
            InitStateMachine();
        }

        private void Update()
        {
            // Update our state machine
            if (stateMachine != null)
            {
                stateMachine.UpdateCurrentState();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            print("Entering Trigger: " + other.name);

            if(other.CompareTag(restZoneTagName))
            {
                restZonePosition = other.transform.position;
                ChangeState(STATE_IN_RESTZONE);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            //print("Staying Trigger: " + other.name);
        }

        private void OnTriggerExit(Collider other)
        {
            print("Exiting Trigger: " + other.name);
        }

        private void OnCollisionEnter(Collision collision)
        {
            print("Entering Collision with: " + collision.gameObject.name);
        }

        private void OnCollisionStay(Collision collision)
        {
            ContactPoint[] contacts = new ContactPoint[collision.contactCount];

            int numContacts = collision.GetContacts(contacts);

            seperation = 0f;
            foreach(ContactPoint contactPoint in contacts)
            {
                if(contactPoint.separation < seperation)
                {
                    print("Setting Seperation");

                    seperation = contactPoint.separation;
                }
            }

            print("Staying Collision with: " + collision.gameObject.name);
        }

        private void OnCollisionExit(Collision collision)
        {
            print("Exiting Collision with: " + collision.gameObject.name);
        }

        #region STATE MACHINE

        /// <summary>
        /// Creates a new statemachine instance for this object and adds the states rest, snapped and followmouse to its list.
        /// </summary>
        private void InitStateMachine()
        {
            stateMachine = new StateMachine(this);
            stateMachine.AddState(STATE_IN_RESTZONE, new Snappable_State_In_RestZone());
            stateMachine.AddState(STATE_SNAPPED, new Snappable_State_Snapped());
            stateMachine.AddState(STATE_FOLLOW_MOUSE, new Snappable_State_FollowMouse());

            // Set the initial state to rest.
            ChangeState(STATE_IN_RESTZONE);
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

            ChangeState(STATE_IN_RESTZONE);
        }

        #endregion

        #region PARAMETERS FOR STATES

        /// <summary>
        /// Set snappable params when the state is Out rest zone.        
        /// /// </summary>
        public void SetFollowMouseParams()
        {
            ChangeColorToSelected();

            //myRigidBody.isKinematic = false;
        }

        /// <summary>
        /// Set snappable params when the state in In rest zone.
        /// </summary>
        public void SetInRestZoneParams()
        {
            ChangeColorToRest();

            myRigidBody.isKinematic = true;
            myRigidBody.velocity = Vector3.zero;
            myRigidBody.angularVelocity = Vector3.zero;

            transform.position = restZonePosition;
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

                if(seperation != 0f)
                {
                    print("Using Seperation");

                    targetPosition -= hit.normal * seperation;
                }

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

            //// Change state to rest.
            //ChangeState(STATE_IN_RESTZONE);
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
