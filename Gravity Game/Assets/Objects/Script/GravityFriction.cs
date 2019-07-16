using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GravityFriction : MonoBehaviour
{
    public float forceKFriction;
    public bool useGravity;

    public bool emitterAttached;

    private float gravitationConstant;
    private Rigidbody rb;
    private Collider col;

    private float height;
    private float width;

    private int layerMask;

    void Start()
    {
        emitterAttached = false;
        
        useGravity = true;
        col = this.transform.GetComponent<Collider>();
        gravitationConstant = 9.8f;
        rb = this.GetComponent<Rigidbody>();

        height = col.bounds.size.y;
        width = col.bounds.size.x;

        layerMask = (1 << 8) | (1 << 9) | (1 << 10);
        layerMask = ~layerMask;

    }

    void FixedUpdate()
    {
    
        AddKineticFriction();
        
        if (useGravity)
        {
            Gravity();
        }
    }

    void AddKineticFriction()
    {
        // check walls to apply friction alongside

        var frictionHit = new List<RaycastHit>();

        // shoot a bunch of raycast in a circle, then save the raycasts that hit "close enough". basically collision detection
        for (int i = 0; i <= 90; i += 5)
        {
            var radians = Mathf.Deg2Rad * i;    
            var raycastDir = Mathf.Cos(radians) * transform.right + Mathf.Sin(radians) * transform.forward;

            RaycastHit hit;
            Physics.Raycast(this.transform.position, raycastDir, out hit, Mathf.Infinity, layerMask);

            if (Vector3.Distance(hit.point, this.transform.position) < width / 2 + 0.1f)
            {
                frictionHit.Add(hit);
            }
        }

        RaycastHit hitUp;
        Physics.Raycast(this.transform.position, transform.up, out hitUp, Mathf.Infinity, layerMask);
        if (Vector3.Distance(hitUp.point, this.transform.position) < height/2 + 0.1f)
        {
            frictionHit.Add(hitUp);
        }

        RaycastHit hitDown;
        Physics.Raycast(this.transform.position, -transform.up, out hitDown, Mathf.Infinity, layerMask);
        if (Vector3.Distance(hitDown.point, this.transform.position) < height/2 + 0.1f)
        {
            frictionHit.Add(hitDown);
        }

        if (frictionHit.Count != 0)
        {
            var normals = frictionHit
                .Select(r => r.normal)
                .Distinct();

            var currentVelocity = rb.velocity;

            foreach (Vector3 n in normals)
            {
                currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, n) * n);
            }

            // currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, transform.up) * transform.up);
            
            var currentVelDir = currentVelocity.normalized;
            var frictionDir = -currentVelDir;

            rb.AddForce(frictionDir * forceKFriction * currentVelocity.magnitude);

        }
    }

    void Gravity()
    {
        Vector3 gravDirection = -Vector3.up;
        rb.AddForce(gravDirection * gravitationConstant, ForceMode.Force);
    }
}
