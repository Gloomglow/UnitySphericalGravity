using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlFP : MonoBehaviour
{
    //Camera Rotation// 
    static float mouseSensitivityX = 2f;
    static float mouseSensitivityY = 2f;
    private float verticalLookRotation;

    //Controller Movement//
    public float moveSpeed;
    public float sprintSpeed;
    public float jumpForce; 
    private bool grounded;
    private float currentMoveSpeed;
    public float currentOrbitGravity; 

    //Gravity//
    public float hoverGravity;
    public float orbitGravity; 

    //Objects & Components//
    public Collider col;
    public Rigidbody rb;
    public GameObject myCamera;
    public GameObject orbitPoint;
    public LayerMask globeMask;
    public GameObject activeBuilding;

    //Enums// 
    public GravityMode GMD;
    public GravityType GTP;
    public enum GravityMode { Grounded, Floating, FloatToGround }
    public enum GravityType { Flat, Rounded }

    //Core Functions// 
    public void CameraRotation()
    {
        //Basic Camera Controls//
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
        verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -60, 60);
        myCamera.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }
    public void ControllerMovement()
    {
        if (GMD == GravityMode.Grounded)
        {
            //Sprinting// 
            if (Input.GetButton("Shift")){ currentMoveSpeed = sprintSpeed; } else { currentMoveSpeed = moveSpeed; }

            //Jumping// 
            float addJumpForce = 0f; 
            if (Input.GetButton("Jump")){ addJumpForce = jumpForce; }

            //Final Movement//
            Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal") * currentMoveSpeed * Time.deltaTime, addJumpForce * Time.deltaTime, Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime);
            transform.Translate(moveDir * currentMoveSpeed);
        }
    }
    public void OrbitalPull()
    {
        //Rotate Controller To Center// 
        Vector3 targetDir = (gameObject.transform.position - orbitPoint.transform.position).normalized;

        //Normal Faux Movement Applies// 
        if (GMD == GravityMode.Grounded)
        {
            //Ray From Bottom of Controller//
            RaycastHit hit; 
            bool foundGround = Physics.Raycast(gameObject.transform.position, -gameObject.transform.up, out hit, Mathf.Infinity, globeMask);

            //--------Flat Surface vs Round Surface Gravity--------//

            //Are you aligned with a planet? 
            if(foundGround == true)
            {
                //Bind to a flat surface// 
                if (hit.collider.tag == "FlatSurface")
                {
                    GTP = GravityType.Flat;
                    activeBuilding = hit.collider.gameObject;
                }
                //Normal Spherical Gravity// 
                else if (hit.collider.tag != "FlatSurface")
                {
                    GTP = GravityType.Rounded;
                    activeBuilding = null;
                }
            }
            //Not Aligned. Go to Spherical Gravity// 
            else
            {
                GTP = GravityType.Rounded;
                activeBuilding = null;
            }

            //----------------Rotate Towards Orbit Point--------------//

            if (GTP == GravityType.Flat)
            {
                gameObject.transform.rotation = Quaternion.FromToRotation(gameObject.transform.up, activeBuilding.transform.up) * gameObject.transform.rotation;
            }
            else if(GTP == GravityType.Rounded)
            {
                gameObject.transform.rotation = Quaternion.FromToRotation(gameObject.transform.up, targetDir) * gameObject.transform.rotation;
            }

            //Move Towards Orbit Point// 
            transform.Translate(new Vector3(0, -hoverGravity * Time.deltaTime, 0));

        }

        //Move Towards A new planet -- No extra rotations//
        else if (GMD == GravityMode.Floating)
        {
            transform.Translate
            (
                (-targetDir.x * Time.deltaTime * orbitGravity),
                (-targetDir.y * Time.deltaTime * orbitGravity),
                (-targetDir.z * Time.deltaTime * orbitGravity), Space.World
            );
        }
        
        //About to hit ground -- Rotate towards Planet// 
        else if (GMD == GravityMode.FloatToGround)
        {
            gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, Quaternion.FromToRotation(gameObject.transform.up, targetDir) * gameObject.transform.rotation, 1f * Time.deltaTime);
            transform.Translate(new Vector3(0, -hoverGravity * Time.deltaTime, 0));
        }
    }
    public void PlanetTransfer()
    {
        //If On Solid Ground, Can change planets// 
        if (Input.GetKeyDown(KeyCode.Alpha1) && GMD == GravityMode.Grounded)
        {
            //Shoot a Raycast forward (from center of camera)// 
            RaycastHit hit;
            Debug.DrawRay(myCamera.transform.position, myCamera.transform.forward * 1000f, Color.red, 2f);

            //Note - "globe mask" is used to avoid hitting trees and things on the planet// 
            if (Physics.Raycast(myCamera.transform.position, myCamera.transform.forward, out hit, Mathf.Infinity, globeMask))
            {
                if (hit.collider.tag == "Globe")
                {
                    //Hit a planet. Switch your orbit point//
                    orbitPoint = hit.collider.gameObject;

                    //Ghost mode (A coroutine that handles smooth accelleration and deceleration towards planet)// 
                    StartCoroutine(GhostMode());
                }
            }
        }
    }

    public IEnumerator GhostMode()
    {
        //Was Grounded. Begin to Float.// 
        if (GMD == GravityMode.Grounded)
        {
            GMD = GravityMode.Floating;

            //Accellerate Towards Planet// 
            currentOrbitGravity = 0;
            float planetRadius = orbitPoint.transform.localScale.y;
            float planetAtmosphere = orbitPoint.transform.localScale.y * 2.5f;

            //Go to maximum gravity//
            while (Vector3.Distance(orbitPoint.transform.position, gameObject.transform.position) > (planetRadius + planetAtmosphere))
            {
                yield return new WaitForSeconds(.1f);
                if (currentOrbitGravity < orbitGravity)
                {
                    currentOrbitGravity += (orbitGravity * 0.1f);
                }
            }

            //Slow Down On Approach//
            for (int i = 0; i < 20; i++)
            {
                if (Vector3.Distance(orbitPoint.transform.position, gameObject.transform.position) < (planetRadius + 2))
                {
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(0.1f);
                    currentOrbitGravity -= (orbitGravity * 0.05f);
                }

            }

            //Begin Rotating// 
            GMD = GravityMode.FloatToGround;
            yield return new WaitForSeconds(3f);

            //Grounded. Normal gravity takes over// 
            GMD = GravityMode.Grounded;
        }
        else
        {
            Debug.Log("Error, starting ghost mode not grounded!");
        }
        
    }

    //Runtime Functions// 
    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        GMD = GravityMode.Grounded;
        grounded = true;
    }
    private void Update()
    {
        CameraRotation();
        ControllerMovement();
        PlanetTransfer();
        OrbitalPull();
    }
    private void FixedUpdate()
    {
        //Null out rigidbody physics//
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}

