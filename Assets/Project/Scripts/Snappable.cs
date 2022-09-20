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

        public void SnapToSimpleObject(Transform parentTransform, Vector3 snapPositionLocal)
        {
            transform.parent = parentTransform;
            transform.localPosition = snapPositionLocal;
            IsSnapped = true;
        }
    }
}
