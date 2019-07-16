using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickyToObj : MonoBehaviour
{
    public GameObject stickTo;
    public bool stick;

    public Vector3 positionRelative;

    void Start()
    {
        stick = true;
        positionRelative = this.transform.position - stickTo.transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
    if (stick)  
        {
            this.transform.position = stickTo.transform.position + positionRelative; 
        }
    }
}
