using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    public interface IState
    {
        public abstract void EnterState(object targetObject);
        public abstract void UpdateState(object targetObject);
        public abstract void ExitState(object targetObject);
    }
}
