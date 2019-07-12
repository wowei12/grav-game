using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGun : MonoBehaviour
{
    // Start is called before the first frame update

    public GameObject emitterToward;
    public GameObject emitterAway;

    public GameObject head;


    public GameObject gravityFieldToward;
    public GameObject gravityFieldAway;


    // public float bulletSpeed = 15f;

    void Start()
    {
        head = Camera.main.transform.parent.gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            FireTowardsNormal();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            FireAwayNormal();
        }
    }

    void FireTowardsNormal()
    {
        Vector3 fireDirection = head.transform.forward;
        int layerMask = (1 << 8) | (1 << 9);
        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, fireDirection, out hit, Mathf.Infinity, layerMask))
        {
            GameObject checkEmitter = GameObject.Find("EmitterTowardsNormal");
            if (checkEmitter != null)
            {
                this.GetComponent<Rigidbody>().useGravity = true;
                this.GetComponent<Movement>().rotatable = true;

                Destroy(checkEmitter);
            }


            GameObject newEmitter = Instantiate(emitterToward, transform.position + fireDirection * hit.distance, transform.rotation);  
            newEmitter.name = "EmitterTowardsNormal";

            GameObject newGravField = Instantiate(gravityFieldToward, transform.position + fireDirection * hit.distance, Quaternion.identity, newEmitter.transform);
            newGravField.name = "FieldTowardsNormal";
            newGravField.GetComponent<GravityFieldAffect>().fieldDirection = -hit.normal; // towards normals

            // newEmitter.transform.parent = hit.transform.gameObject.transform;

        }

        // GameObject newEmitter = Instantiate(emitter, transform.position + fireDirection * 0.5f, transform.rotation);
        // Rigidbody rb = newEmitter.GetComponent<Rigidbody>();    
        // rb.velocity = fireDirection * bulletSpeed;

    }

    void FireAwayNormal()
    {
        Vector3 fireDirection = head.transform.forward;
        int layerMask = (1 << 8) | (1 << 9);
        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(transform.position, fireDirection, out hit, Mathf.Infinity, layerMask))
        {
            GameObject checkEmitter = GameObject.Find("EmitterAwayNormal");
            if (checkEmitter != null)
            {
                this.GetComponent<Rigidbody>().useGravity = true;
                Destroy(checkEmitter);
            }


            GameObject newEmitter = Instantiate(emitterAway, transform.position + fireDirection * hit.distance, transform.rotation);  
            newEmitter.name = "EmitterAwayNormal";

            GameObject newGravField = Instantiate(gravityFieldAway, transform.position + fireDirection * hit.distance, Quaternion.identity, newEmitter.transform);
            newGravField.name = "FieldAwayNormal";
            newGravField.GetComponent<GravityFieldAway>().fieldDirection = hit.normal; // away surface

            // newEmitter.transform.parent = hit.transform.gameObject.transform;

        }

        // GameObject newEmitter = Instantiate(emitter, transform.position + fireDirection * 0.5f, transform.rotation);
        // Rigidbody rb = newEmitter.GetComponent<Rigidbody>();    
        // rb.velocity = fireDirection * bulletSpeed;
    }
}
