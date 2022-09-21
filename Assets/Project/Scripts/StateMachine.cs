using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    /// <summary>
    /// Manages the states and transitions between them.
    /// </summary>
    public class StateMachine
    {
        // Owner of this statemachine instance.
        private object ownerObject;

        // Reference to current state.
        private IState currentState;

        // Reference to previous state.
        private IState previousState;

        // List of states the owner has.
        private Dictionary<string, IState> statesList = new Dictionary<string, IState>();
    
        /// <summary>
        /// Constructor of the statemachine class.
        /// </summary>
        /// <param name="ownerObject">Typically the owner instance of the statemachine.</param>
        public StateMachine(object ownerObject)
        {
            this.ownerObject = ownerObject;
            this.currentState = null;
            this.previousState = null;
        }

        /// <summary>
        /// Changes the state to the new state.
        /// </summary>
        /// <param name="newState">Reference of the IState implemented object.</param>
        private void ChangeState(IState newState)
        {
            // If current state is not the new state.
            if(currentState != newState)
            {
                // If current state is not null.
                if(currentState != null)
                {
                    // Exit the current state.
                    currentState.ExitState(ownerObject);
                }

                // Save the current state.
                previousState = currentState;

                // Change the current state to newstate.
                currentState = newState;

                // Enter the new current state.
                currentState.EnterState(ownerObject);
            }
        }

        /// <summary>
        /// Changes the state using the name of the state.
        /// </summary>
        /// <param name="stateName">Name of the state to change to.</param>
        /// <returns></returns>
        public string ChangeState(string stateName)
        {
            // If the state name is present in the stateslist dict.
            if(statesList.ContainsKey(stateName))
            {
                // Change to the mentioned state.
                ChangeState(statesList[stateName]);

                // Return the name of the new state.
                return stateName;
            }

            return string.Empty;
        }

        /// <summary>
        /// Typically gets called in the owner object's updated method. Calls the currentstates updatestate function.
        /// </summary>
        public void UpdateCurrentState()
        {
            if(currentState != null)
            {
                currentState.UpdateState(ownerObject);
            }
        }

        /// <summary>
        /// Add the state the the list of states.
        /// </summary>
        /// <param name="stateName">Name of the state to add.</param>
        /// <param name="state">Reference of the IState implemented object.</param>
        public void AddState(string stateName, IState state)
        {
            statesList.Add(stateName, state);
        }
    }
}
