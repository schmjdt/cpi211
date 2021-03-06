﻿using UnityEngine;

/*
 Thanks to Quill (http://quill18.com/unity_tutorials/) and his Drag & Drop tutorials for the idea!
 */

public class Draggable : MonoBehaviour {

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
    public Transform originalParent = null;      // XXXHolder where drag came from: dieHolder, cardHolder, tokenHolder
    public Zone zonePlaceholder = null;   // Zone dragged over:
    
    /*
    zoneHolder
    */

    public Transform dragArea;
    //public Transform lastParent = null;
    
    RaycastHit hit;
    Zone zoneHit;
    // -------------------------------------------------
    
    void Start()
    {
        originalParent = this.transform.parent;
        zonePlaceholder = this.transform.parent.parent.GetComponentInChildren<Zone>();
        
        //originalZoneHolder = placeHolderParent.holders.zoneHolder;

    }

    void Update()
    {
        //Debug.DrawLine(transform.position, new Vector3(transform.position.x, transform.position.y - 10000f, transform.position.z));
    }

    void OnMouseDown()
    {
        canDrag = GameLogic.instance.isValidDrag(this);
        if (!canDrag) return;
           
        if (hideCursorOnDrag) Cursor.visible = false;
        GetComponent<Rigidbody>().isKinematic = true;

        #region Save Positions

        lastPositX = getPosition(directionX);
        lastPositY = getPosition(directionY);
        lastPositZ = getPosition(directionZ);

        originalPosition = transform.position;

        #endregion

        #region Set Parents

        originalParent = this.transform.parent;
        zonePlaceholder = getParentZone();
        this.transform.SetParent(dragArea);

        #endregion

        #region Lift Object

        liftOffset = lift * transform.localScale.y / offset;

        transform.position = new Vector3(transform.position.x,
                                         transform.position.y + liftOffset,
                                         transform.position.z);

        #endregion


        //GetComponent<CanvasGroup>().blocksRaycasts = false; 

        // CHANGE: Put on GameLogic's isValidDrag
        //GameLogic.instance.gameLayout.checkZoneColor();

    }

    public void OnMouseDrag()
    {
        if (!canDrag) return;
        
        changeX = -(lastPositX - getPosition(directionX)) * transform.localScale.x / offset * CameraControl.playerCameraOffset;
        changeY = (lastPositY - getPosition(directionY)) * transform.localScale.y / offset * CameraControl.playerCameraOffset;
        changeZ = -(lastPositZ - getPosition(directionZ)) * transform.localScale.z / offset * CameraControl.playerCameraOffset;


        transform.position = new Vector3(transform.position.x + (changeX),
                                            transform.position.y + (changeY),
                                            transform.position.z + (changeZ));
        

        /*
        Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(transform.position));
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log("Camera Ray = " + hit.collider.name);
        }
        */


        if (Physics.Raycast(transform.position, new Vector3(0, -transform.position.y, 0), out hit))
        {
            Debug.Log("Own Ray: " + hit.transform.name);
            zoneHit = hit.transform.GetComponent<Zone>();
            if (zoneHit != null)
            {
                if (GameLogic.instance.isValidDrop(zoneHit))
                {
                    zonePlaceholder = zoneHit;
                    //zoneHit.checkOwnColor = true;  // checkZoneColor() code below does this for now
                }
            }
            else
                zonePlaceholder = getParentZone();
            // NOTE:  Find better way to change just the last zone checked!
            GameLogic.instance.gameLayout.checkZoneColor(); 
        }
        
        lastPositX = getPosition(directionX);
        lastPositY = getPosition(directionY);
        lastPositZ = getPosition(directionZ);
    }

    void OnMouseUp()
    {
        if (!canDrag) return;
        Cursor.visible = true;
        GetComponent<Rigidbody>().isKinematic = false;

        Zone previousZone = null;
        if (getParentZone() != zonePlaceholder)
        {
            previousZone = getParentZone();
            this.originalParent = zonePlaceholder.holders.diceHolder;
            transform.position = new Vector3(transform.position.x,
                                             transform.position.y - liftOffset,
                                             transform.position.z);
            
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

        GameLogic.instance.dieDropped(previousZone);
    }

    public void setPlaceHolderParent()
    {

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

    // HARD: blarg...  die knows zoneHolder too ??? meh
    public Zone getParentZone() { return originalParent.parent.GetComponentInChildren<Zone>(); }
    //public Zone getParentZone() { return zonePlaceholder; }
    public Transform getParentArea() { return originalParent.parent.parent; }

    public void setParentZone(Zone z)
    {
        zonePlaceholder = z;
        originalParent = z.holders.diceHolder;
    }
       
    
}


public class Draggable_Die : Draggable
{
}

public class Draggable_Card : Draggable
{
}

public class Draggable_Token : Draggable
{
}