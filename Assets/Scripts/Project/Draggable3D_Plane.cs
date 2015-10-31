using UnityEngine;

public class Draggable3D_Plane : MonoBehaviour {

    // Dragging Vars -----------------------------------
    public enum eDirection { X, Y, Z, STATIC };

    public eDirection directionX, directionY, directionZ;
    public float offset = 1500, lift = 20;

    public bool hideCursorOnDrag = false;
    public bool canDrag;


    float lastPositX, lastPositY, lastPositZ;
    float changeX, changeY, changeZ, liftOffset;
    Vector3 originalPosition;
    // -------------------------------------------------

    // Zone Vars ---------------------------------------
    public Transform originalParent = null;
    public Transform placeHolderParent = null;

    //public Transform lastParent = null;

    public Zone.Slot typeOfSlot;

    RaycastHit hit;
    Zone zoneHit;
    // -------------------------------------------------

    void Update()
    {

        Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 10000f, transform.position.z));
    }

    void OnMouseDown()
    {
        canDrag = GameLogic.instance.isValidDrag(this);
        if (!canDrag) return;
           
        if (hideCursorOnDrag) Cursor.visible = false;



        lastPositX = getPosition(directionX);
        lastPositY = getPosition(directionY);
        lastPositZ = getPosition(directionZ);

        
        liftOffset = lift * transform.localScale.y / offset;

        originalPosition = transform.position;
        transform.position = new Vector3(transform.position.x,
                                         transform.position.y + liftOffset,
                                         transform.position.z);

        originalParent = this.transform.parent;
        placeHolderParent = originalParent.parent.Find("zoneOutline");
        this.transform.SetParent(this.transform.parent.parent.parent.parent);

        //GetComponent<CanvasGroup>().blocksRaycasts = false; 
        GetComponent<Rigidbody>().isKinematic = true;
        GameLogic.instance.gameLayout.checkZoneColor();
    }

    public void OnMouseDrag()
    {
        if (!canDrag) return;
        //OnDrag(eventData); 

        changeX = -(lastPositX - getPosition(directionX)) * transform.localScale.x / offset * CameraControl.playerCameraOffset;
        changeY = (lastPositY - getPosition(directionY)) * transform.localScale.y / offset * CameraControl.playerCameraOffset;
        changeZ = -(lastPositZ - getPosition(directionZ)) * transform.localScale.z / offset * CameraControl.playerCameraOffset;


        transform.position = new Vector3(transform.position.x + (changeX),
                                            transform.position.y + (changeY),
                                            transform.position.z + (changeZ));

        //transform.Translate(changeX, changeY, changeZ);


        Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(transform.position));
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Camera Ray = " + hit.collider.name);
        }



        if (Physics.Raycast(transform.position, new Vector3(0, -transform.position.y, 0), out hit))
        {
            Debug.Log("Own Ray: " + hit.transform.name);
            zoneHit = hit.transform.GetComponent<Zone>();
            if (zoneHit != null)
            {
                //if (this.typeOfSlot == zoneHit.typeOfSlot)
                if (GameLogic.instance.isValidDrop(zoneHit))
                {
                    placeHolderParent = zoneHit.transform;
                }
            }
            else
                placeHolderParent = originalParent.parent.Find("zoneOutline");
            GameLogic.instance.gameLayout.checkZoneColor();
        }

        //lastPositX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x; 
        //lastPositY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        lastPositX = getPosition(directionX);
        lastPositY = getPosition(directionY);
        lastPositZ = getPosition(directionZ);
    }

    void OnMouseUp()
    {
        if (!canDrag) return;
        Cursor.visible = true;

        if (originalParent.parent.Find("zoneOutline") != placeHolderParent)
        {
            // Either the object must match the drop zone OR (IE: weapon slot) 
            //      drop zone is of certain type that allows all objects (IE: discard)
            //Zone p = placeHolderParent.GetComponent<Zone>(); 
            //if (p.typeOfSlot == this.typeOfSlot || p.typeOfSlot == Draggable.Slot.DISCARD)

            this.originalParent = placeHolderParent.parent.Find("Dice");
            transform.position = new Vector3(transform.position.x,
                                                transform.position.y - liftOffset,
                                                transform.position.z);
            //transform.position = new Vector3(0f, .5f, 0f);
        }
        else
        {
            this.transform.position = originalPosition;
            originalPosition = transform.position;
        }

        this.transform.SetParent(originalParent);
        Debug.Log(this.name + " was dropped on " + originalParent.name);
        //GetComponent<CanvasGroup>().blocksRaycasts = true; 

        canDrag = false;
        GetComponent<Rigidbody>().isKinematic = false;

        GameLogic.instance.dieDropped();
    }


    float getPosition(eDirection d)
    {
        switch (d)
        {
            case eDirection.X:
                return Input.mousePosition.x;
            case eDirection.Y:
                return Input.mousePosition.y;
            case eDirection.Z:
                return Input.mousePosition.z;
            default:
                return 0.0f;
        }
    }

    public Zone getParentZone() { return originalParent.parent.gameObject.GetComponentInChildren<Zone>(); }
    public GameObject getParentArea() { return originalParent.parent.parent.gameObject; }
}
