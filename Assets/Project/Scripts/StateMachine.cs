using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    public class StateMachine
    {
        public object ownerObject;

        public IState currentState;
        public IState previousState;

        public Dictionary<string, IState> statesList = new Dictionary<string, IState>();
    
        public StateMachine(object ownerObject)
        {
            this.ownerObject = ownerObject;
            this.currentState = null;
            this.previousState = null;
        }

        private void ChangeState(IState newState)
        {
            if(currentState != newState)
            {
                if(currentState != null)
                {
                    currentState.ExitState(ownerObject);
                }

                previousState = currentState;

                currentState = newState;
                currentState.EnterState(ownerObject);
            }
        }

        public string ChangeState(string stateName)
        {
            if(statesList.ContainsKey(stateName))
            {
                ChangeState(statesList[stateName]);

                return stateName;
            }

            return string.Empty;
        }

        public void UpdateCurrentState()
        {
            if(currentState != null)
            {
                currentState.UpdateState(ownerObject);
            }
        }

        public void AddState(string stateName, IState state)
        {
            statesList.Add(stateName, state);
        }

        public string GetCurrentStateName()
        {
            foreach(KeyValuePair<string, IState> keyValuePair in statesList)
            {
                if(currentState == keyValuePair.Value)
                    return keyValuePair.Key;
            }

            return string.Empty;
        }
    }
}
