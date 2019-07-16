using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public bool useGravity;

    public float maxSpeed;
    public float rotateSpeed;

    public float maxAirSpeed;

    public float forceMove;
    public float forceAirMove;
    public float forceJump;
    public float forceKFrictionHorizontal;
    public float forceKFrictionVertical;
    public float forceDrag;

    public float flipTime;
    public float wallFlipDist;

    public float flipCooldown;

    public float deltaCameraYJump;
    public float deltaCameraBobTime;

    public float deltaCameraFallConstant;
    public float deltaCameraFallTime;

    public float fieldGunCooldown;

    public GameObject gField;
    public GameObject emitter;
    public GameObject gravField;

    public float pickUpRange;
    public float pickUpLeashRange;
    public float pushBackTime;

    private bool isPickingUp;

    private Rigidbody rb;
    
    private GameObject mainCamera;
    private Collider col;

    private Vector2 keyInput;
    private Vector2 mouseInput;

    private float current_rot_x;
    private float current_rot_y;

    private Quaternion oldRot;
    private Quaternion newRot;

    private Vector3 oldPos;
    private Vector3 newPos;
    private Vector3 dirNextRotate;

    private int layerMask;
    private int interactableLayerMask;
    private int gravityPickupLayerMask;

    private float gravitationConstant;

    private float height;
    private float width;
    private float fallVelocity;

    private bool canFlip;
    private bool canFire;
    private bool canFallBob;
    private bool cameraRotate;
    private bool canJump;
    private bool isGrounded;

    private GameObject gun;

    private AudioSource audioSourceFootsteps;
    private AudioSource audioSourceJumpLand;

    private GameObject destroy;

    private float epsilon;

    private Rigidbody rbPickup;
    private float initialPickupDistance;
    private GameObject objPickup;
    private GameObject prevEmitterAttachedGameObject;

    private bool isPickingUpField;

    void Start()
    {
        isPickingUpField = false;
        isPickingUp = false;
        canJump = true;
        epsilon = 0.1f;
        cameraRotate = true;
        useGravity = true;
        rb = this.GetComponent<Rigidbody>();
        mainCamera = Camera.main.gameObject;
        col = gameObject.GetComponent<Collider>();
        // gun = this.transform.GetChild(3).gameObject;

        width = col.bounds.size.x;

        canFallBob = false;
        
        height = col.bounds.size.y;

        canFlip = true;
        canFire = true;

        gravitationConstant = 9.8f;
        dirNextRotate = transform.up;

        Cursor.lockState = CursorLockMode.Locked;

        layerMask = (1 << 8) | (1 << 9) | (1 << 10) | (1 << 12);
        layerMask = ~layerMask;

        interactableLayerMask = (1 << 11);
        gravityPickupLayerMask = (1 << 12);

        audioSourceFootsteps = this.transform.GetChild(1).transform.GetComponent<AudioSource>();
        audioSourceFootsteps.enabled = false;

        audioSourceJumpLand = this.transform.GetChild(2).transform.GetComponent<AudioSource>();
        audioSourceJumpLand.enabled = true;

        isGrounded = false;

    }

    void Update()
    {
        // get user input
        keyInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        mouseInput = new Vector2(Input.GetAxis("Mouse X") , Input.GetAxis("Mouse Y"));

        if (Input.GetKeyDown(KeyCode.Space) && IsJumpable() && canJump)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.F) && CheckWall() && canFlip)
        {
            Flip();
            StartCoroutine("FlipCooldown");
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && canFire)
        {
            if (isPickingUpField)
            {
                ShootOutField(); //
            }
            else
            {
                FireField();
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            // Debug.Log("E Key Fired");
            if (!isPickingUp)
            {
                // Debug.Log("Pickup()");
                Pickup();
            }
            else
            {
                // Debug.Log("LetGo()");
                LetGo();
            }
        }
    }

    void ShootOutField()
    {
        RaycastHit locate;
        Vector3 fireDirection = mainCamera.transform.forward;

        if (Physics.Raycast(this.transform.position, fireDirection, out locate, Mathf.Infinity, layerMask))
        {
            StopCoroutine("PickingObjectUp");
            isPickingUpField = false;
            isPickingUp = false;

            rbPickup.isKinematic = true;
            rbPickup.transform.parent.gameObject.GetComponent<StickyToObj>().stick = true;
            rbPickup.transform.parent.gameObject.GetComponent<StickyToObj>().positionRelative = locate.point - locate.collider.gameObject.transform.position;
            rbPickup.transform.parent.gameObject.GetComponent<StickyToObj>().stickTo = locate.collider.gameObject;

            if (locate.collider.gameObject.GetComponent<GravityFriction>() != null)
            {
                locate.collider.gameObject.GetComponent<GravityFriction>().emitterAttached = true;
            }

            objPickup.GetComponent<Collider>().isTrigger = true;

            prevEmitterAttachedGameObject = locate.collider.gameObject;

            objPickup.GetComponent<GravityFriction>().useGravity = false;
            objPickup.transform.parent.GetChild(1).GetComponent<GravityField>().fieldEnable = true;


            GameObject gFieldOriginal = objPickup.transform.parent.gameObject;
            GameObject gravFieldOriginal = objPickup.transform.parent.GetChild(1).parent.gameObject;
            GameObject emitterOriginal = objPickup;

            // gravFieldOriginal.transform.parent = null;
            // emitterOriginal.transform.parent = null;

            gFieldOriginal.transform.position = locate.point;
            gravFieldOriginal.transform.localPosition = Vector3.zero;
            emitterOriginal.transform.localPosition = Vector3.zero;



            // emitterOriginal.transform.parent = gFieldOriginal.transform;
            // gravFieldOriginal.transform.parent = gFieldOriginal.transform;

            // Debug.Log(locate.point);
        }

    }

    void Pickup()
    {
        RaycastHit pickUpHit;
        if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out pickUpHit, pickUpRange, interactableLayerMask))   
        {
            // Debug.Log("RaycastFired and Accepted");
            objPickup = pickUpHit.collider.gameObject;
            rbPickup = pickUpHit.collider.gameObject.GetComponent<Rigidbody>();
            initialPickupDistance = Vector3.Distance(mainCamera.transform.position, pickUpHit.collider.gameObject.transform.position);
            

            objPickup.GetComponent<GravityFriction>().useGravity = false;

            if (objPickup.GetComponent<GravityFriction>().emitterAttached)
            {
                GameObject.Find("GField").gameObject.transform.GetChild(1).transform.GetComponent<GravityField>().fieldEnable = false;
            }

            StartCoroutine("PickingObjectUp");
        }

        else if (Physics.Raycast(mainCamera.transform.position, mainCamera.transform.forward, out pickUpHit, pickUpRange, gravityPickupLayerMask))
        {
            isPickingUpField = true;
            objPickup = pickUpHit.collider.gameObject;
            rbPickup = pickUpHit.collider.gameObject.GetComponent<Rigidbody>();
            initialPickupDistance = Vector3.Distance(mainCamera.transform.position, pickUpHit.collider.gameObject.transform.position);

            rbPickup.isKinematic = false;
            rbPickup.transform.parent.gameObject.GetComponent<StickyToObj>().stick = false;
            objPickup.GetComponent<GravityFriction>().useGravity = false;
            objPickup.transform.parent.GetChild(1).GetComponent<GravityField>().fieldEnable = false;
            objPickup.GetComponent<Collider>().isTrigger = false;

            if (prevEmitterAttachedGameObject != null && prevEmitterAttachedGameObject.GetComponent<GravityFriction>() != null)
            {
                prevEmitterAttachedGameObject.GetComponent<GravityFriction>().emitterAttached = false;
            }
            
            StartCoroutine("PickingObjectUp");

        }
    }

    IEnumerator PickingObjectUp()
    {
        // Debug.Log("Pickup Begun");
        isPickingUp = true;
        while (isPickingUp)
        {
            // Debug.Log("Lifting...");              
            Vector3 oldPosition = objPickup.transform.position;
            Vector3 nextPosition = mainCamera.transform.position + mainCamera.transform.forward * initialPickupDistance;
            
            Vector3 moveForceDir = (nextPosition - oldPosition).normalized;
            float moveObjDistance = Vector3.Distance(oldPosition, nextPosition);

            float forceConstant = (2f / (pushBackTime * pushBackTime)) * (moveObjDistance + (1f / 6f) * pushBackTime * pushBackTime * pushBackTime);

            rbPickup.MovePosition(nextPosition);

            yield return new WaitForFixedUpdate();

            if (!isPickingUp)
            {

            }
            else if (Vector3.Distance(objPickup.transform.position, nextPosition) > pickUpLeashRange)
            {
                LetGo();
            }
        }
    }

    void LetGo()
    {
        // Debug.Log("Letting go...");
        objPickup.GetComponent<GravityFriction>().useGravity = true;
        isPickingUp = false;
        isPickingUpField = false;

        if (objPickup.GetComponent<GravityFriction>().emitterAttached)
        {
            GameObject.Find("GField").gameObject.transform.GetChild(1).transform.GetComponent<GravityField>().fieldEnable = true;
        }
    }

    void FireField()
    {
        StartCoroutine("FieldCooldown");

        RaycastHit hit;
        Vector3 fireDirection = mainCamera.transform.forward;
        if (Physics.Raycast(this.transform.position, fireDirection, out hit, Mathf.Infinity, layerMask))
        {
            if (GameObject.Find("GField") != null)
            {
                destroy = (GameObject.Find("GField"));
                destroy.GetComponent<StickyToObj>().stick = false;
                StartCoroutine("DestroyField");

                if (prevEmitterAttachedGameObject != null && prevEmitterAttachedGameObject.GetComponent<GravityFriction>() != null)
                {
                    prevEmitterAttachedGameObject.GetComponent<GravityFriction>().emitterAttached = false;
                }
            }

            prevEmitterAttachedGameObject = hit.collider.gameObject;

            if (hit.collider.gameObject.GetComponent<GravityFriction>() != null)
            {
                hit.collider.gameObject.GetComponent<GravityFriction>().emitterAttached = true;
            }
            GameObject newGField = Instantiate(gField, hit.point, Quaternion.identity);
            newGField.name = "GField";
            newGField.GetComponent<StickyToObj>().stickTo = hit.collider.gameObject;

            GameObject newEmitter = Instantiate(emitter, hit.point, Quaternion.identity, newGField.transform);
            GameObject newField = Instantiate(gravField, hit.point, Quaternion.identity, newGField.transform);

            newEmitter.name = "Emitter";

            newField.GetComponent<GravityField>().fieldDirection = -hit.normal;
        }
    }

    IEnumerator DestroyField()
    {
        destroy.transform.position = Vector3.one * 1000f;
        yield return new WaitForFixedUpdate();
        Destroy(destroy);
    }

    IEnumerator FieldCooldown()
    {
        canFire = false;
        yield return new WaitForSecondsRealtime(fieldGunCooldown);
        canFire = true;
    }

    void FixedUpdate()
    {
        
        CheckGrounded();

        AddKineticFriction();

        if (IsJumpable())
        {
            Move();
        }
        else // is not "grounded", is in air
        {
            CheckFalling();
            MoveAir();
            Drag();
        }

        // real grounded check
        if (isGrounded)
        {
            SoundFootsteps();
        }
        else
        {
            audioSourceFootsteps.enabled = false;
        }
        
        if (cameraRotate)
        {
            RotateCamera();
        }

        if (useGravity)
        {
            Gravity();
        }
    }

    void OnCollisionEnter()
    {
        if (canFallBob)
        {
            audioSourceJumpLand.Play();
            StartCoroutine("MuteFootsteps5");

            StartCoroutine("CameraBobFalling");
            canFallBob = false;
        }
    }

    IEnumerator MuteFootsteps5()
    {
        audioSourceFootsteps.mute = true;
        yield return new WaitForSeconds(0.33f);
        audioSourceFootsteps.mute = false;
    }

    void CheckFalling()
    {
        RaycastHit hit;
        Physics.Raycast(this.transform.position, -this.transform.up, out hit, Mathf.Infinity, layerMask);
        
        if (Vector3.Distance(this.transform.position, hit.point) < 5f)
        {
            fallVelocity = Vector3.Dot(-this.transform.up, rb.velocity);
            if (fallVelocity > 0)
            {
                canFallBob = true;
            }
        }
    }

    void MoveAir()
    {
        // input force
        var goalForwardVel = Vector3.forward * keyInput.y;
        var goalSideVel = Vector3.right * keyInput.x;
        
        var goalVelocity = (goalForwardVel + goalSideVel).normalized * maxAirSpeed;

        if (goalVelocity != Vector3.zero)
        {
            // if velocity is closer to goal velocity, add less force
            // calculated in relative space
            var currentVelocity = transform.InverseTransformDirection(rb.velocity);
            var projectedVelocity = (goalVelocity.normalized * Vector3.Dot(goalVelocity, currentVelocity)) / goalVelocity.magnitude;

            rb.AddRelativeForce((goalVelocity - projectedVelocity) * forceAirMove, ForceMode.Force);
        }

        // basic fix
        // var forwardAirMove = Vector3.forward * keyInput.y;
        // var sideAirMove = Vector3.right * keyInput.x;
        
        // rb.AddRelativeForce(forceAirMove * (forwardAirMove + sideAirMove));
    }

    void Drag()
    {
        var currentVelocity = rb.velocity;

        // remove vert
        currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, transform.up) * transform.up);
        
        var currentVelDir = currentVelocity.normalized;
        var frictionDir = -currentVelDir;

        rb.AddForce(frictionDir * forceDrag * currentVelocity.magnitude);
    }

    void AddKineticFriction()
    {
        // check walls to apply friction alongside

        var frictionHit = new List<RaycastHit>();

        // shoot a bunch of raycast in a circle, then save the raycasts that hit "close enough". basically collision detection
        for (int i = 0; i <= 360; i += 5)
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

        for (int j = 0; j <= 360; j += 5)
        {
            var jRad = Mathf.Deg2Rad * j;
            for (int i = 180; i <= 360; i += 5)
            {
                var radians = Mathf.Deg2Rad * i;    
                var raycastDir = Mathf.Cos(radians) * transform.right + Mathf.Sin(radians) * transform.up + Mathf.Sin(jRad) * transform.forward;

                // Debug.DrawRay(this.transform.position + (-height/2f + width/2f) * this.transform.up, raycastDir * 10f, Color.green, 0.5f);

                RaycastHit hit;
                Physics.Raycast(this.transform.position + (-height/2f + width/2f) * this.transform.up, raycastDir, out hit, Mathf.Infinity, layerMask);

                if (Vector3.Distance(hit.point, this.transform.position) < height / 2 + 0.1f)
                {
                    frictionHit.Add(hit);
                }
            }

        }

        // for (int i = 260; i <= 280; i += 5)
        // {
        //     var radians = Mathf.Deg2Rad * i;    
        //     var raycastDir = Mathf.Cos(radians) * transform.forward + Mathf.Sin(radians) * transform.up;

        //     RaycastHit hit;
        //     Physics.Raycast(this.transform.position, raycastDir, out hit, Mathf.Infinity, layerMask);

        //     if (Vector3.Distance(hit.point, this.transform.position) < height / 2 + 0.1f)
        //     {
        //         frictionHit.Add(hit);
        //     }
        // }

        // RaycastHit hitUp;
        // Physics.Raycast(this.transform.position, transform.up, out hitUp, Mathf.Infinity, layerMask);
        // if (Vector3.Distance(hitUp.point, this.transform.position) < height/2 + 0.1f)
        // {
        //     frictionHit.Add(hitUp);
        // }

        // RaycastHit hitDown;
        // Physics.Raycast(this.transform.position, -transform.up, out hitDown, Mathf.Infinity, layerMask);
        // if (Vector3.Distance(hitDown.point, this.transform.position) < height/2 + 0.1f)
        // {
        //     frictionHit.Add(hitDown);
        // }



        if (frictionHit.Count != 0)
        {
            var normals = frictionHit
                .Select(r => r.normal)
                .Distinct();

            var currentVelocity = rb.velocity;

            foreach (Vector3 n in normals)
            {
                currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, n) * n);
                var currentVelDir = currentVelocity.normalized;
                var frictionDir = -currentVelDir;   

                // Debug.Log(n + " : " + transform.up);
                if (Vector3.Distance(n, this.transform.up) < epsilon)
                {
                    // Debug.Log("Horizontal Friction");
                    rb.AddForce(frictionDir * forceKFrictionHorizontal * currentVelocity.magnitude);
                }
                else
                {
                    // Debug.Log("Vertical Friction");
                    // rb.AddForce(frictionDir * forceKFrictionVertical * currentVelocity.magnitude);
                }
            }

            // currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, transform.up) * transform.up);
            


        }


    }

    void Move()
    {
        // input force
        var goalForwardVel = Vector3.forward * keyInput.y;
        var goalSideVel = Vector3.right * keyInput.x;
        
        var goalVelocity = (goalForwardVel + goalSideVel).normalized * maxSpeed;

        if (goalVelocity != Vector3.zero)
        {
            // if velocity is closer to goal velocity, add less force
            // calculated in relative space
            var currentVelocity = transform.InverseTransformDirection(rb.velocity);
            var projectedVelocity = (goalVelocity.normalized * Vector3.Dot(goalVelocity, currentVelocity)) / goalVelocity.magnitude;

            rb.AddRelativeForce((goalVelocity - projectedVelocity) * forceMove, ForceMode.Force);
        }
    }

    void SoundFootsteps()
    {
        var currentVelocity = rb.velocity;
        currentVelocity = currentVelocity - (Vector3.Dot(currentVelocity, transform.up) * transform.up);

        if (Mathf.Round(currentVelocity.magnitude) > 0)
        {
            audioSourceFootsteps.enabled = true;
        }
        else
        {
            audioSourceFootsteps.enabled = false;
        }

    }

    void RotateCamera()
    {
        // janky fix, set rotation about x axis, but add rotation about y axis
        current_rot_y = mouseInput.x * rotateSpeed * Time.deltaTime;
        current_rot_x -= mouseInput.y * rotateSpeed * Time.deltaTime;

        // clamp to not camera look up too far or too low
        current_rot_x = Mathf.Clamp(current_rot_x, -90, 90);

        this.transform.Rotate(0, current_rot_y, 0, Space.Self);
        
        if (mouseInput.y != 0) {
            mainCamera.transform.localRotation = Quaternion.Euler(Vector3.right * current_rot_x);
        }

    }

    void Jump()
    {
        audioSourceFootsteps.enabled = false;

        rb.AddRelativeForce(forceJump * Vector3.up, ForceMode.VelocityChange);
        StartCoroutine("CameraBobJump");
    }

    void Flip()
    {
        audioSourceFootsteps.enabled = false;

        CalculateRotate();
        CalculateTranslate();

        StartCoroutine("RotateTowards");
        StartCoroutine("TranslateTowards");
        StartCoroutine("CameraLock");
        // FlipVelocity();

        StartCoroutine("PauseWaitResume");
    }

    IEnumerator CameraLock()
    {
        cameraRotate = false;
        yield return new WaitForSecondsRealtime(flipTime);
        cameraRotate = true;
    }

    void CalculateRotate()
    {
        // save old rotation
        oldRot = this.transform.rotation;

        // new up vector is the normal of the wall
        var newUp = dirNextRotate;

        // find the forward angle by taking the signed angle between the direction turning and the current forward based on old up orientation
        var angleNew = Vector3.SignedAngle(-dirNextRotate, transform.forward, transform.up);

        // adjust for shit/third/fourth cartesian planes 
        if (angleNew > 90)
        {
            angleNew = (angleNew - 180);
        }
        else if (angleNew < -90)
        {
            angleNew = (angleNew + 180);
        }
        
        // find new forward
        var newForward = Vector3.Cross(this.transform.right, newUp);

        // adjust new forward, and setup the up orientation
        newRot = Quaternion.AngleAxis(angleNew, newUp) * Quaternion.LookRotation(newForward, newUp);
    }

    void CalculateTranslate()
    {
        oldPos = this.transform.position;
        newPos = oldPos;

        // shoot raycast towards the next wall to flip to
        Vector3 rayCastDir = -dirNextRotate;

        RaycastHit hit;
        Physics.Raycast(this.transform.position, rayCastDir, out hit, Mathf.Infinity, layerMask);

        // see if the raycasted distance is less than the needed space to fit
        var distanceTo = Vector3.Distance(this.transform.position, hit.point);
        if (distanceTo <= (height / 2f))
        {
            newPos += dirNextRotate * ((height / 2f) - distanceTo);
        }
    }

    bool IsJumpable()
    {
        return Physics.CheckCapsule(col.bounds.center, col.bounds.center + (-transform.up * (1.05f)), 0.18f, layerMask);
    }

    IEnumerator PauseWaitResume () 
    {
        Time.timeScale = 0.0f;
        yield return new WaitForSeconds(flipTime * Time.timeScale);
        Time.timeScale = 1.0f;
    }

    IEnumerator RotateTowards()
    {
        float smoothT = 0.0f;
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.fixedUnscaledDeltaTime / flipTime;
            smoothT = (t * t) * (3f - (2f * t)); // smoothing function

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            transform.rotation = Quaternion.Slerp(oldRot, newRot, smoothT);
            yield return null;
        }
    }

    IEnumerator TranslateTowards()
    {
        float smoothT = 0.0f;
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.fixedUnscaledDeltaTime / flipTime;
            smoothT = (t * t) * (3f - (2f * t)); // smoothing function

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            transform.position = Vector3.Lerp(oldPos, newPos, smoothT);
            yield return null;
        }
    }
    
    bool CheckWall()
    {
        ArrayList flippablePoints = new ArrayList();
        // fire ray cast for center
        for (int i = 0; i <= 360; i += 15)
        {
            var radians = Mathf.Deg2Rad * i;    
            var raycastDir = Mathf.Cos(radians) * transform.right + Mathf.Sin(radians) * transform.forward;

            RaycastHit hit;
            Physics.Raycast(this.transform.position, raycastDir, out hit, Mathf.Infinity, layerMask);

            if (Vector3.Distance(hit.point, this.transform.position) < wallFlipDist && !(Mathf.Approximately(Vector3.Dot(transform.up, hit.normal), 1)))
            {
                flippablePoints.Add(hit);
            }
        }

        // fire ray cast for center
        for (int i = 0; i <= 360; i += 15)
        {
            var radians = Mathf.Deg2Rad * i;    
            var raycastDir = Mathf.Cos(radians) * transform.right + Mathf.Sin(radians) * transform.forward;

            RaycastHit hit;
            Physics.Raycast(this.transform.position - (transform.up * (height / 4f)), raycastDir, out hit, Mathf.Infinity, layerMask);

            if (Vector3.Distance(hit.point, this.transform.position - (transform.up * (height / 4f))) < wallFlipDist && !(Mathf.Approximately(Vector3.Dot(transform.up, hit.normal), 1)))
            {
                flippablePoints.Add(hit);
            }
        }

        RaycastHit hitUp;
        Physics.Raycast(this.transform.position, transform.up, out hitUp, Mathf.Infinity, layerMask);
        if (Vector3.Distance(hitUp.point, this.transform.position) < wallFlipDist && !(Mathf.Approximately(Vector3.Dot(transform.up, hitUp.normal), 1)))
        {
            flippablePoints.Add(hitUp);
        }

        if (flippablePoints.Count == 0)
        {
            return false;
        }

        RaycastHit rayCastShortestDist = (RaycastHit)flippablePoints[0];
        for (int i = 1; i < flippablePoints.Count; i++)
        {
            var distRayCastShortest = Vector3.Distance(rayCastShortestDist.point, this.transform.position);
            var distCurrentRayCastCheck = Vector3.Distance(((RaycastHit)flippablePoints[i]).point, this.transform.position);
            if (distCurrentRayCastCheck < distRayCastShortest)
            {
                rayCastShortestDist = (RaycastHit)flippablePoints[i];
            }
        }

        dirNextRotate = rayCastShortestDist.normal;
        return true;
/* 
        // archived because OverlapSphere() does not give collision points

        var hitColliders = Physics.OverlapSphere(this.transform.position, checkFlipRadius, layerMask);

        // sorts by distance
        var sortedColliders = hitColliders.OrderBy(c => {
            return Vector3.Distance(c.transform.position, this.transform.position);
        });

        foreach (Collider c in sortedColliders)
        {
            RaycastHit hit;
            Physics.Raycast(this.transform.position, c.gameObject.transform.position, out hit, Mathf.Infinity, layerMask);
            if (!(Mathf.Approximately(Vector3.Dot(transform.up, hit.normal), 1)))
            {
                dirNextRotate = hit.normal;
                return true;
            }
        }
        return false;
*/

    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(this.transform.position, -this.transform.up, height / 2f + 0.001f, layerMask);
    }

    void Gravity()
    {
        Vector3 gravDirection = -transform.up;
        rb.AddForce(gravDirection * gravitationConstant, ForceMode.Force);
    }

    IEnumerator FlipCooldown()
    {
        canFlip = false;
        yield return new WaitForSecondsRealtime(flipCooldown);
        canFlip = true;
    }

    IEnumerator CameraBobJump()
    {
        Vector3 oldCamPos = mainCamera.transform.localPosition;
        Vector3 newCamPos = oldCamPos - Vector3.up * deltaCameraYJump;

        float smoothT = 0.0f;
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.deltaTime / deltaCameraBobTime;
            smoothT = t*t*t * (t * (6f*t - 15f) + 10f); // smoothing function

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            mainCamera.transform.localPosition = Vector3.Lerp(oldCamPos, newCamPos, smoothT);
            yield return null;
        }
        mainCamera.transform.localPosition = newCamPos;
        
        smoothT = 0f; 
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.deltaTime / deltaCameraBobTime;
            smoothT = t*t*t * (t * (6f*t - 15f) + 10f);
            // smoothT = (t * t) * (3f - (2f * t)); // smoothing function

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            mainCamera.transform.localPosition = Vector3.Lerp(newCamPos, oldCamPos, smoothT);
            yield return null;
        }

        mainCamera.transform.localPosition = oldCamPos;
    }

    IEnumerator CameraBobFalling()
    {
        canJump = false;
        Vector3 oldCamPos = mainCamera.transform.localPosition;
        Vector3 newCamPos = oldCamPos - Vector3.up * (fallVelocity / deltaCameraFallConstant);

        float smoothT = 0.0f;
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.deltaTime / deltaCameraFallTime;
            smoothT = ((t - 1) * (t - 1) * (t - 1)) + 1; // smoothing function

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            mainCamera.transform.localPosition = Vector3.Lerp(oldCamPos, newCamPos, smoothT);
            yield return null;
        }
        mainCamera.transform.localPosition = newCamPos;
        
        for (float t = 0.0f; t < 1.0f;)
        {
            t += Time.deltaTime / deltaCameraFallTime;
            smoothT = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            if (smoothT > 1.0f) 
            {
                smoothT = 1.0f;
            }

            mainCamera.transform.localPosition = Vector3.Lerp(newCamPos, oldCamPos, smoothT);
            yield return null;
        
        }

        mainCamera.transform.localPosition = oldCamPos;
        canJump = true;
    }

}
