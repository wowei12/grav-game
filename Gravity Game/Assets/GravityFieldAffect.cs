    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityFieldAffect : MonoBehaviour
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
                Debug.Log("Entered Field");
                rb.useGravity = false;

                c.gameObject.GetComponent<Movement>().nextRotate = fieldDirection;
                c.gameObject.GetComponent<Movement>().rotatable = true;
            }
        }        
    }
    void OnTriggerStay(Collider c)
    {
        Rigidbody rb = c.gameObject.transform.GetComponent<Rigidbody>(); 
        if (rb != null)
        {
            // not uniform grav field
            // Vector3 colliderPos = c.gameObject.transform.position;
            // Vector3 gravityfieldPos = this.gameObject.transform.position;
            
            // Vector3 differenceBetween = colliderPos - gravityfieldPos;

            // float angleBetween = Vector3.Angle(differenceBetween, fieldDirection);
            // // float heightPosition = (differenceBetween * Mathf.Sin(Mathf.Deg2Rad * angleBetween)).magnitude;

            // float heightPosition = Vector3.Dot(differenceBetween, fieldDirection);
            // // Debug.Log(heightPosition);
            
            // float m = 1239.46f;
            // float l = 1239.46f;
            // float b = -226.201f;
            // float p = -2.86466f;

            // float forcePower = Mathf.Pow((m / ((b * heightPosition) + l)), p);

            // Debug.Log(forcePower);

            // rb.useGravity = false;
            //(fieldDirection * fieldMagnitude * forcePower);

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
