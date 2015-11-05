using UnityEngine;
using System.Collections;

public class CameraControl : MonoBehaviour
{
    //public static CameraControl instance;
    // Player Camera 
    public Camera playerCamera;
	public GameObject[] camPlayerPositions;
	public int currentCamPlayerPosition;

    public static int playerCameraOffset = 1;

    private GameObject nextCameraPosition = null, startCameraPosition = null;

    public int cameraFovMax = 60;
    public float cameraMovementSpeed = 0.8f;
    public float cameraMovement = 0;

    Quaternion origRot;
    Vector3 origPot;
    bool zooming, usedButton, followingDragged;

    float count = 0, lastValue = 0;

    public KeyCode zoomKey;

    void Awake()
    {
        //instance = this;
    }

    // Use this for initialization
    void Start ()
    {
        origPot = transform.position;
        origRot = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKey(KeyCode.LeftControl))
        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetMouseButton(3) || Input.GetKey(zoomKey))
        {
            if (!zooming)
            {
                zooming = true;
                origPot = transform.position;
                origRot = transform.rotation;


                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.GetComponent<Zone>() || hit.transform.GetComponent<Die>())
                        // Depending on which zone mouse is hovered on
                        /*
                        if (hit.transform.name == "Player1" ||
                            hit.transform.name == "Player2" ||
                            hit.transform.name == "Market") */
                        transform.LookAt(hit.transform);
                    //transform.LookAt(GameObject.Find("Player2").transform);
                    //transform.LookAt(GameObject.Find("Market").transform);
                }
            }


            if (playerCamera.fieldOfView > 10)
                playerCamera.fieldOfView -= 1;

            if (usedButton)
                usedButton = false;

            if (Input.GetMouseButton(3) || Input.GetKey(zoomKey))
                usedButton = true;
            
        }
        //else if (Input.GetKey(KeyCode.RightControl))
        else if (Input.GetKeyUp(KeyCode.End) && zooming)
        {
            resetCamPosition();
        }
        else if (Input.GetKeyUp(KeyCode.Home))
        {
            playerCamera.fieldOfView = 10;
            zooming = true;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 || usedButton)
        {
            if (playerCamera.fieldOfView < cameraFovMax)
                playerCamera.fieldOfView += 1;

            //playerCamera.transform
        }
        /*
        else if (Input.GetMouseButton(3))
        {
            if (playerCamera.fieldOfView > 10)
                playerCamera.fieldOfView -= 1;
        }
        */

        if (playerCamera.fieldOfView == cameraFovMax && zooming)
        {
            resetCamPosition();
        }

        if (nextCameraPosition != null)
            MoveCamera();

        if (GameLogic.instance.draggedObject != null && playerCamera.fieldOfView == 10)
        {
            followingDragged = true;
            transform.LookAt(GameLogic.instance.draggedObject.transform);
        }
        else
        {
            if (followingDragged)
                resetCamPosition();
            followingDragged = false;
        }
    }

    public void resetCamPosition()
    {
        usedButton = false;
        zooming = false;
        transform.position = origPot;
        transform.rotation = origRot;
        playerCamera.fieldOfView = cameraFovMax;  // sanity check
    }

    

	public void togglePlayerCam()
    {
        resetCamPosition();

        startCameraPosition = camPlayerPositions[currentCamPlayerPosition];
        currentCamPlayerPosition++;
		if (currentCamPlayerPosition == camPlayerPositions.Length)
			currentCamPlayerPosition = 0;
        nextCameraPosition = camPlayerPositions[currentCamPlayerPosition];

        /*
        transform.position = camPlayerPositions[currentCamPlayerPosition].transform.position;		
		transform.rotation = camPlayerPositions[currentCamPlayerPosition].transform.rotation;
        */


        playerCameraOffset *= -1;  // Fixes reverse of X & Z axis

    }

    // Moving the camera
    void MoveCamera()
    {
        // increment total moving time
        cameraMovement += Time.deltaTime * 1;
        // if we surpass the speed we have to cap the movement because we are 'slerping'
        if (cameraMovement > cameraMovementSpeed)
            cameraMovement = cameraMovementSpeed;

        // slerp (circular interpolation) the position between start and next camera position
        Camera.main.transform.position = Vector3.Slerp(startCameraPosition.transform.position, nextCameraPosition.transform.position, cameraMovement / cameraMovementSpeed);
        // slerp (circular interpolation) the rotation between start and next camera rotation
        Camera.main.transform.rotation = Quaternion.Slerp(startCameraPosition.transform.rotation, nextCameraPosition.transform.rotation, cameraMovement / cameraMovementSpeed);

        // stop moving if we arrived at the desired next camera postion
        if (cameraMovement == cameraMovementSpeed)
        {
            nextCameraPosition = null;

            origPot = transform.position;
            origRot = transform.rotation;

            cameraMovement = 0;
        }
    }
    
}



