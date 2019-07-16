using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityField : MonoBehaviour
{
    public float fieldStrength;
    public Vector3 fieldDirection;

    public bool fieldEnable;

    public bool fieldIsDisabled;

    void Start()
    {
        // play courotine animation
        // EnableField();
        fieldEnable = true;
        fieldIsDisabled = false;
    }

    void FixedUpdate()
    {
        //
        if (!fieldEnable)
        {
            // play couroutine disable animation
            // placeholder
            DisableField();
            fieldIsDisabled = true;
        }
        if (fieldEnable && fieldIsDisabled)
        {
            EnableField();
            fieldIsDisabled = false;
        }
    }

    void DisableField()
    {
        this.transform.localScale = new Vector3(0, 0, 0);
    }

    void EnableField()
    {
        this.transform.localScale = Vector3.one * 2.5f;
    }

    void OnTriggerStay(Collider c)
    {
        if (fieldEnable)
        {
            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (c.gameObject.tag == "Player")
                {
                    // c.gameObject.GetComponent<PlayerScript>().useGravity = false;
                    rb.AddForce(fieldDirection * fieldStrength, ForceMode.Force);
                }
                else if (c.gameObject.name == "Emitter")
                {

                }
                else
                {
                    c.gameObject.GetComponent<GravityFriction>().useGravity = false;
                    rb.AddForce(fieldDirection * fieldStrength, ForceMode.Force);
                }
            } 
        }

    }

    void OnTriggerExit(Collider c)
    {
        if (fieldEnable)
        {
            Rigidbody rb = c.GetComponent<Rigidbody>();
            if (rb != null)
            {
                if (c.gameObject.tag == "Player")
                {
                    c.gameObject.GetComponent<PlayerScript>().useGravity = true;
                    // rb.AddForce(fieldDirection * fieldStrength, ForceMode.Force);
                }
                else
                {
                    c.gameObject.GetComponent<GravityFriction>().useGravity = true;
                    // rb.AddForce(fieldDirection * fieldStrength, ForceMode.Force);
                }
            } 
        }

    }

}
