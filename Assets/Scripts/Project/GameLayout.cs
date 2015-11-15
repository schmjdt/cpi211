using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class GameLayout
{
    public GameArea[] gameAreas;
    
    public Transform scorePositions;
    public Transform[] scoreTokens;
    public Transform[] positScores;


    public void initLayout()
    {
        GameObject[] areas = GameObject.FindGameObjectsWithTag(CommonLogic.TAG_HOLDER_AREA);

        gameAreas = new GameArea[areas.Length];

        for (int i = 0; i < gameAreas.Length; i++)
        {
            gameAreas[i] = new GameArea(areas[i].transform);
        }
        /*
        foreach (GameObject item in areas)
        {
            item.initZones();
        }
        */
        // Init Scoreboard
        positScores = scorePositions.GetComponentsInChildren<Transform>();
    }

    public Zone clickedZone()
    {
        //bool b = false;
        Zone z = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            z = hit.collider.transform.GetComponent<Zone>();
        }

        return z;
        //return b;
    }


    public Card getCard(string s)
    {
        // Returns first card
        return getGameZone(s).cards[0];
    }

    public GameZone getGameZone(string s) { return getGameZone(getZone(s)); }

    public GameZone getGameZone(Zone z)
    {
        if (z == null) return null;

        GameZone rZone = null;
        foreach (GameArea area in gameAreas)
        {
            foreach (GameZone zone in area.areaZones)
            {
                if (zone.zone == z)
                    rZone = zone;
            }
        }
        return rZone;
    }


    public string checkAreaClick() { return checkAreaClick(clickedZone()); }

    public string checkAreaClick(Zone z)
    {
        string rString = "";

        GameZone zone = getGameZone(z);

        if (zone != null)
            rString = zone.getAreaT().name;

        return rString;
    }


    public void checkZoneColor()
    {
        foreach (GameArea gA in gameAreas)
        {
            foreach (GameZone gZ in gA.areaZones)
            {
                gZ.checkZoneColor();
            }
        }
    }

    public GameArea getArea(string s)
    {
        for (int i = 0; i < gameAreas.Length; i++)
        {
            if (s == gameAreas[i].getName()) return gameAreas[i];
        }
        return null;
    }

    public Zone getZone(string s)
    {
        if (!s.Contains("/"))
            s = GameLogic.instance.getCurrentZoneS(s);

        string[] ss = s.Split('/');

        GameArea gA = getArea(ss[0]);

        foreach (GameZone zone in gA.areaZones)
        {
            if (zone.zoneName == ss[1])
                return zone.zone;
        }
        return null;
    }

    public void gatherDice()
    {
        foreach (GameArea area in gameAreas)
            foreach (GameZone zone in area.areaZones)
                zone.gatherDice();
    }

    public Die[] getDiceInZone(int a, int z)
    {
        // Update to the current dice in Zone
        gameAreas[a].areaZones[z].gatherDice();
        // Return the current dice in Zone
        return gameAreas[a].areaZones[z].dice;
    }

    public Die[] getDiceInZone(string a, string z)
    {
        return getDiceInZone(a + "/" + z);
    }

    public Die[] getDiceInZone(Zone z)
    {
        return getGameZone(z).dice;
    }


    public Die[] getDiceInZoneNext(string z)
    {
        return getDiceInZone("area" + GameLogic.instance.playerLogic.nextPlayer().getPlayerName() + "/" + z);
    }

    public Die[] getDiceInZone(string z)
    {
        Die[] rDice = null;
        Zone zone = getZone(z);
        if (zone != null)
        {
            GameZone gZ = getGameZone(zone);
            if (gZ != null)
            {
                // NOTE:  Find better spot for this?
                //      Only update when KNOW there was a CHANGE
                //      This updates EVERY TIME just get list [not saving anything]
                //          DieLogic.moveDie()
                gZ.gatherDice();
                rDice = gZ.dice;
            }
        }
        return rDice;
    }

    public bool isDiceInZone(string z)
    {
        bool rBool;

        Die[] d = getDiceInZone(z);

        if (d == null || d.Length == 0) rBool = false;
        else rBool = true;

        return rBool;
    }

    /*
    public Die[] getDiceInZone(string a, string z)
    {
        string obj = a + "/";
        if (obj == "/") obj = "";
        obj = z;
        Die[] dice = null;
        GameObject zone = GameObject.Find(obj);
        if (zone != null)
        {
            dice = zone.GetComponentsInChildren<Die>();
        }
        return dice;
    }
    */

    public GameObject getZoneDiceParent(string z)
    {
        return getZoneDiceParent("", z);
    }

    public GameObject getZoneDiceParent(string a, string z)
    {
        GameArea gA = getArea(a);
        if (gA != null)
        {
            GameZone gZ = gA.getZone(z);
            if (gZ != null)
                return gZ.getDiceT().gameObject;
        }
        return null;
        
        /*
        string obj = a + "/";
        if (obj == "/") obj = "";
        obj = z + "/Dice";
        return GameObject.Find(obj);
        */
    }
}



[System.Serializable]
public class GameArea
{
    public enum eAreaType { PLAYER, MARKET, OTHER };

    public string areaName;
    public Transform areaHolder;
    public List<GameZone> areaZones = new List<GameZone>();
    public eAreaType areaType;

    public GameArea(Transform t)
    {
        Debug.Log("GameZone " + t.name + " starting");
        areaHolder = t;
        areaName = t.name;

        switch (areaName.Substring(4, 1))
        {
            case "P":
                areaType = eAreaType.PLAYER;
                break;
            case "M":
                areaType = eAreaType.MARKET;
                break;
            default:
                areaType = eAreaType.OTHER;
                break;
        }

        Debug.Log("GameArea " + t.name + " created");
        createZones();
    }

    public string getName() { return areaName; }

    public GameZone getZone(string s)
    {
        for (int i = 0; i < areaZones.Count; i++)
        {
            if (s == areaZones[i].getName()) return areaZones[i];
        }
        return null;
    }
    
    public void initZones()
    {
        areaName = areaHolder.name;
        createZones();
    }

    public void createZones()
    {
        Transform[] zones = LayoutLogic.getSpecificTransformsTag(CommonLogic.TAG_HOLDER_ZONE, areaHolder);
        //GameObject[] zones = GameObject.FindGameObjectsWithTag("ZoneHolder");
        
        for (int i = 0; i < zones.Length; i++)
        {
            areaZones.Add(new GameZone(areaHolder, zones[i]));
        }
    }

// createArea via code (instead of Unity drag-drop)
#if false
    void createArea()
    {
        string[] zones = { };
        switch (areaType)
        {
            case eAreaType.PLAYER:
                areaZones = new GameZone[6];
                zones = "zoneSupport,zoneServicable,zoneSummoned,zoneSpent,zoneStored,zoneSupply".Split(',');
                break;
            case eAreaType.MARKET:
                areaZones = new GameZone[3];
                zones = "zoneCard1,zoneCard2,zoneCard3".Split(',');
                break;
        }

        for (int i = 0; i < areaZones.Length; i++)
        {
            areaZones[i] = new GameZone(zones[i]);
        }
    }
#endif

}

[System.Serializable]
public class GameZone
{
    public string zoneName;
    public Zone zone;
    public Die[] dice;
    public List<Card> cards = new List<Card>();
    //public Card[] cards;
    //public Token[] tokens;
    
    public Holders holders = new Holders();

    public GameZone(Transform a, Transform z)
    {
        Debug.Log("GameZone " + a.name + "/" + z.name + " starting");

        zoneName = z.name;
        zone = z.GetComponentInChildren<Zone>();

        holders.setMainHolders(a, z);
        
        holders.setChildHolders(
                                LayoutLogic.getSpecificTransformTag(CommonLogic.TAG_HOLDER_DICE,  z),
                                LayoutLogic.getSpecificTransformTag(CommonLogic.TAG_HOLDER_CARD,  z),
                                LayoutLogic.getSpecificTransformTag(CommonLogic.TAG_HOLDER_TOKEN, z));

        zone.holders = holders;
        zone.zoneName = holders.zoneHolder.name;

        checkIfCard();
        Debug.Log("GameZone " + a.name + "/" + z.name + " created");
    }

    public string getName() { return zoneName; }
    public Transform getDiceT() { return holders.diceHolder; }
    public Transform getCardT() { return holders.cardHolder; }
    public Transform getAreaT() { return holders.areaHolder; }


    public Card getCard() { return getCard(0); }
    public Card getCard(int i) { return cards[i]; }
    //public Transform getTokenT() { return tokenHolder; }
    

    public void gatherDice()
    {
        this.dice = getDiceT().GetComponentsInChildren<Die>();
    }

    public void checkZoneColor()
    {
        this.zone.checkOwnColor = true;
    }

    public void checkIfCard()
    {
        if (getCardT() == null || getCardT().childCount == 0)
            return;

        Card card = getCardT().GetComponentInChildren<Card>();
        if (card)
        {
            card.initCard();
            cards.Add(card);

            for (int i = 0; i < card.totalDice; i++)
            {
                GameLogic.instance.instantiateDie(getDiceT());
            }

            gatherDice();

            foreach (Die die in dice)
            {
                die.setCard(card);
            }
        }
        else
        {
            gatherDice();
        }
    }
}

[System.Serializable]
public class Holders
{
    public Transform areaHolder;
    public Transform zoneHolder;  // parent to all below
    public Transform diceHolder;  // die parent
    public Transform cardHolder;
    public Transform tokenHolder;

    public void setMainHolders(Transform a, Transform z)
    {
        areaHolder = a;
        zoneHolder = z;
    }

    public void setChildHolders(Transform d, Transform c, Transform t)
    {
        diceHolder = d;
        cardHolder = c;
        tokenHolder = t;
    }
}



[System.Serializable]
public class CardLayout : MeshLayout
{
    //public RawImage imgCard;
    //public Text txtCard;
    public GameObject pnlCard;

    public List<Texture2D> texCards = new List<Texture2D>();


    public void toggleImage(bool b)
    {
        toggleImage(b, null, "");
    }

    public void toggleImage(bool b, Texture i, string s)
    {
        pnlCard.SetActive(b);
        if (b)
        {
            setImage(i);
            setText(s);
        }
    }

    // HARD: Assums there is a panel (gameobject) with a rawimage and text for the card
    public void setImage(Texture img)
    {
        pnlCard.GetComponentInChildren<RawImage>().texture = img;
    }

    public void setText(string s)
    {
        pnlCard.GetComponentInChildren<Text>().text = s;
    }
}

public struct LayoutLogic
{

    public static List<gameTag> gameObjects = new List<gameTag>();

    public struct gameTag
    {
        public string tagName;
        public GameObject[] tagObjects;

        
        public gameTag(string tag) {
            tagName = tag;
            tagObjects = GameObject.FindGameObjectsWithTag(tag);
        }

        public void createObjects(string tag)
        {
            tagObjects = GameObject.FindGameObjectsWithTag(tag);
        }
    }

    static GameObject[] newTagObject(string tag)
    {
        gameTag gT = new gameTag(tag);
        gameObjects.Add(gT);
        return gT.tagObjects;
    }

    static GameObject[] checkTagged(string tag)
    {
        foreach (gameTag item in gameObjects)
        {
            if (item.tagName == tag)
                return item.tagObjects;
        }
        return null;
    }


    public static Transform getSpecificTransformTag(string tag, Transform p)
    {
        GameObject[] dT = checkTagged(tag);
        if (dT == null)
            dT = newTagObject(tag);

        
        foreach (GameObject item in dT)
        {
            if (item.transform.IsChildOf(p))
                return item.transform;
        }
        return null;
        
    }

    public static Transform[] getSpecificTransformsTag(string tag, Transform p)
    {
        List<Transform> t = new List<Transform>();
        GameObject[] dT = checkTagged(tag);
        if (dT == null)
            dT = newTagObject(tag);

        foreach (GameObject item in dT)
        {
            if (item.transform.IsChildOf(p))
                t.Add(item.transform);
        }

        return t.ToArray();
    }
}