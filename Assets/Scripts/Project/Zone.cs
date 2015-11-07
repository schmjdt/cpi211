using UnityEngine;
using System.Collections;

public class Zone : MonoBehaviour {
    public string zoneName;
	public int numberItems;

    public bool checkOwnColor;

    public Holders holders;

    public Zone(Transform z) {
		zoneName = z.name;
    }

    void Start()
    {
        if (holders.zoneHolder != null)
            zoneName = holders.zoneHolder.name;
    }
    
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
