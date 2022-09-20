using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace MyScripts
{
    public class Snappable : MonoBehaviour
    {
        private  List<Snappable> snapped = new List<Snappable>();

        private bool isSnapped = false;
        public bool IsSnapped { get => isSnapped; set => isSnapped = value; }

        private Collider myCollider;

        private Renderer myRenderer;

        private void Awake()
        {
            myCollider = GetComponentInChildren<Collider>();
            myRenderer = GetComponentInChildren<Renderer>();
        }

        void Start()
        {
            
        }
    }
}
