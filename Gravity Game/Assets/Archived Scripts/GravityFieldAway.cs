using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityFieldAway : MonoBehaviour
{
    public Vector3 fieldDirection;
    public float fieldMagnitude = 10f;

    public float damp = 0.37f;
    // Start is called before the first frame update
    void Start()
    {
        fieldDirection = fieldDirection.normalized;
    }


    void OnTriggerEnter(Collider c)
    {
        Rigidbody rb = c.gameObject.transform.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            rb.velocity += fieldDirection * Mathf.Abs(Vector3.Dot(rb.velocity, fieldDirection)) * damp;

            if (c.gameObject.tag == "Player")
            {
                rb.useGravity = false;        
            }        
        }
    }
    void OnTriggerStay(Collider c)
    {
        Rigidbody rb = c.gameObject.transform.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            rb.AddForce(fieldDirection * fieldMagnitude, ForceMode.Acceleration);
            rb.useGravity = false;
        }
    }
    void OnTriggerExit(Collider c)
    {
        
        Rigidbody rb = c.gameObject.transform.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            rb.useGravity = true;
            
            c.gameObject.GetComponent<Movement>().rotatable = true;

        } 
    }
}
