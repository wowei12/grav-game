using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{


    public float maxSpeed;
    public float rotateSpeed;

    public float forceMove;
    public float forceJump;
    public float forceKFriction;
    
    private Rigidbody rb;
    
    private GameObject head;
    private GameObject mainCamera;

    private Vector2 keyInput;
    private Vector2 mouseInput;

    private float current_rot_x;
    private float current_rot_y;

    void Start()
    {
        // instantiate
        rb = this.GetComponent<Rigidbody>();
        head = this.transform.GetChild(0).gameObject;
        mainCamera = Camera.main.gameObject;
    }

    void Update()
    {
        // get user input
        keyInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mouseInput = new Vector2(Input.GetAxis("Mouse X") , Input.GetAxis("Mouse Y"));

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (IsGrounded())
            {
                Jump();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Flip();
        }
    }

    void FixedUpdate()
    {
        // player WASD
        Move();

        // reduce speed when stopping
        AddKineticFriction();

        // camera controls
        Rotate();


    }

    void Move()
    {
        // input force
        var goalForwardVel = Vector3.forward * keyInput.y;
        var goalSideVel = Vector3.right * keyInput.x;
        
        var goalVelocity = (goalForwardVel + goalSideVel).normalized * maxSpeed;

        Debug.Log(goalVelocity);

        if (goalVelocity != Vector3.zero)
        {
            // if velocity is closer to goal velocity, add less force
            // calculated in relative space
            var currentVelocity = transform.InverseTransformDirection(rb.velocity);
            var projectedVelocity = (goalVelocity.normalized * Vector3.Dot(goalVelocity, currentVelocity)) / goalVelocity.magnitude;

            rb.AddRelativeForce((goalVelocity - projectedVelocity) * forceMove, ForceMode.Force);
        }
    }

    void AddKineticFriction()
    {
        // only add kinetic friction if moving
        var currentVelocity = transform.InverseTransformDirection(rb.velocity);

        // remove y velocity
        currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, transform.up) * transform.up);
        
        var currentVelDir = currentVelocity.normalized;
        var frictionDir = -currentVelDir;

        rb.AddRelativeForce(frictionDir * forceKFriction * currentVelocity.magnitude);
    }

    void Rotate()
    {
        // janky fix, set rotation about x axis, but add rotation about y axis
        current_rot_y = mouseInput.x * rotateSpeed * Time.deltaTime;
        current_rot_x -= mouseInput.y * rotateSpeed * Time.deltaTime;

        // clamp to not camera look up too far or too low
        current_rot_x = Mathf.Clamp(current_rot_x, -90, 90);
            
        this.transform.Rotate(0, current_rot_y, 0, Space.Self);
        
        if (mouseInput.y != 0) {
            head.transform.localRotation = Quaternion.Euler(Vector3.right * current_rot_x);
        }
    }

    void Jump()
    {
        rb.AddRelativeForce(forceJump * Vector3.up, ForceMode.VelocityChange);
    }

    void Flip()
    {

    }

    bool IsGrounded()
    {
        var layerMask = (1 << 8) | (1 << 9);
        layerMask = ~layerMask;

        // shoot a raycast from the position of transform to the floor avoiding player and fields
        RaycastHit hit;
        Physics.Raycast(this.transform.position, -transform.up, out hit, Mathf.Infinity, layerMask);

        // if the dist from the center of mass to the ground = the dist of the center of mass, is grounded
        var rayCastDistance = (this.transform.position - hit.point).magnitude;
        var heightOfCharacter = 2f;

        return (Mathf.Approximately(rayCastDistance, heightOfCharacter / 2));
    }

}
