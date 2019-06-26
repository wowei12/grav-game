using UnityEngine;

public class Movement : MonoBehaviour
{

    public float thrust = 100f;
    public float mouseSensitivity = 299f;

    public float rotation_x;
    public float rotation_y;


    GameObject cube; 

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        cube = GameObject.Find("cameraO");
    }

    // Update is called once per frame
    void Update() 
    {
        // translation
        // Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        // Vector3 inputDir = input.normalized;


        // inputDir = transform.forward * inputDir.x + transform.right * inputDir.z;
        
//        Vector3 localVelDir = transform.InverseTransformDirection(inputDir);

        var forwardSpeed = 3.7f * Input.GetAxisRaw("Vertical");
        var sidewaysSpeed = 3.7f * Input.GetAxisRaw("Horizontal");
        var forwardVelocity = transform.forward * forwardSpeed;
        var sidewaysVelocity = transform.right * sidewaysSpeed;
        
        var rigidbody = GetComponent<Rigidbody>();
        rigidbody.velocity = forwardVelocity + sidewaysVelocity;
        
        // transform.GetComponent<Rigidbody>().AddRelativeForce(inputDir * thrust);


        // rotation
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        rotation_y += mouseInput.x * mouseSensitivity * Time.deltaTime;
        rotation_x -= mouseInput.y * mouseSensitivity * Time.deltaTime;

        rotation_x = Mathf.Clamp(rotation_x, -90, 90);

        if (mouseInput.x != 0) {
            transform.rotation = Quaternion.Euler(0, rotation_y, 0);
        }
        if (mouseInput.y != 0) {
            cube.transform.localRotation = Quaternion.Euler(Vector3.right * rotation_x);

        }

    }
}
