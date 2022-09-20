using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MyScripts
{
    public class SimpleObject : Singleton<SimpleObject>
    {
        private float scanRadious = 5f;

        public override void Awake()
        {
            base.Awake();


        }


    }
}
