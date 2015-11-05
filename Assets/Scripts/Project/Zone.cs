using UnityEngine;
using System.Collections;

public class Zone : MonoBehaviour {

    public enum Slot {        S1 = 1, S2, S3, S4,
                       P1_SUPPLY = 11, P1_SUPPORT, P1_SERVICABLE, P1_SUMMONED, P1_SPENT, P1_STORED,
                       P2_SUPPLY = 21, P2_SUPPORT, P2_SERVICABLE, P2_SUMMONED, P2_SPENT, P2_STORED };



    public Slot typeOfSlot;
    

    public string zoneName;
	public int numberItems;

    public bool checkOwnColor;

    public Transform diceHolder;
    public Transform zoneHolder;


	public Zone(Transform z) {
		zoneName = z.name;
        zoneHolder = z;
    }

    void Start()
    {
        if (zoneHolder != null)
            zoneName = zoneHolder.name;
    }


    // Update is called once per frame
    void Update () {

        if (checkOwnColor)
        {
            checkOwnColor = false;
            if (GameLogic.instance.isValidDrop(this))
            {
                //Zone dZ = 
                //Debug.Log("Zone: " + dZ.name);
                if (GameLogic.instance.getDraggedPlaceholder() == this)
                    this.GetComponent<SpriteRenderer>().color = Color.green;
                else
                    this.GetComponent<SpriteRenderer>().color = Color.yellow;

                return;
            }

            this.GetComponent<SpriteRenderer>().color = Color.white;
            
        }
    }


    // HARD: Hard code knowing child of zone will be 'Die'
    public Die[] getZoneDice()
    {
        return diceHolder.GetComponentsInChildren<Die>();
    }
}



/*

If this zone is the draggedObject's originalParent 
    green
    return

If (GameLogic.instance.isValidDrop(this))
    yellow
    return


white



isValidDrop(Zone z) {
    // know draggedObject
    //      know original zone/area
    //      if is a Die, know side
    // know current step

    switch (step)
        case 1
            break
        case 2
        case 3
        case 4

            if (originalArea == Stall && 
                (z.id == 6 + (currentPlayerID * 10)))
                    return true
        case 5
        case 6
        
    return false
        
        

}


*/
