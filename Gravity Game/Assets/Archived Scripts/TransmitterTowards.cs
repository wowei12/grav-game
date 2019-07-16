using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransmitterTowards : MonoBehaviour
{
    public GameObject gravityField;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision c) 
    {

        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        GameObject gravFieldNew = Instantiate(gravityField, transform.position, Quaternion.identity, this.transform);
        gravFieldNew.GetComponent<GravityFieldAffect>().fieldDirection = c.contacts[0].normal;

    }
}
