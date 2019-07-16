using UnityEngine;
using System.Collections;


public class Movement : MonoBehaviour
{

    public float jumpPower = 5f;

    public float movespeed = 0.05f;
    public float mouseSensitivity = 299f;

    public bool inGravField;

    public float rotation_x;
    public float rotation_y;
    public Rigidbody rb;

    public Vector3 nextRotate;

    public Quaternion oldRot;
    public Quaternion newRot;

    public bool rotatable;

    GameObject cube; 
    
    GameObject reference;

    // Start is called before the first frame update
    void Start()
    {
        reference = GameObject.Find("Reference");
        inGravField = false;
        Cursor.lockState = CursorLockMode.Locked;
        cube = GameObject.Find("cameraO");
        rb = GetComponent<Rigidbody>();
       // rb.freezeRotation = true;
        rotatable = false;
    }

    // Update is called once per frame
    void FixedUpdate() 
    {
        // translation
        
        var forwardSpeed = Input.GetAxisRaw("Vertical");
        var sidewaysSpeed = Input.GetAxisRaw("Horizontal");
        var forwardVelocity = transform.forward * forwardSpeed; // z
        var sidewaysVelocity = transform.right * sidewaysSpeed; // x
        
        var falltime = rb.velocity.y * transform.up;
        
        //rb.velocity = forwardVelocity + sidewaysVelocity + falltime;
        rb.MovePosition(rb.transform.position + ((forwardVelocity + sidewaysVelocity) * movespeed));

        // rotation
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        rotation_y = mouseInput.x * mouseSensitivity * Time.deltaTime;
        rotation_x -= mouseInput.y * mouseSensitivity * Time.deltaTime;

        rotation_x = Mathf.Clamp(rotation_x, -90, 90);

        if (mouseInput.x != 0) {
            // this.transform.localRotation = Quaternion.Euler(transform.localEulerAngles.x, rotation_y, transform.localEulerAngles.z);
            this.transform.Rotate(0, rotation_y, 0, Space.Self);
        }
        if (mouseInput.y != 0) {
            cube.transform.localRotation = Quaternion.Euler(Vector3.right * rotation_x);
            // cube.transform.Rotate(rotation_x, 0, 0, Space.Self);
        }

        //flip rotation
        if (Input.GetKeyDown(KeyCode.F) && rotatable)
        {
            if (rb.useGravity == false && !(Mathf.Approximately(Vector3.Dot(transform.up, -nextRotate), 1)))
            {
                oldRot = this.transform.rotation;

                var newUp = -nextRotate;
                var angleNew = Vector3.SignedAngle(nextRotate, transform.forward, transform.up);

                if (angleNew > 90)
                {
                    angleNew = (angleNew - 180);
                }
                else if (angleNew < -90)
                {
                    angleNew = (angleNew + 180);
                }
                
                var newForward = Vector3.Cross(this.transform.right, newUp);
                newRot = Quaternion.AngleAxis(angleNew, newUp) * Quaternion.LookRotation(newForward, newUp);

                StartCoroutine("rotateTowards");

                reference.transform.rotation = Quaternion.LookRotation(newForward, newUp);
                rotatable = false;
            }
            else if (rb.useGravity == true && !(Mathf.Approximately(Vector3.Dot(transform.up, Vector3.up), 1)))
            {
                oldRot = this.transform.rotation;

                var newUp = Vector3.up;
                var angleNew = Vector3.SignedAngle(-Vector3.up, transform.forward, transform.up);

                if (angleNew > 90)
                {
                    angleNew = (angleNew - 180);
                }
                else if (angleNew < -90)
                {
                    angleNew = (angleNew + 180);
                }
                
                var newForward = Vector3.Cross(this.transform.right, newUp);

                newRot = Quaternion.AngleAxis(angleNew, newUp) * Quaternion.LookRotation(newForward, newUp);
                
                StartCoroutine("rotateTowards");

                reference.transform.rotation = Quaternion.LookRotation(Vector3.forward, newUp);
                rotatable = false;
            }
            // jump archived
            // else
            // {
            //     rb.AddRelativeForce(Vector3.up * jumpPower, ForceMode.VelocityChange);  
            // }
        }

    }

    IEnumerator rotateTowards()
    {
        float smoothT = 0.0f;
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.deltaTime;
            smoothT = t*t * (3f - 2f*t);

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            transform.rotation = Quaternion.Slerp(oldRot, newRot, smoothT);
            yield return null;
        }
    }
}

