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

            if (IsFollowingMouse)
            {
                if (other.CompareTag(restZoneTagName))
                {
                    if(!(other.bounds.Contains(myCollider.bounds.max) && other.bounds.Contains(myCollider.bounds.min)))
                    {
                        restZonePosition = other.transform.position;

                        ChangeState(STATE_IN_RESTZONE);
                    }
                }
            }
        }

        private void OnTriggerStay(Collider other)
        {
            //print("Staying Trigger: " + other.name);
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsFollowingMouse)
            {
                if (other.CompareTag(restZoneTagName))
                {
                    print("Exiting Trigger: " + other.name);

                    //myRigidBody.detectCollisions = true;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            //print("Entering Collision with: " + collision.gameObject.name);
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
                    //print("Setting Seperation");

                    seperation = contactPoint.separation;
                }
            }

            //print("Staying Collision with: " + collision.gameObject.name);
        }

        private void OnCollisionExit(Collision collision)
        {
            //print("Exiting Collision with: " + collision.gameObject.name);
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
        /// 
        /// </summary>
        public void SnapToCurrentTarget()
        {
            
        }

        #endregion

        #region PARAMETERS FOR STATES

        /// <summary>
        /// Set snappable params when the state is Out rest zone.        
        /// /// </summary>
        public void SetFollowMouseParams()
        {
            ChangeColorToSelected();

            myRigidBody.isKinematic = false;
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
                    //print("Using Seperation");

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
            //EventManager.Instance.RaiseLogicEvent("Deselect Current Snappable");

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
