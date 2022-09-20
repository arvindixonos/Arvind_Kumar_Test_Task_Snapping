using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Transform box1;
    private Collider ourCollider;

    void Start()
    {
        ourCollider = box1.GetComponent<Collider>();
    }

    void Update()
    {
        DebugExtension.DebugBounds(new Bounds(box1.position, box1.GetComponent<Renderer>().bounds.size), Color.yellow);
        Collider[] colliders = Physics.OverlapBox(box1.position, box1.GetComponent<Renderer>().bounds.extents);
        
        if(colliders.Length > 1)
        {
            print("Overlapping something");
        }
        else
        {
            print("Not Overlapping");
        }
    }
}
