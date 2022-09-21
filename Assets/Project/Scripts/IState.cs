using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    /// <summary>
    /// Interface for State. Implement this interface if there is need to use states and statemachine.
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// Gets called when an object implementing this state, enters it.
        /// </summary>
        /// <param name="targetObject">Typically owner object of this state</param>
        public abstract void EnterState(object targetObject);

        /// <summary>
        /// Typically gets called in the Update loop of the target object containing the statemachine.
        /// </summary>
        /// <param name="targetObject">Typically owner object of this state</param>
        public abstract void UpdateState(object targetObject);

        /// <summary>
        /// Gets called when an object implementing this state, exits it.
        /// </summary>
        /// <param name="targetObject">Typically owner object of this state</param>
        public abstract void ExitState(object targetObject);
    }
}
