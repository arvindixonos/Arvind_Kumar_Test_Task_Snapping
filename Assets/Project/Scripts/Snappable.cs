using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyScripts
{
    public class Snappable_State_Rest : IState
    {
        public void EnterState(object targetObject)
        {
            Snappable targetSnappable = targetObject as Snappable;

            targetSnappable.UnFreezePositionRotation();
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

            targetSnappable.FreezePositionRotation();
        }

        public void ExitState(object targetObject)
        {
        }

        public void UpdateState(object targetObject)
        {
        }
    }


    public class Snappable : MonoBehaviour
    {
        public const string STATE_REST = "Rest";
        public const string STATE_SNAPPED = "Snapped";

        private  List<Snappable> snapped = new List<Snappable>();

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

        private void Awake()
        {
            myRigidBody = GetComponent<Rigidbody>();

            InitStateMachine();
        }

        public void InitStateMachine()
        {
            stateMachine = new StateMachine(this);
            stateMachine.AddState(STATE_REST, new Snappable_State_Rest());
            stateMachine.AddState(STATE_SNAPPED, new Snappable_State_Snapped());

            ChangeState(STATE_REST);
        }

        private void ChangeState(string stateName)
        {
            currentStateName = stateMachine.ChangeState(stateName);
        }

        public void SnapToSimpleObject(Transform parentTransform, Vector3 snapPositionLocal)
        {
            transform.parent = parentTransform;
            transform.localPosition = snapPositionLocal;

            ChangeState(STATE_SNAPPED);
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

        public void FreezePositionRotation()
        {
            myRigidBody.constraints = RigidbodyConstraints.FreezeAll;
        }

        public void UnFreezePositionRotation()
        {
            myRigidBody.constraints = RigidbodyConstraints.None;
        }
    }
}
